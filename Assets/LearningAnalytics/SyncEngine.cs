using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// Motor de sincronização com o servidor central (Supabase), independente do Unity para poder
/// ser testado com um servidor falso.
///
/// Princípios:
///  - Os arquivos locais (events_*.jsonl, affect_*.csv) são a fonte da verdade e NUNCA são apagados.
///  - Envia em lotes só o que o servidor ainda não confirmou. O servidor responde "recebi até o
///    seq N" (eventos) ou "até a amostra N" (faciais); o progresso fica em .sync_state.tsv.
///  - Reenviar é seguro: o servidor ignora eventos repetidos (pelo id) e amostras repetidas.
///  - Sem estado local (ex.: jogo reinstalado), pergunta ao servidor até onde já recebeu.
///  - Linhas incompletas (sendo gravadas no momento) ficam para a próxima rodada.
/// </summary>
public class SyncEngine
{
    /// <summary>Faz uma chamada RPC; devolve (ok, código HTTP, corpo da resposta).</summary>
    public delegate (bool ok, int status, string body) Transport(string rpc, string jsonBody);

    public const string StateFile = ".sync_state.tsv";

    public string Folder { get; }
    public string StudyId { get; }
    public string UploadKey { get; }
    public int EventBatch { get; set; } = 500;
    public int AffectBatch { get; set; } = 300;

    private readonly Transport _transport;
    private readonly Dictionary<string, long> _ackEvents = new Dictionary<string, long>();
    private readonly Dictionary<string, long> _ackAffect = new Dictionary<string, long>();

    public SyncEngine(string folder, string studyId, string uploadKey, Transport transport)
    {
        Folder = folder;
        StudyId = studyId;
        UploadKey = uploadKey;
        _transport = transport;
        LoadState();
    }

    public class Result
    {
        public bool ok = true;
        public string error = "";
        public int eventsSent, affectSent;
        public long pendingEvents, pendingAffect;
        public int sessions;
    }

    // ------------------------------------------------------------------ Execução

    public Result SyncOnce()
    {
        var r = new Result();
        if (!Directory.Exists(Folder)) return r;

        var eventFiles = Directory.GetFiles(Folder, "events_*.jsonl").OrderBy(f => f, StringComparer.Ordinal).ToList();
        var affectFiles = Directory.GetFiles(Folder, "affect_*.csv").OrderBy(f => f, StringComparer.Ordinal).ToList();
        r.sessions = eventFiles.Count;

        // Sessões que este computador ainda não sabe até onde o servidor recebeu.
        var unknown = eventFiles.Select(f => SessionOf(f, "events_", ".jsonl")).Where(s => !_ackEvents.ContainsKey(s))
            .Concat(affectFiles.Select(f => SessionOf(f, "affect_", ".csv")).Where(s => !_ackAffect.ContainsKey(s)))
            .Distinct().ToList();
        if (unknown.Count > 0 && !AskStatus(unknown, r)) return Finish(r);

        foreach (var f in eventFiles)
        {
            if (!SyncEvents(f, r)) return Finish(r);
        }
        foreach (var f in affectFiles)
        {
            if (!SyncAffect(f, r)) return Finish(r);
        }
        return Finish(r);
    }

    private Result Finish(Result r)
    {
        SaveState();
        r.pendingEvents = CountPending(true);
        r.pendingAffect = CountPending(false);
        return r;
    }

    /// <summary>Quantas linhas ainda não foram confirmadas pelo servidor (para o painel).</summary>
    public long CountPending(bool events)
    {
        if (!Directory.Exists(Folder)) return 0;
        long total = 0;
        foreach (var f in Directory.GetFiles(Folder, events ? "events_*.jsonl" : "affect_*.csv"))
        {
            string session = events ? SessionOf(f, "events_", ".jsonl") : SessionOf(f, "affect_", ".csv");
            var lines = ReadCompleteLines(f);
            long ack = (events ? _ackEvents : _ackAffect).TryGetValue(session, out var a) ? a : 0;
            long n = events ? lines.Count(l => SeqOf(l, 0) > ack) : Math.Max(0, lines.Count - 1 - ack);
            total += n;
        }
        return total;
    }

    // ------------------------------------------------------------------ Eventos

    private bool SyncEvents(string file, Result r)
    {
        string session = SessionOf(file, "events_", ".jsonl");
        var lines = ReadCompleteLines(file);
        long ack = _ackEvents.TryGetValue(session, out var a) ? a : 0;

        var pending = new List<(long seq, string json)>();
        for (int i = 0; i < lines.Count; i++)
        {
            string line = lines[i];
            if (line.Trim().Length == 0) continue;
            string json = EnsureIdAndSeq(line, session, i + 1);   // arquivos antigos (esquema 1.0) não têm id/seq
            long seq = SeqOf(json, i + 1);
            if (seq > ack) pending.Add((seq, json));
        }

        for (int start = 0; start < pending.Count; start += EventBatch)
        {
            var batch = pending.Skip(start).Take(EventBatch).ToList();
            string body = "{\"p_study\":" + Q(StudyId) + ",\"p_key\":" + Q(UploadKey) +
                          ",\"p_events\":[" + string.Join(",", batch.Select(b => b.json)) + "]}";
            var (ok, status, resp) = _transport("fc_ingest_events", body);
            if (!ok) return Fail(r, status, resp);
            long newAck = AckOf(resp, session);
            if (newAck < batch.Last().seq) return Fail(r, status, $"servidor confirmou até {newAck}, esperado {batch.Last().seq}");
            _ackEvents[session] = newAck;
            r.eventsSent += batch.Count;
            SaveState();
        }
        return true;
    }

    /// <summary>Linhas do esquema 1.0 (sem id/seq) recebem id determinístico e seq = número da linha.</summary>
    public static string EnsureIdAndSeq(string line, string session, int lineNumber)
    {
        string t = line.TrimStart();
        if (t.StartsWith("{\"id\":") || Regex.IsMatch(t, "\"seq\"\\s*:")) return line;
        string id = Hex(SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(session + "#" + lineNumber))).Substring(0, 32);
        return "{\"id\":\"" + id + "\",\"seq\":" + lineNumber.ToString(CultureInfo.InvariantCulture) + "," + t.Substring(1);
    }

    // ------------------------------------------------------------------ Dados faciais

    private bool SyncAffect(string file, Result r)
    {
        string session = SessionOf(file, "affect_", ".csv");
        var lines = ReadCompleteLines(file);
        if (lines.Count < 2) return true;
        string[] header = SplitCsv(lines[0]);
        long ack = _ackAffect.TryGetValue(session, out var a) ? a : 0;

        var pending = new List<(long idx, string json)>();
        for (int i = 1; i < lines.Count; i++)
        {
            long idx = i;   // número da amostra = linha do CSV (sem contar o cabeçalho)
            if (idx <= ack || lines[i].Trim().Length == 0 || lines[i] == lines[0]) continue;   // cabeçalho repetido (versões antigas)
            pending.Add((idx, AffectRowToJson(header, SplitCsv(lines[i]), idx)));
        }

        for (int start = 0; start < pending.Count; start += AffectBatch)
        {
            var batch = pending.Skip(start).Take(AffectBatch).ToList();
            string body = "{\"p_study\":" + Q(StudyId) + ",\"p_key\":" + Q(UploadKey) +
                          ",\"p_samples\":[" + string.Join(",", batch.Select(b => b.json)) + "]}";
            var (ok, status, resp) = _transport("fc_ingest_affect", body);
            if (!ok) return Fail(r, status, resp);
            long newAck = AckOf(resp, session);
            if (newAck < batch.Last().idx) return Fail(r, status, $"servidor confirmou até {newAck}, esperado {batch.Last().idx}");
            _ackAffect[session] = newAck;
            r.affectSent += batch.Count;
            SaveState();
        }
        return true;
    }

    private static readonly HashSet<string> TextColumns = new HashSet<string> { "utc", "session", "participant", "scene" };

    /// <summary>Converte uma linha do affect_*.csv num objeto JSON (números como números, vazio = null).</summary>
    public static string AffectRowToJson(string[] header, string[] cells, long idx)
    {
        var sb = new StringBuilder(64 + header.Length * 16);
        sb.Append("{\"idx\":").Append(idx.ToString(CultureInfo.InvariantCulture));
        for (int c = 0; c < header.Length; c++)
        {
            string key = header[c];
            string v = c < cells.Length ? cells[c] : "";
            sb.Append(',').Append(Q(key)).Append(':');
            if (TextColumns.Contains(key)) sb.Append(Q(v));
            else if (v.Length == 0) sb.Append("null");
            else if (double.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out double d) && !double.IsNaN(d) && !double.IsInfinity(d))
                sb.Append(v);
            else sb.Append(Q(v));
        }
        return sb.Append('}').ToString();
    }

    public static string[] SplitCsv(string line)
    {
        var cells = new List<string>();
        var cur = new StringBuilder();
        bool quoted = false;
        for (int i = 0; i < line.Length; i++)
        {
            char ch = line[i];
            if (quoted)
            {
                if (ch == '"' && i + 1 < line.Length && line[i + 1] == '"') { cur.Append('"'); i++; }
                else if (ch == '"') quoted = false;
                else cur.Append(ch);
            }
            else if (ch == '"') quoted = true;
            else if (ch == ',') { cells.Add(cur.ToString()); cur.Clear(); }
            else cur.Append(ch);
        }
        cells.Add(cur.ToString());
        return cells.ToArray();
    }

    // ------------------------------------------------------------------ Status no servidor

    private bool AskStatus(List<string> sessions, Result r)
    {
        string body = "{\"p_study\":" + Q(StudyId) + ",\"p_key\":" + Q(UploadKey) + ",\"p_sessions\":[" +
                      string.Join(",", sessions.Select(Q)) + "]}";
        var (ok, status, resp) = _transport("fc_upload_status", body);
        if (!ok) return Fail(r, status, resp);
        foreach (var s in sessions)
        {
            var m = Regex.Match(resp, "\"" + Regex.Escape(s) + "\"\\s*:\\s*\\{([^}]*)\\}");
            if (!m.Success) continue;
            _ackEvents[s] = NumberAfter(m.Groups[1].Value, "events");
            _ackAffect[s] = NumberAfter(m.Groups[1].Value, "affect");
        }
        return true;
    }

    /// <summary>Testa conexão e chave (usado pelos painéis). Retorna mensagem legível.</summary>
    public string TestConnection(out bool ok)
    {
        string body = "{\"p_study\":" + Q(StudyId) + ",\"p_key\":" + Q(UploadKey) + ",\"p_sessions\":[]}";
        var (good, status, resp) = _transport("fc_upload_status", body);
        ok = good;
        if (!good) return Describe(status, resp);
        var m = Regex.Match(resp, "\"_study\"\\s*:\\s*\\{([^}]*)\\}");
        return m.Success
            ? $"Conectado ao estudo {StudyId}: {NumberAfter(m.Groups[1].Value, "sessions")} sessões, {NumberAfter(m.Groups[1].Value, "events")} eventos e {NumberAfter(m.Groups[1].Value, "affect")} amostras faciais no servidor."
            : "Conectado.";
    }

    // ------------------------------------------------------------------ Estado local

    private void LoadState()
    {
        string p = Path.Combine(Folder, StateFile);
        if (!File.Exists(p)) return;
        foreach (var line in File.ReadAllLines(p))
        {
            var c = line.Split('\t');
            if (c.Length != 3 || !long.TryParse(c[2], out long v)) continue;
            (c[0] == "events" ? _ackEvents : _ackAffect)[c[1]] = v;
        }
    }

    private void SaveState()
    {
        if (!Directory.Exists(Folder)) return;
        var sb = new StringBuilder();
        foreach (var kv in _ackEvents) sb.Append("events\t").Append(kv.Key).Append('\t').Append(kv.Value).Append('\n');
        foreach (var kv in _ackAffect) sb.Append("affect\t").Append(kv.Key).Append('\t').Append(kv.Value).Append('\n');
        string p = Path.Combine(Folder, StateFile);
        File.WriteAllText(p + ".tmp", sb.ToString());
        if (File.Exists(p)) File.Delete(p);
        File.Move(p + ".tmp", p);
    }

    // ------------------------------------------------------------------ Utilidades

    /// <summary>Lê o arquivo mesmo aberto pelo jogo e descarta a última linha se ainda não terminou.</summary>
    public static List<string> ReadCompleteLines(string path)
    {
        string text;
        using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
        using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
            text = sr.ReadToEnd();
        var lines = text.Split('\n').Select(l => l.TrimEnd('\r')).ToList();
        lines.RemoveAt(lines.Count - 1);   // depois do último '\n': vazio, ou linha ainda sendo gravada
        return lines;
    }

    private static string SessionOf(string file, string prefix, string ext)
    {
        string n = Path.GetFileName(file);
        return n.Substring(prefix.Length, n.Length - prefix.Length - ext.Length);
    }

    private static long SeqOf(string json, long fallback)
    {
        var m = Regex.Match(json, "\"seq\"\\s*:\\s*(\\d+)");
        return m.Success ? long.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture) : fallback;
    }

    private static long AckOf(string resp, string session)
    {
        var m = Regex.Match(resp ?? "", "\"" + Regex.Escape(session) + "\"\\s*:\\s*(\\d+)");
        return m.Success ? long.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture) : -1;
    }

    private static long NumberAfter(string s, string key)
    {
        var m = Regex.Match(s, "\"" + key + "\"\\s*:\\s*(\\d+)");
        return m.Success ? long.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture) : 0;
    }

    private static bool Fail(Result r, int status, string resp)
    {
        r.ok = false;
        r.error = Describe(status, resp);
        return false;
    }

    public static string Describe(int status, string resp)
    {
        string msg = resp ?? "";
        var m = Regex.Match(msg, "\"message\"\\s*:\\s*\"([^\"]*)\"");
        if (m.Success) msg = m.Groups[1].Value;
        if (status == 0) return "sem conexão com o servidor (" + msg + ")";
        if (msg.Contains("chave de envio invalida")) return "chave de envio inválida ou estudo não cadastrado no servidor";
        if (status == 401 || status == 403) return "acesso negado (confira a URL e a chave pública do Supabase)";
        if (status == 404) return "função não encontrada (rode o schema.sql no Supabase)";
        return $"erro {status}: {(msg.Length > 200 ? msg.Substring(0, 200) : msg)}";
    }

    private static string Hex(byte[] b) => string.Concat(b.Select(x => x.ToString("x2", CultureInfo.InvariantCulture)));

    private static string Q(string s) => "\"" + (s ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
}
