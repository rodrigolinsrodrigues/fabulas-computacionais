using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

/// <summary>
/// Envio ao servidor central: usa um servidor falso, em memória, com as mesmas regras do
/// schema.sql (ignora repetidos, responde até onde recebeu, recusa chave errada).
/// </summary>
public class SyncTests : AnalyticsTestBase
{
    private class FakeServer
    {
        public readonly Dictionary<string, Dictionary<string, object>> Events = new Dictionary<string, Dictionary<string, object>>();
        public readonly Dictionary<string, Dictionary<string, object>> Affect = new Dictionary<string, Dictionary<string, object>>();
        public bool Offline;
        public int FailAfterCalls = -1;
        public int Calls;
        public string Key = "env_ok";

        public (bool, int, string) Handle(string rpc, string body)
        {
            Calls++;
            if (Offline || (FailAfterCalls >= 0 && Calls > FailAfterCalls)) return (false, 0, "Unable to connect");
            var req = MiniJson.Obj(body);
            if ((string)req["p_key"] != Key || (string)req["p_study"] != "EST1")
                return (false, 400, "{\"code\":\"28000\",\"message\":\"chave de envio invalida para o estudo EST1\"}");
            switch (rpc)
            {
                case "fc_ingest_events":
                    foreach (Dictionary<string, object> e in (List<object>)req["p_events"])
                        if (!Events.ContainsKey((string)e["id"])) Events[(string)e["id"]] = e;
                    return (true, 200, MaxBy(Events.Values, "seq"));
                case "fc_ingest_affect":
                    foreach (Dictionary<string, object> a in (List<object>)req["p_samples"])
                        Affect[(string)a["session"] + "#" + Convert.ToInt64(a["idx"])] = a;
                    return (true, 200, MaxBy(Affect.Values, "idx"));
                case "fc_upload_status":
                    var sb = new StringBuilder("{");
                    foreach (string s in (List<object>)req["p_sessions"])
                        sb.Append($"\"{s}\":{{\"events\":{Max(Events.Values, s, "seq")},\"affect\":{Max(Affect.Values, s, "idx")}}},");
                    sb.Append($"\"_study\":{{\"sessions\":1,\"events\":{Events.Count},\"affect\":{Affect.Count}}}}}");
                    return (true, 200, sb.ToString());
            }
            return (false, 404, "{\"message\":\"funcao nao encontrada\"}");
        }

        private static long Max(IEnumerable<Dictionary<string, object>> rows, string session, string field) =>
            rows.Where(r => (string)r["session"] == session).Select(r => Convert.ToInt64(r[field])).DefaultIfEmpty(0).Max();

        private static string MaxBy(IEnumerable<Dictionary<string, object>> rows, string field) =>
            "{" + string.Join(",", rows.GroupBy(r => (string)r["session"])
                .Select(g => $"\"{g.Key}\":{g.Max(r => Convert.ToInt64(r[field]))}")) + "}";
    }

    private FakeServer _server;

    private SyncEngine Engine(string key = "env_ok") => new SyncEngine(Folder, "EST1", key, _server.Handle) { EventBatch = 3, AffectBatch = 2 };

    [SetUp]
    public void SetUpServer() => _server = new FakeServer();

    private string PlaySession(int puzzles)
    {
        var tm = CreateTelemetry();
        tm.StartSession("Ana", 9, "F", "4º ano");
        for (int i = 0; i < puzzles; i++) TelemetryManager.Log("puzzle_attempt", ("puzzle", "Musical"), ("correct", i % 2 == 0));
        string session = tm.SessionId;
        tm.EndSession("teste");
        return session;
    }

    private void WriteAffect(string session, int rows, bool partialLastLine = false)
    {
        var sb = new StringBuilder("utc,t_ms,session,participant,scene,face_present,head_yaw,smile,bs_jawOpen\n");
        for (int i = 1; i <= rows; i++)
            sb.Append($"2026-11-07T10:00:{i:00}Z,{i * 500},{session},K7M2X,\"Area1, sala\",{(i == 2 ? 0 : 1)},{(i == 2 ? "" : "-3.5")},0.{i},0.05\n");
        if (partialLastLine) sb.Append("2026-11-07T10:01:00Z,99999,");
        File.WriteAllText(Path.Combine(Folder, $"affect_{session}.csv"), sb.ToString());
    }

    [Test]
    public void EnviaTodosOsEventosEAmostrasEMantemOsArquivosLocais()
    {
        string s = PlaySession(5);
        WriteAffect(s, 5);
        int localLines = File.ReadAllLines(Path.Combine(Folder, $"events_{s}.jsonl")).Length;

        var r = Engine().SyncOnce();

        Assert.IsTrue(r.ok, r.error);
        Assert.AreEqual(localLines, _server.Events.Count, "todos os eventos no servidor");
        Assert.AreEqual(5, _server.Affect.Count, "todas as amostras faciais no servidor");
        Assert.AreEqual(0, r.pendingEvents);
        Assert.AreEqual(0, r.pendingAffect);
        Assert.IsTrue(File.Exists(Path.Combine(Folder, $"events_{s}.jsonl")), "arquivo local não pode sumir");
        Assert.IsTrue(File.Exists(Path.Combine(Folder, $"affect_{s}.csv")));

        var a2 = _server.Affect[s + "#2"];
        Assert.AreEqual(0L, Convert.ToInt64(a2["face_present"]));
        Assert.IsNull(a2["head_yaw"], "célula vazia vira null");
        Assert.AreEqual("Area1, sala", a2["scene"], "vírgula entre aspas no CSV");
        Assert.AreEqual(0.05, Convert.ToDouble(a2["bs_jawOpen"]), 1e-9, "blendshapes vão junto");
    }

    [Test]
    public void SemInternetNadaSePerdeEDepoisEnviaSoOQueFalta()
    {
        string s = PlaySession(6);
        WriteAffect(s, 4);
        _server.Offline = true;
        var r1 = Engine().SyncOnce();
        Assert.IsFalse(r1.ok);
        StringAssert.Contains("sem conexão", r1.error);
        Assert.Greater(r1.pendingEvents, 0);

        // Volta a internet no meio: cai depois de 2 chamadas (lotes de 3 eventos)
        _server.Offline = false;
        _server.FailAfterCalls = _server.Calls + 3;   // status + 2 lotes
        var r2 = Engine().SyncOnce();
        Assert.IsFalse(r2.ok);
        int afterPartial = _server.Events.Count;
        Assert.Greater(afterPartial, 0);

        _server.FailAfterCalls = -1;
        int callsBefore = _server.Calls;
        var r3 = Engine().SyncOnce();
        Assert.IsTrue(r3.ok, r3.error);
        Assert.AreEqual(File.ReadAllLines(Path.Combine(Folder, $"events_{s}.jsonl")).Length, _server.Events.Count);
        Assert.AreEqual(4, _server.Affect.Count);
        Assert.AreEqual(File.ReadAllLines(Path.Combine(Folder, $"events_{s}.jsonl")).Length - afterPartial, r3.eventsSent,
            "não reenvia o que o servidor já confirmou");

        var r4 = Engine().SyncOnce();
        Assert.AreEqual(0, r4.eventsSent + r4.affectSent, "nada novo, nada enviado");
    }

    [Test]
    public void SemEstadoLocalPerguntaAoServidorAteOndeRecebeu()
    {
        string s = PlaySession(4);
        Assert.IsTrue(Engine().SyncOnce().ok);
        File.Delete(Path.Combine(Folder, SyncEngine.StateFile));   // ex.: jogo reinstalado

        var r = Engine().SyncOnce();
        Assert.IsTrue(r.ok, r.error);
        Assert.AreEqual(0, r.eventsSent, "servidor já tinha tudo");
    }

    [Test]
    public void LinhaIncompletaFicaParaDepois()
    {
        string s = PlaySession(1);
        WriteAffect(s, 3, partialLastLine: true);
        var r = Engine().SyncOnce();
        Assert.IsTrue(r.ok, r.error);
        Assert.AreEqual(3, _server.Affect.Count, "a linha sendo gravada não vai");
    }

    [Test]
    public void ChaveErradaEhRecusadaComMensagemClara()
    {
        PlaySession(1);
        var r = Engine("env_errada").SyncOnce();
        Assert.IsFalse(r.ok);
        StringAssert.Contains("chave de envio inválida", r.error);
        Assert.AreEqual(0, _server.Events.Count);
    }

    [Test]
    public void ArquivoDoEsquemaAntigoSemIdRecebeIdEstavel()
    {
        string old = "{\"t\":\"2026-01-01T00:00:00Z\",\"t_ms\":0,\"session\":\"OLD\",\"participant\":\"P\",\"scene\":\"\",\"event\":\"session_start\",\"data\":{}}";
        File.WriteAllText(Path.Combine(Folder, "events_OLD.jsonl"), old + "\n" + old.Replace("session_start", "session_end") + "\n");
        Assert.IsTrue(Engine().SyncOnce().ok);
        Assert.AreEqual(2, _server.Events.Count);
        var a = SyncEngine.EnsureIdAndSeq(old, "OLD", 1);
        Assert.AreEqual(a, SyncEngine.EnsureIdAndSeq(old, "OLD", 1), "id determinístico: reenvio não duplica");
        Assert.AreEqual(1L, Convert.ToInt64(MiniJson.Obj(a)["seq"]));
    }

    [Test]
    public void ConfiguracaoDoServidorEhValidada()
    {
        var cfg = new ResearchConfig { studyId = "EST1" };
        var off = StudyConfigValidator.ValidateServer(new ServerSyncConfig(), cfg);
        Assert.IsTrue(off.All(i => i.level == StudyConfigValidator.Level.Info));

        var secret = new ServerSyncConfig { enabled = true, serverUrl = "https://x.supabase.co", anonKey = "sb_secret_abc", uploadKey = "env_1" };
        Assert.IsTrue(StudyConfigValidator.ValidateServer(secret, cfg).Any(i => i.level == StudyConfigValidator.Level.Error && i.message.Contains("SECRETA")));

        var ok = new ServerSyncConfig { enabled = true, serverUrl = "https://x.supabase.co", anonKey = "sb_publishable_abc", uploadKey = "env_1" };
        Assert.IsFalse(StudyConfigValidator.ValidateServer(ok, cfg).Any(i => i.level == StudyConfigValidator.Level.Error));
    }

    [Test]
    public void GeraChavesDoEstudoNumArquivoComOComandoSql()
    {
        var (up, read, file) = StudyTools.GenerateServerKeys("EST1", Folder);
        Assert.AreNotEqual(up, read);
        StringAssert.StartsWith("env_", up);
        StringAssert.StartsWith("lei_", read);
        Assert.AreEqual(36, up.Length);
        string txt = File.ReadAllText(file);
        StringAssert.Contains($"select public.fc_create_study('EST1', '{up}', '{read}');", txt);
        Assert.AreNotEqual(up, StudyTools.GenerateServerKeys("EST1", Folder).uploadKey, "chaves aleatórias");
    }
}
