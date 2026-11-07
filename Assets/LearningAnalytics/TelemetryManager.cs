using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Registro de eventos de learning analytics.
///
/// - Criado automaticamente ao iniciar o jogo (não precisa estar em nenhuma cena).
/// - Grava um arquivo JSON Lines por sessão em {DataFolder}/events_{sessionId}.jsonl.
/// - Cada evento tem "id" (único) e "seq" (sequência na sessão), para detectar perdas e duplicatas.
/// - Se o jogo fechar sem encerrar a sessão (queda de energia, travamento), a próxima abertura
///   registra um session_end com reason = "interrupted" e recovered = true.
/// - O nome da criança NÃO vai para o arquivo de eventos: no modo "name" ele fica apenas em
///   participants_map.csv; no modo "code" nenhum nome é gravado.
///
/// Uso em qualquer script:
///   TelemetryManager.Log("puzzle_attempt", ("level", "Musical"), ("attempt", 3));
/// Todo evento novo deve ser incluído em catalogo_eventos.json (um teste automatizado verifica).
/// </summary>
[DefaultExecutionOrder(-1000)]
public class TelemetryManager : MonoBehaviour
{
    public static TelemetryManager Instance { get; private set; }

    public const string SchemaVersion = "1.1";
    public const string OpenSessionMarker = ".sessao_aberta.json";

    public string SessionId { get; private set; }
    public string ParticipantId { get; private set; }
    public string Condition { get; private set; } = "";
    public bool SessionActive { get; private set; }
    public string CurrentScene { get; private set; } = "";
    public long EventCount => _seq;
    public string LastEventName { get; private set; } = "";
    public string CurrentEventsFile { get; private set; }

    // ------------------------------------------------------------------ Pasta de dados

    /// <summary>Usado pelos testes para isolar os dados; null = padrão.</summary>
    public static string DataFolderOverride;

    public static string DataFolder
    {
        get
        {
            if (!string.IsNullOrEmpty(DataFolderOverride)) return DataFolderOverride;
            string arg = ResearchConfig.CommandLineArg("-analyticsDataFolder");
            return !string.IsNullOrEmpty(arg) ? arg : Path.Combine(Application.persistentDataPath, "LearningAnalytics");
        }
    }

    private StreamWriter _writer;
    private double _sessionStartRealtime;
    private double _sceneEnterMs;
    private long _seq;

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
        go.AddComponent<ResearchSessionFlow>();
        go.AddComponent<ResearcherPanel>();
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

        try
        {
            string recovered = RecoverInterruptedSession(DataFolder);
            if (recovered != null) Debug.LogWarning($"[Telemetry] Sessão interrompida recuperada: {recovered}");
        }
        catch (Exception e)
        {
            Debug.LogError("[Telemetry] Falha ao recuperar sessão interrompida: " + e.Message);
        }
    }

    private void OnDestroy()
    {
        if (Instance != this) return;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
        Instance = null;
    }

    /// <summary>Milissegundos desde o início da sessão (relógio monotônico, usado para sincronizar com a câmera).</summary>
    public double SessionTimeMs => SessionActive ? (Time.realtimeSinceStartupAsDouble - _sessionStartRealtime) * 1000.0 : 0;

    // ------------------------------------------------------------------ Sessão

    /// <param name="playerNameOrCode">Nome (modo "name") ou código do participante (modo "code").</param>
    public void StartSession(string playerNameOrCode, int age, string gender, string schoolGrade,
        bool consent = true, bool cameraConsent = true)
    {
        var cfg = ResearchConfig.Current;
        if (!cfg.telemetryEnabled) return;
        if (SessionActive) EndSession("new_session");

        Directory.CreateDirectory(DataFolder);
        SessionId = DateTime.UtcNow.ToString("yyyyMMdd'T'HHmmss'Z'") + "-" + Guid.NewGuid().ToString("N").Substring(0, 6);
        ParticipantId = cfg.UsesParticipantCodes
            ? ParticipantCodes.Normalize(playerNameOrCode)
            : GetOrCreateParticipantId(playerNameOrCode, age, gender, schoolGrade);
        Condition = cfg.AssignCondition(ParticipantId);
        _sessionStartRealtime = Time.realtimeSinceStartupAsDouble;
        _sceneEnterMs = 0;
        _seq = 0;

        CurrentEventsFile = Path.Combine(DataFolder, $"events_{SessionId}.jsonl");
        _writer = new StreamWriter(CurrentEventsFile, true, new UTF8Encoding(false)) { AutoFlush = true };
        SessionActive = true;
        WriteOpenMarker();

        Log("session_start",
            ("schema", SchemaVersion),
            ("study_id", cfg.studyId),
            ("school_id", cfg.schoolId),
            ("class_id", cfg.classId),
            ("machine_id", cfg.EffectiveMachineId),
            ("condition", Condition),
            ("id_mode", cfg.UsesParticipantCodes ? "code" : "name"),
            ("consent", consent),
            ("camera_consent", cameraConsent),
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

        Debug.Log($"[Telemetry] Sessão {SessionId} iniciada para {ParticipantId} (condição '{Condition}'). Dados em {DataFolder}");

        if (AffectManager.Instance != null) AffectManager.Instance.OnSessionStarted(cameraConsent);
    }

    public void EndSession(string reason)
    {
        if (!SessionActive) return;
        if (AffectManager.Instance != null) AffectManager.Instance.OnSessionEnding();
        Log("session_end", ("reason", reason), ("duration_ms", Math.Round(SessionTimeMs)), ("recovered", false));
        SessionActive = false;
        _writer?.Flush();
        _writer?.Dispose();
        _writer = null;
        DeleteOpenMarker();
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

    // ------------------------------------------------------------------ Sessões interrompidas

    [Serializable]
    private class OpenMarker
    {
        public string session;
        public string participant;
        public string file;
        public string started_utc;
    }

    private void WriteOpenMarker()
    {
        var m = new OpenMarker { session = SessionId, participant = ParticipantId, file = Path.GetFileName(CurrentEventsFile), started_utc = DateTime.UtcNow.ToString("o") };
        File.WriteAllText(Path.Combine(DataFolder, OpenSessionMarker), JsonUtility.ToJson(m));
    }

    private static void DeleteOpenMarker()
    {
        string p = Path.Combine(DataFolder, OpenSessionMarker);
        if (File.Exists(p)) File.Delete(p);
    }

    /// <summary>
    /// Se existir uma sessão aberta de uma execução anterior, acrescenta a ela um session_end com
    /// reason = "interrupted" e recovered = true. Retorna o id da sessão recuperada (ou null).
    /// </summary>
    public static string RecoverInterruptedSession(string folder)
    {
        string markerPath = Path.Combine(folder, OpenSessionMarker);
        if (!File.Exists(markerPath)) return null;
        var m = JsonUtility.FromJson<OpenMarker>(File.ReadAllText(markerPath));
        string eventsPath = Path.Combine(folder, m?.file ?? "");
        if (m == null || string.IsNullOrEmpty(m.file) || !File.Exists(eventsPath))
        {
            File.Delete(markerPath);
            return null;
        }

        string last = null;
        foreach (var line in File.ReadLines(eventsPath))
            if (!string.IsNullOrWhiteSpace(line)) last = line;

        long seq = 0;
        double tms = 0;
        if (last != null)
        {
            var ms = Regex.Match(last, "\"seq\":(\\d+)");
            var mt = Regex.Match(last, "\"t_ms\":([0-9.]+)");
            if (ms.Success) seq = long.Parse(ms.Groups[1].Value, CultureInfo.InvariantCulture);
            if (mt.Success) tms = double.Parse(mt.Groups[1].Value, CultureInfo.InvariantCulture);
            if (last.Contains("\"event\":\"session_end\""))
            {
                File.Delete(markerPath);   // já estava encerrada
                return null;
            }
        }

        string line2 = BuildLine(seq + 1, tms, m.session, m.participant, "", "session_end",
            new (string, object)[] { ("reason", "interrupted"), ("duration_ms", Math.Round(tms)), ("recovered", true) });
        File.AppendAllText(eventsPath, line2 + "\n", new UTF8Encoding(false));
        File.Delete(markerPath);
        return m.session;
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
        _seq++;
        LastEventName = eventName;
        string line = BuildLine(_seq, SessionTimeMs, SessionId, ParticipantId, CurrentScene, eventName, data);
        try
        {
            _writer.WriteLine(line);
        }
        catch (Exception e)
        {
            Debug.LogError($"[Telemetry] Falha ao gravar evento: {e.Message}");
        }
    }

    /// <summary>Monta uma linha JSON do evento (usado também pela recuperação e pelos testes).</summary>
    public static string BuildLine(long seq, double tMs, string session, string participant, string scene,
        string eventName, (string key, object value)[] data)
    {
        var sb = new StringBuilder(320);
        sb.Append('{');
        AppendPair(sb, "id", Guid.NewGuid().ToString("N"), true);
        AppendPair(sb, "seq", seq);
        AppendPair(sb, "t", DateTime.UtcNow.ToString("o"));
        AppendPair(sb, "t_ms", Math.Round(tMs, 1));
        AppendPair(sb, "session", session);
        AppendPair(sb, "participant", participant);
        AppendPair(sb, "scene", scene);
        AppendPair(sb, "event", eventName);
        sb.Append(",\"data\":{");
        if (data != null)
        {
            for (int i = 0; i < data.Length; i++)
                AppendPair(sb, data[i].key, data[i].value, i == 0);
        }
        sb.Append("}}");
        return sb.ToString();
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
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        Log(hasFocus ? "app_focus" : "app_blur");
    }

    private void OnApplicationQuit()
    {
        EndSession("app_quit");
    }
}
