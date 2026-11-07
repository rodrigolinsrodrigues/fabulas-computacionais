using System;
using System.IO;
using UnityEngine;

/// <summary>
/// Configuração da coleta de dados de pesquisa, lida de StreamingAssets/research_config.json.
/// O pesquisador edita esse arquivo (ou usa o Painel do Estudo no Unity: Fabulas > Learning
/// Analytics > Painel do Estudo) em cada máquina, sem precisar recompilar o jogo.
///
/// Argumentos de linha de comando (úteis para testes e para várias configurações na mesma máquina):
///   -researchConfig "caminho\config.json"      usa outro arquivo de configuração
///   -analyticsDataFolder "caminho\pasta"       grava os dados em outra pasta
/// </summary>
[Serializable]
public class ResearchConfig
{
    public const string FileName = "research_config.json";

    // ---- Identificação do estudo (vão para o evento session_start) ----
    public string studyId = "";
    public string schoolId = "";
    public string classId = "";
    public string machineId = "";          // vazio = nome do computador

    // ---- Condições experimentais (A/B) ----
    // conditionAssignment: "fixed" (usa "condition"), "random" (sorteia a cada sessão entre "conditions")
    // ou "by_participant" (sempre a mesma condição para o mesmo participante, sorteada pelo código).
    public string condition = "";
    public string[] conditions = new string[0];
    public string conditionAssignment = "fixed";

    // ---- Identificação do participante ----
    // "name": o cadastro pede o nome (gera código pseudônimo; nome fica só em participants_map.csv).
    // "code": o campo de nome passa a pedir o CÓDIGO DO PARTICIPANTE (gerado no Painel do Estudo);
    //         nenhum nome é gravado no computador.
    public string participantIdMode = "name";

    // ---- Consentimento ----
    public bool requireConsent = false;   // mostra a tela de consentimento antes de iniciar a sessão
    public string consentTextFile = "consent_text.txt";

    // ---- Modo pesquisador ----
    public string researcherPin = "1234"; // Ctrl+Shift+R abre o painel; vazio = sem senha

    // ---- Exportação ----
    public string exportFolder = "";       // vazio = <pasta de dados>/Exportacoes
    public bool exportIncludesNameMap = false;

    // ---- Telemetria geral ----
    public bool telemetryEnabled = true;
    public float idleThresholdSeconds = 30f;

    // ---- Captura de expressões faciais (MediaPipe) ----
    // Só ative em máquinas/turmas com TCLE (responsáveis) e TALE (criança) assinados.
    public bool affectCaptureEnabled = false;
    public float affectSampleRateHz = 2f;
    public float affectSummaryIntervalSeconds = 10f;
    public string cameraDeviceName = "";
    public int cameraWidth = 640;
    public int cameraHeight = 480;
    public int cameraFps = 15;
    public bool saveRawBlendshapes = true;
    public bool showCameraIndicator = true;
    public string modelFileName = "MediaPipe/face_landmarker_v2_with_blendshapes.bytes";

    private static ResearchConfig _current;

    public static ResearchConfig Current
    {
        get
        {
            if (_current == null) _current = Load();
            return _current;
        }
    }

    /// <summary>Caminho do arquivo em uso (padrão ou indicado por -researchConfig).</summary>
    public static string ConfigPath
    {
        get
        {
            string arg = CommandLineArg("-researchConfig");
            return !string.IsNullOrEmpty(arg) ? arg : Path.Combine(Application.streamingAssetsPath, FileName);
        }
    }

    public string EffectiveMachineId => string.IsNullOrEmpty(machineId) ? SystemInfo.deviceName : machineId;

    public bool UsesParticipantCodes => string.Equals(participantIdMode, "code", StringComparison.OrdinalIgnoreCase);

    public static ResearchConfig Load() => LoadFrom(ConfigPath);

    public static ResearchConfig LoadFrom(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                var cfg = JsonUtility.FromJson<ResearchConfig>(File.ReadAllText(path));
                if (cfg != null) return cfg.Normalized();
            }
            else
            {
                Debug.LogWarning($"[ResearchConfig] {path} não encontrado; usando valores padrão (câmera desligada).");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[ResearchConfig] Erro ao ler {path}: {e.Message}. Usando valores padrão.");
        }
        return new ResearchConfig();
    }

    public void Save(string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllText(path, JsonUtility.ToJson(this, true));
    }

    /// <summary>Substitui a configuração em uso (usado pelos testes e pelo Painel do Estudo).</summary>
    public static void SetCurrent(ResearchConfig cfg) => _current = cfg;

    private ResearchConfig Normalized()
    {
        conditions ??= new string[0];
        participantIdMode = string.IsNullOrEmpty(participantIdMode) ? "name" : participantIdMode.Trim().ToLowerInvariant();
        conditionAssignment = string.IsNullOrEmpty(conditionAssignment) ? "fixed" : conditionAssignment.Trim().ToLowerInvariant();
        return this;
    }

    /// <summary>
    /// Condição experimental desta sessão. "by_participant" usa um hash estável do código,
    /// para que o mesmo participante caia sempre na mesma condição.
    /// </summary>
    public string AssignCondition(string participantId)
    {
        if (conditions == null || conditions.Length == 0) return condition ?? "";
        switch (conditionAssignment)
        {
            case "random":
                return conditions[UnityEngine.Random.Range(0, conditions.Length)];
            case "by_participant":
                return conditions[(int)(StableHash(participantId ?? "") % (uint)conditions.Length)];
            default:
                return string.IsNullOrEmpty(condition) ? conditions[0] : condition;
        }
    }

    /// <summary>FNV-1a 32 bits: igual em qualquer máquina e versão do .NET.</summary>
    public static uint StableHash(string s)
    {
        uint h = 2166136261;
        foreach (char c in s)
        {
            h ^= c;
            h *= 16777619;
        }
        return h;
    }

    public static string CommandLineArg(string name)
    {
        var args = Environment.GetCommandLineArgs();
        int i = Array.IndexOf(args, name);
        return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
    }

    public static bool HasCommandLineFlag(string name) => Array.IndexOf(Environment.GetCommandLineArgs(), name) >= 0;
}
