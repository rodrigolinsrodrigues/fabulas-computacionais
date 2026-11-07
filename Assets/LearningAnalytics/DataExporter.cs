using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

/// <summary>
/// Exporta a coleta desta máquina num único .zip, pronto para levar num pendrive:
///  - todos os events_*.jsonl e affect_*.csv;
///  - manifest.json com estudo, escola, máquina, versões, contagens e o SHA-256 de cada arquivo
///    (o pacote R fabulasLA confere a integridade ao ler);
///  - participants_map.csv (nomes) SÓ se "exportIncludesNameMap" estiver ligado — por padrão
///    a exportação é anonimizada.
/// Usado pelo Painel do Pesquisador (no jogo) e pelo Painel do Estudo (no Unity).
/// </summary>
public static class DataExporter
{
    public static string DefaultExportFolder
    {
        get
        {
            string cfg = ResearchConfig.Current.exportFolder;
            return string.IsNullOrEmpty(cfg) ? Path.Combine(TelemetryManager.DataFolder, "Exportacoes") : cfg;
        }
    }

    public static string Export(string destFolder = null, bool? includeNameMap = null)
    {
        var cfg = ResearchConfig.Current;
        string src = TelemetryManager.DataFolder;
        destFolder ??= DefaultExportFolder;
        bool withMap = includeNameMap ?? cfg.exportIncludesNameMap;
        Directory.CreateDirectory(destFolder);

        var files = new List<string>();
        if (Directory.Exists(src))
        {
            files.AddRange(Directory.GetFiles(src, "events_*.jsonl"));
            files.AddRange(Directory.GetFiles(src, "affect_*.csv"));
            string map = Path.Combine(src, "participants_map.csv");
            if (withMap && File.Exists(map)) files.Add(map);
        }
        files.Sort(StringComparer.Ordinal);

        string Safe(string s) => string.IsNullOrEmpty(s) ? "sem" : new string(s.Select(c => char.IsLetterOrDigit(c) ? c : '-').ToArray());
        string name = $"coleta_{Safe(cfg.studyId)}_{Safe(cfg.schoolId)}_{Safe(cfg.EffectiveMachineId)}_{DateTime.Now:yyyyMMdd_HHmmss}.zip";
        string zipPath = Path.Combine(destFolder, name);

        var manifest = new StringBuilder();
        manifest.Append("{\n");
        manifest.Append($"  \"schema\": \"{TelemetryManager.SchemaVersion}\",\n");
        manifest.Append($"  \"exported_utc\": \"{DateTime.UtcNow:o}\",\n");
        manifest.Append($"  \"study_id\": {Q(cfg.studyId)},\n  \"school_id\": {Q(cfg.schoolId)},\n  \"class_id\": {Q(cfg.classId)},\n");
        manifest.Append($"  \"machine_id\": {Q(cfg.EffectiveMachineId)},\n");
        manifest.Append($"  \"game_version\": {Q(Application.version)},\n  \"unity_version\": {Q(Application.unityVersion)},\n");
        manifest.Append($"  \"includes_name_map\": {(withMap ? "true" : "false")},\n");
        manifest.Append("  \"files\": [\n");

        using (var fs = new FileStream(zipPath, FileMode.Create))
        using (var zip = new ZipArchive(fs, ZipArchiveMode.Create))
        {
            for (int i = 0; i < files.Count; i++)
            {
                string f = files[i];
                byte[] bytes = ReadShared(f);
                string entryName = Path.GetFileName(f);
                var entry = zip.CreateEntry(entryName, System.IO.Compression.CompressionLevel.Optimal);
                using (var es = entry.Open()) es.Write(bytes, 0, bytes.Length);

                int lines = entryName.EndsWith(".jsonl") || entryName.EndsWith(".csv") ? CountLines(bytes) : 0;
                manifest.Append($"    {{\"name\": {Q(entryName)}, \"bytes\": {bytes.Length}, \"lines\": {lines}, \"sha256\": \"{Sha256(bytes)}\"}}");
                manifest.Append(i < files.Count - 1 ? ",\n" : "\n");
            }
            manifest.Append("  ]\n}\n");
            var m = zip.CreateEntry("manifest.json");
            using (var w = new StreamWriter(m.Open(), new UTF8Encoding(false))) w.Write(manifest.ToString());
        }

        TelemetryManager.Log("data_export", ("file", name), ("files", files.Count));
        Debug.Log($"[DataExporter] {files.Count} arquivos exportados para {zipPath}");
        return zipPath;
    }

    /// <summary>Lê mesmo que o arquivo esteja aberto pelo jogo (sessão em andamento).</summary>
    private static byte[] ReadShared(string path)
    {
        using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var ms = new MemoryStream();
        fs.CopyTo(ms);
        return ms.ToArray();
    }

    private static int CountLines(byte[] bytes)
    {
        int n = 0;
        foreach (byte b in bytes) if (b == (byte)'\n') n++;
        return n;
    }

    public static string Sha256(byte[] bytes)
    {
        using var sha = SHA256.Create();
        return string.Concat(sha.ComputeHash(bytes).Select(b => b.ToString("x2", CultureInfo.InvariantCulture)));
    }

    private static string Q(string s) => "\"" + (s ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
}
