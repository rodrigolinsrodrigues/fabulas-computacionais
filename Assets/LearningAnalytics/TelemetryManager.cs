using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Registro de eventos de learning analytics.
///
/// - Criado automaticamente ao iniciar o jogo (não precisa estar em nenhuma cena).
/// - Grava um arquivo JSON Lines por sessão em
///   {Application.persistentDataPath}/LearningAnalytics/events_{sessionId}.jsonl
/// - O nome da criança NÃO vai para o arquivo de eventos: ele fica apenas em
///   participants_map.csv, que liga nome ↔ código do participante.
///
/// Uso em qualquer script:
///   TelemetryManager.Log("puzzle_attempt", ("level", "Musical"), ("attempt", 3));
/// </summary>
[DefaultExecutionOrder(-1000)]
public class TelemetryManager : MonoBehaviour
{
    public static TelemetryManager Instance { get; private set; }

    public const string SchemaVersion = "1.0";

    public string SessionId { get; private set; }
    public string ParticipantId { get; private set; }
    public bool SessionActive { get; private set; }
    public string CurrentScene { get; private set; } = "";

    public static string DataFolder => Path.Combine(Application.persistentDataPath, "LearningAnalytics");

    private StreamWriter _writer;
    private double _sessionStartRealtime;
    private float _nextFlush;
    private double _sceneEnterMs;

    // Detecção de ociosidade
    private float _lastInputTime;
    private bool _idle;
    private Vector3 _lastMousePos;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance != null) return;
        var go = new GameObject("[LearningAnalytics]");
        DontDestroyOnLoad(go);
        go.AddComponent<TelemetryManager>();
        go.AddComponent<AffectManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
        _lastInputTime = Time.unscaledTime;
    }

    private void OnDestroy()
    {
        if (Instance != this) return;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    /// <summary>Milissegundos desde o início da sessão (relógio monotônico, usado para sincronizar com a câmera).</summary>
    public double SessionTimeMs => SessionActive ? (Time.realtimeSinceStartupAsDouble - _sessionStartRealtime) * 1000.0 : 0;

    // ------------------------------------------------------------------ Sessão

    public void StartSession(string playerName, int age, string gender, string schoolGrade)
    {
        if (!ResearchConfig.Current.telemetryEnabled) return;
        if (SessionActive) EndSession("new_session");

        Directory.CreateDirectory(DataFolder);
        SessionId = DateTime.UtcNow.ToString("yyyyMMdd'T'HHmmss'Z'") + "-" + Guid.NewGuid().ToString("N").Substring(0, 6);
        ParticipantId = GetOrCreateParticipantId(playerName, age, gender, schoolGrade);
        _sessionStartRealtime = Time.realtimeSinceStartupAsDouble;
        _sceneEnterMs = 0;

        string path = Path.Combine(DataFolder, $"events_{SessionId}.jsonl");
        _writer = new StreamWriter(path, true, new UTF8Encoding(false));
        SessionActive = true;

        Log("session_start",
            ("schema", SchemaVersion),
            ("age", age),
            ("gender", gender),
            ("school_grade", schoolGrade),
            ("game_version", Application.version),
            ("unity_version", Application.unityVersion),
            ("platform", Application.platform.ToString()),
            ("os", SystemInfo.operatingSystem),
            ("device_model", SystemInfo.deviceModel),
            ("screen", $"{Screen.width}x{Screen.height}"),
            ("joysticks", string.Join("|", Input.GetJoystickNames())));

        Debug.Log($"[Telemetry] Sessão {SessionId} iniciada para {ParticipantId}. Dados em {DataFolder}");

        if (AffectManager.Instance != null) AffectManager.Instance.OnSessionStarted();
    }

    public void EndSession(string reason)
    {
        if (!SessionActive) return;
        if (AffectManager.Instance != null) AffectManager.Instance.OnSessionEnding();
        Log("session_end", ("reason", reason), ("duration_ms", Math.Round(SessionTimeMs)));
        SessionActive = false;
        _writer?.Flush();
        _writer?.Dispose();
        _writer = null;
    }

    /// <summary>
    /// Código pseudônimo estável por (nome, idade, série) nesta máquina.
    /// O mapa nome ↔ código fica separado dos dados de análise.
    /// </summary>
    private string GetOrCreateParticipantId(string playerName, int age, string gender, string schoolGrade)
    {
        string mapPath = Path.Combine(DataFolder, "participants_map.csv");
        string key = $"{playerName}|{age}|{schoolGrade}";
        try
        {
            if (File.Exists(mapPath))
            {
                foreach (var line in File.ReadAllLines(mapPath))
                {
                    var cols = line.Split(';');
                    if (cols.Length >= 5 && $"{cols[1]}|{cols[2]}|{cols[4]}" == key) return cols[0];
                }
            }
            else
            {
                File.WriteAllText(mapPath, "participant_id;name;age;gender;school_grade;created_utc\n", new UTF8Encoding(false));
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[Telemetry] Erro lendo participants_map.csv: {e.Message}");
        }

        string id = "P-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpperInvariant();
        string Clean(string s) => (s ?? "").Replace(";", ",").Replace("\n", " ");
        File.AppendAllText(mapPath,
            $"{id};{Clean(playerName)};{age};{Clean(gender)};{Clean(schoolGrade)};{DateTime.UtcNow:o}\n",
            new UTF8Encoding(false));
        return id;
    }

    // ------------------------------------------------------------------ Log

    /// <summary>Registra um evento. Ignorado se não houver sessão ativa.</summary>
    public static void Log(string eventName, params (string key, object value)[] data)
    {
        if (Instance == null || !Instance.SessionActive) return;
        Instance.Write(eventName, data);
    }

    private void Write(string eventName, (string key, object value)[] data)
    {
        var sb = new StringBuilder(256);
        sb.Append('{');
        AppendPair(sb, "t", DateTime.UtcNow.ToString("o"), true);
        AppendPair(sb, "t_ms", Math.Round(SessionTimeMs, 1));
        AppendPair(sb, "session", SessionId);
        AppendPair(sb, "participant", ParticipantId);
        AppendPair(sb, "scene", CurrentScene);
        AppendPair(sb, "event", eventName);
        sb.Append(",\"data\":{");
        if (data != null)
        {
            for (int i = 0; i < data.Length; i++)
                AppendPair(sb, data[i].key, data[i].value, i == 0);
        }
        sb.Append("}}");

        try
        {
            _writer.WriteLine(sb.ToString());
        }
        catch (Exception e)
        {
            Debug.LogError($"[Telemetry] Falha ao gravar evento: {e.Message}");
        }
    }

    private static void AppendPair(StringBuilder sb, string key, object value, bool first = false)
    {
        if (!first) sb.Append(',');
        sb.Append('"').Append(Escape(key)).Append("\":");
        AppendValue(sb, value);
    }

    private static void AppendValue(StringBuilder sb, object value)
    {
        switch (value)
        {
            case null: sb.Append("null"); break;
            case bool b: sb.Append(b ? "true" : "false"); break;
            case string s: sb.Append('"').Append(Escape(s)).Append('"'); break;
            case float f: sb.Append(float.IsNaN(f) || float.IsInfinity(f) ? "null" : f.ToString("0.####", CultureInfo.InvariantCulture)); break;
            case double d: sb.Append(double.IsNaN(d) || double.IsInfinity(d) ? "null" : d.ToString("0.####", CultureInfo.InvariantCulture)); break;
            case int or long or short or byte or uint or ulong:
                sb.Append(Convert.ToString(value, CultureInfo.InvariantCulture)); break;
            case Vector3 v:
                sb.Append('[').Append(v.x.ToString("0.###", CultureInfo.InvariantCulture)).Append(',')
                  .Append(v.y.ToString("0.###", CultureInfo.InvariantCulture)).Append(',')
                  .Append(v.z.ToString("0.###", CultureInfo.InvariantCulture)).Append(']');
                break;
            case IDictionary<string, float> dict:
                sb.Append('{');
                bool first = true;
                foreach (var kv in dict)
                {
                    AppendPair(sb, kv.Key, kv.Value, first);
                    first = false;
                }
                sb.Append('}');
                break;
            case IDictionary<string, object> objDict:
                sb.Append('{');
                bool firstObj = true;
                foreach (var kv in objDict)
                {
                    AppendPair(sb, kv.Key, kv.Value, firstObj);
                    firstObj = false;
                }
                sb.Append('}');
                break;
            case System.Collections.IEnumerable list:
                sb.Append('[');
                bool firstItem = true;
                foreach (var item in list)
                {
                    if (!firstItem) sb.Append(',');
                    AppendValue(sb, item);
                    firstItem = false;
                }
                sb.Append(']');
                break;
            default: sb.Append('"').Append(Escape(value.ToString())).Append('"'); break;
        }
    }

    private static string Escape(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        var sb = new StringBuilder(s.Length + 8);
        foreach (char c in s)
        {
            switch (c)
            {
                case '"': sb.Append("\\\""); break;
                case '\\': sb.Append("\\\\"); break;
                case '\n': sb.Append("\\n"); break;
                case '\r': sb.Append("\\r"); break;
                case '\t': sb.Append("\\t"); break;
                default:
                    if (c < 0x20) sb.Append("\\u").Append(((int)c).ToString("x4"));
                    else sb.Append(c);
                    break;
            }
        }
        return sb.ToString();
    }

    // ------------------------------------------------------------------ Eventos automáticos

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CurrentScene = scene.name;
        _sceneEnterMs = SessionTimeMs;
        Log("scene_enter", ("scene_name", scene.name));
    }

    private void OnSceneUnloaded(Scene scene)
    {
        Log("scene_exit", ("scene_name", scene.name), ("time_in_scene_ms", Math.Round(SessionTimeMs - _sceneEnterMs)));
    }

    private void Update()
    {
        if (!SessionActive) return;

        // Ociosidade: nenhum botão/eixo/mouse por N segundos.
        bool anyInput = Input.anyKey
                        || Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.1f
                        || Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.1f
                        || (Input.mousePosition - _lastMousePos).sqrMagnitude > 1f;
        _lastMousePos = Input.mousePosition;

        if (anyInput)
        {
            if (_idle)
            {
                _idle = false;
                Log("idle_end", ("idle_ms", Math.Round((Time.unscaledTime - _lastInputTime) * 1000f)));
            }
            _lastInputTime = Time.unscaledTime;
        }
        else if (!_idle && Time.unscaledTime - _lastInputTime > ResearchConfig.Current.idleThresholdSeconds)
        {
            _idle = true;
            Log("idle_start", ("threshold_s", ResearchConfig.Current.idleThresholdSeconds));
        }

        if (Time.unscaledTime >= _nextFlush)
        {
            _nextFlush = Time.unscaledTime + 2f;
            _writer?.Flush();
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        Log(hasFocus ? "app_focus" : "app_blur");
        _writer?.Flush();
    }

    private void OnApplicationQuit()
    {
        EndSession("app_quit");
    }
}
