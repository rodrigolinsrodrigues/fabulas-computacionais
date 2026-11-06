using System;
using System.IO;
using UnityEngine;

/// <summary>
/// Configuração da coleta de dados de pesquisa, lida de
/// StreamingAssets/research_config.json. O pesquisador edita esse arquivo
/// em cada máquina (inclusive na build) sem precisar recompilar o jogo.
/// </summary>
[Serializable]
public class ResearchConfig
{
    public const string FileName = "research_config.json";

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

    public static ResearchConfig Load()
    {
        string path = Path.Combine(Application.streamingAssetsPath, FileName);
        try
        {
            if (File.Exists(path))
            {
                var cfg = JsonUtility.FromJson<ResearchConfig>(File.ReadAllText(path));
                if (cfg != null) return cfg;
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
}
