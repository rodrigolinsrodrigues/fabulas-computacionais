using System;
using System.IO;
using UnityEngine;

/// <summary>
/// Configuração do envio ao servidor central (Supabase), em StreamingAssets/server_sync.json.
/// O repositório traz este arquivo configurado para o Supabase do projeto (estudo FABULAS).
/// A chave de envio é pública de propósito: só permite gravar dados novos, não ler nem apagar.
/// Para outro estudo, gere chaves pelo Painel do Estudo.
///
/// Argumento de linha de comando: -serverConfig "caminho\server_sync.json"
/// </summary>
[Serializable]
public class ServerSyncConfig
{
    public const string FileName = "server_sync.json";

    public bool enabled = false;
    public string serverUrl = "";          // https://<projeto>.supabase.co
    public string anonKey = "";            // chave pública (anon / publishable) do projeto
    public string studyId = "";            // vazio = studyId do research_config.json
    public string uploadKey = "";          // chave de envio do estudo (só permite ENVIAR dados)
    public float syncIntervalSeconds = 30f;

    private static ServerSyncConfig _current;

    public static ServerSyncConfig Current
    {
        get
        {
            if (_current == null) _current = LoadFrom(ConfigPath);
            return _current;
        }
    }

    public static void SetCurrent(ServerSyncConfig cfg) => _current = cfg;

    public static string ConfigPath
    {
        get
        {
            string arg = ResearchConfig.CommandLineArg("-serverConfig");
            return !string.IsNullOrEmpty(arg) ? arg : Path.Combine(Application.streamingAssetsPath, FileName);
        }
    }

    public string EffectiveStudyId => EffectiveStudyIdFor(ResearchConfig.Current);

    public string EffectiveStudyIdFor(ResearchConfig c) => string.IsNullOrEmpty(studyId) ? c?.studyId ?? "" : studyId;

    /// <summary>Pronto para enviar: ligado e com todos os campos preenchidos.</summary>
    public bool IsComplete =>
        enabled && !string.IsNullOrEmpty(serverUrl) && !string.IsNullOrEmpty(anonKey) &&
        !string.IsNullOrEmpty(uploadKey) && !string.IsNullOrEmpty(EffectiveStudyId);

    public static ServerSyncConfig LoadFrom(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                var cfg = JsonUtility.FromJson<ServerSyncConfig>(File.ReadAllText(path));
                if (cfg != null)
                {
                    cfg.serverUrl = (cfg.serverUrl ?? "").Trim().TrimEnd('/');
                    cfg.anonKey = (cfg.anonKey ?? "").Trim();
                    cfg.uploadKey = (cfg.uploadKey ?? "").Trim();
                    cfg.studyId = (cfg.studyId ?? "").Trim();
                    return cfg;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[Servidor] Erro ao ler {path}: {e.Message}. Envio ao servidor desligado.");
        }
        return new ServerSyncConfig();
    }

    public void Save(string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllText(path, JsonUtility.ToJson(this, true));
    }
}
