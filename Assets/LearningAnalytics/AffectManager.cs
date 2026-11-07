using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;
#if FABULAS_MEDIAPIPE
using Mediapipe.Tasks.Vision.FaceLandmarker;
using MPBaseOptions = Mediapipe.Tasks.Core.BaseOptions;
using MPRunningMode = Mediapipe.Tasks.Vision.Core.RunningMode;
using MPImageProcessingOptions = Mediapipe.Tasks.Vision.Core.ImageProcessingOptions;
using MPFaceLandmarker = Mediapipe.Tasks.Vision.FaceLandmarker.FaceLandmarker;
#endif

/// <summary>
/// Captura de expressões faciais pela webcam com o MediaPipe FaceLandmarker
/// (MediaPipe Unity Plugin, homuler, v0.15.0).
///
/// Modos:
///  - Coleta: começa com a sessão (se research_config tiver "affectCaptureEnabled": true e houver
///    consentimento); grava affect_{sessionId}.csv e o resumo "affect_summary" no log de eventos.
///  - Teste (preview): ligado pelo Painel do Pesquisador; mostra a câmera e a detecção do rosto
///    SEM gravar nada, para conferir posição e iluminação antes de a criança jogar.
///
/// Processa tudo LOCALMENTE e NUNCA grava imagem ou vídeo — só números.
/// F9 pausa/retoma a coleta (o evento é registrado).
/// O código do MediaPipe só é compilado com o símbolo FABULAS_MEDIAPIPE (ativado pelo
/// MediaPipeSetup quando o plugin está instalado).
/// </summary>
public class AffectManager : MonoBehaviour
{
    public enum Mode { Off, Preview, Capture }

    public static AffectManager Instance { get; private set; }

    public Mode CurrentMode { get; private set; } = Mode.Off;
    public bool IsCapturing => CurrentMode == Mode.Capture;
    public bool IsPreviewing => CurrentMode == Mode.Preview;
    public bool IsPaused { get; private set; }
    public bool Consent { get; private set; } = true;

    /// <summary>Última amostra processada (null antes da primeira).</summary>
    public FaceAffectSample Latest { get; private set; }

    /// <summary>Textura da webcam (para a prévia do Painel do Pesquisador); null se a câmera estiver desligada.</summary>
    public Texture CameraTexture { get; private set; }

    /// <summary>Tempo de processamento do último quadro (ms).</summary>
    public float LastProcessMs { get; private set; }

    /// <summary>Último erro da câmera/MediaPipe (vazio se tudo bem).</summary>
    public string LastError { get; private set; } = "";

    /// <summary>Proporção de amostras com rosto nas últimas ~20 amostras.</summary>
    public float RecentFaceRate
    {
        get
        {
            if (_recent.Count == 0) return 0f;
            int n = 0;
            foreach (bool b in _recent) if (b) n++;
            return (float)n / _recent.Count;
        }
    }

    /// <summary>Disparado no thread principal a cada amostra (útil para gatilhos de autorrelato).</summary>
    public event Action<FaceAffectSample> SampleCaptured;

    public static bool PluginAvailable =>
#if FABULAS_MEDIAPIPE
        true;
#else
        false;
#endif

    private ResearchConfig Cfg => ResearchConfig.Current;

    private StreamWriter _csv;
    private Coroutine _loop;
    private readonly Queue<bool> _recent = new Queue<bool>();

    // Resumo periódico
    private int _sumSamples, _sumFace;
    private readonly float[] _sumDerived = new float[FaceAffectSample.DerivedNames.Length];
    private float _nextSummaryTime;

    private GUIStyle _indicatorStyle;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    // ------------------------------------------------------------------ API pública

    /// <summary>
    /// Pode ser ligado a um Toggle de consentimento no cadastro. Se false, a coleta
    /// não começa (ou é interrompida imediatamente).
    /// </summary>
    public void SetConsent(bool value)
    {
        Consent = value;
        TelemetryManager.Log("affect_consent", ("value", value));
        if (!value) StopCapture("consent_revoked");
        else if (TelemetryManager.Instance != null && TelemetryManager.Instance.SessionActive) OnSessionStarted(true);
    }

    public void SetPaused(bool paused)
    {
        if (!IsCapturing || IsPaused == paused) return;
        IsPaused = paused;
        TelemetryManager.Log(paused ? "affect_capture_paused" : "affect_capture_resumed");
    }

    /// <summary>Liga a câmera e a detecção sem gravar nada (Painel do Pesquisador).</summary>
    public bool StartPreview()
    {
        if (CurrentMode != Mode.Off) return true;
        if (!PluginAvailable)
        {
            LastError = "MediaPipe Unity Plugin não instalado (captura facial indisponível).";
            return false;
        }
        LastError = "";
        CurrentMode = Mode.Preview;
        StartLoop();
        return true;
    }

    public void StopPreview()
    {
        if (CurrentMode != Mode.Preview) return;
        CurrentMode = Mode.Off;
        StopLoop();
    }

    // Chamados pelo TelemetryManager
    internal void OnSessionStarted(bool cameraConsent)
    {
        Consent = cameraConsent;
        if (IsCapturing) return;
        if (!Cfg.affectCaptureEnabled)
        {
            TelemetryManager.Log("affect_capture_disabled", ("reason", "config"));
            return;
        }
        if (!Consent)
        {
            TelemetryManager.Log("affect_capture_disabled", ("reason", "no_consent"));
            return;
        }
        if (!PluginAvailable)
        {
            Fail("plugin_not_installed", "MediaPipe Unity Plugin não encontrado (símbolo FABULAS_MEDIAPIPE ausente).");
            return;
        }
        StartCapture();
    }

    internal void OnSessionEnding()
    {
        StopCapture("session_end");
    }

    // ------------------------------------------------------------------ Ciclo de captura

    private void StartCapture()
    {
        bool cameraOn = CurrentMode == Mode.Preview;   // aproveita a câmera ligada no teste
        CurrentMode = Mode.Capture;
        IsPaused = false;
        ResetSummary();
        OpenCsv();
        if (cameraOn) LogCaptureStart();
        else StartLoop();
    }

    private void StopCapture(string reason)
    {
        if (!IsCapturing) return;
        CurrentMode = Mode.Off;
        StopLoop();
        FlushSummary();
        if (_csv != null)
        {
            _csv.Flush();
            _csv.Dispose();
            _csv = null;
        }
        TelemetryManager.Log("affect_capture_stop", ("reason", reason));
    }

    private void StartLoop()
    {
#if FABULAS_MEDIAPIPE
        if (_loop == null) _loop = StartCoroutine(CaptureLoop());
#endif
    }

    private void StopLoop()
    {
        if (_loop != null) StopCoroutine(_loop);
        _loop = null;
        ReleaseMediaPipe();
        CameraTexture = null;
        _recent.Clear();
    }

    private void Fail(string reason, string detail)
    {
        LastError = detail;
        Debug.LogWarning($"[Affect] Captura indisponível ({reason}): {detail}");
        TelemetryManager.Log("affect_unavailable", ("reason", reason), ("detail", detail));
        if (IsCapturing) StopCapture("error:" + reason);
        else if (IsPreviewing) StopPreview();
    }

    private void Update()
    {
        if (!IsCapturing) return;
        if (Input.GetKeyDown(KeyCode.F9)) SetPaused(!IsPaused);
        if (Time.unscaledTime >= _nextSummaryTime) FlushSummary();
    }

    private void OnApplicationQuit()
    {
        StopCapture("app_quit");
        StopPreview();
#if FABULAS_MEDIAPIPE
        Mediapipe.Protobuf.ResetLogHandler();
#endif
    }

    // ------------------------------------------------------------------ Gravação

    private void OpenCsv()
    {
        var tm = TelemetryManager.Instance;
        if (tm == null || !tm.SessionActive) return;
        Directory.CreateDirectory(TelemetryManager.DataFolder);
        string path = Path.Combine(TelemetryManager.DataFolder, $"affect_{tm.SessionId}.csv");
        bool exists = File.Exists(path) && new FileInfo(path).Length > 0;
        _csv = new StreamWriter(path, true, new UTF8Encoding(false));
        if (exists) return;   // captura retomada na mesma sessão: não repete o cabeçalho

        var header = new StringBuilder("utc,t_ms,session,participant,scene,face_present,head_yaw,head_pitch,head_roll");
        foreach (var n in FaceAffectSample.DerivedNames) header.Append(',').Append(n);
        if (Cfg.saveRawBlendshapes)
            foreach (var n in FaceAffectSample.BlendshapeNames) header.Append(",bs_").Append(n.TrimStart('_'));
        _csv.WriteLine(header.ToString());
    }

    private void Record(FaceAffectSample s)
    {
        Latest = s;
        _recent.Enqueue(s.facePresent);
        while (_recent.Count > 20) _recent.Dequeue();

        if (IsCapturing)
        {
            _sumSamples++;
            if (s.facePresent)
            {
                _sumFace++;
                for (int i = 0; i < _sumDerived.Length; i++) _sumDerived[i] += s.derived[i];
            }

            if (_csv != null)
            {
                var tm = TelemetryManager.Instance;
                var sb = new StringBuilder(1024);
                sb.Append(s.utc.ToString("o")).Append(',')
                  .Append(F(s.sessionMs)).Append(',')
                  .Append(tm.SessionId).Append(',')
                  .Append(tm.ParticipantId).Append(',')
                  .Append(Csv(s.scene)).Append(',')
                  .Append(s.facePresent ? '1' : '0').Append(',')
                  .Append(F(s.headYaw)).Append(',').Append(F(s.headPitch)).Append(',').Append(F(s.headRoll));
                foreach (var v in s.derived) sb.Append(',').Append(F(v));
                if (Cfg.saveRawBlendshapes)
                    foreach (var v in s.blendshapes) sb.Append(',').Append(F(v));
                _csv.WriteLine(sb.ToString());
            }
        }

        SampleCaptured?.Invoke(s);
    }

    private void ResetSummary()
    {
        _sumSamples = 0;
        _sumFace = 0;
        Array.Clear(_sumDerived, 0, _sumDerived.Length);
        _nextSummaryTime = Time.unscaledTime + Mathf.Max(1f, Cfg.affectSummaryIntervalSeconds);
    }

    private void FlushSummary()
    {
        if (_sumSamples > 0)
        {
            var means = new Dictionary<string, float>();
            for (int i = 0; i < _sumDerived.Length; i++)
                means[FaceAffectSample.DerivedNames[i]] = _sumFace > 0 ? _sumDerived[i] / _sumFace : float.NaN;

            TelemetryManager.Log("affect_summary",
                ("samples", _sumSamples),
                ("face_ratio", (float)_sumFace / _sumSamples),
                ("paused", IsPaused),
                ("means", means));
        }
        _csv?.Flush();
        ResetSummary();
    }

    private void LogCaptureStart()
    {
        TelemetryManager.Log("affect_capture_start",
            ("camera", _cameraName),
            ("resolution", CameraTexture != null ? $"{CameraTexture.width}x{CameraTexture.height}" : ""),
            ("sample_rate_hz", Cfg.affectSampleRateHz),
            ("model", Path.GetFileName(Cfg.modelFileName)),
            ("plugin", "homuler/MediaPipeUnityPlugin 0.15.0"));
    }

    private string _cameraName = "";

    private static string F(double v) =>
        double.IsNaN(v) || double.IsInfinity(v) ? "" : v.ToString("0.####", CultureInfo.InvariantCulture);

    private static string Csv(string s) =>
        string.IsNullOrEmpty(s) ? "" : (s.Contains(",") || s.Contains("\"") ? "\"" + s.Replace("\"", "\"\"") + "\"" : s);

    // ------------------------------------------------------------------ Indicador visível

    private void OnGUI()
    {
        if (!IsCapturing || !Cfg.showCameraIndicator) return;
        if (_indicatorStyle == null)
        {
            _indicatorStyle = new GUIStyle(GUI.skin.box)
            {
                fontSize = Mathf.Max(12, Screen.height / 60),
                alignment = TextAnchor.MiddleCenter,
                richText = true,
            };
            _indicatorStyle.normal.textColor = Color.white;
        }
        string text = IsPaused ? "Câmera pausada (F9)" : "<color=#ff5555>●</color> Câmera ativa (pesquisa)";
        var size = _indicatorStyle.CalcSize(new GUIContent(text));
        var rect = new Rect(Screen.width - size.x - 16, 8, size.x + 8, size.y + 4);
        GUI.Box(rect, text, _indicatorStyle);
    }

    // ------------------------------------------------------------------ MediaPipe

#if FABULAS_MEDIAPIPE
    private WebCamTexture _cam;
    private Texture2D _frame;
    private Color32[] _src, _dst;
    private MPFaceLandmarker _landmarker;
    private FaceLandmarkerResult _result;
    private long _lastTimestampMs = -1;
    private Dictionary<string, int> _bsIndex;

    private IEnumerator CaptureLoop()
    {
        // 1) Modelo
        string modelPath = Path.Combine(Application.streamingAssetsPath, Cfg.modelFileName);
        if (!File.Exists(modelPath))
        {
            _loop = null;
            Fail("model_not_found", $"Modelo não encontrado em {modelPath}. Use o menu Fabulas > Learning Analytics > Configurar MediaPipe.");
            yield break;
        }

        if (!TryCreateLandmarker(File.ReadAllBytes(modelPath)))
        {
            _loop = null;
            yield break;
        }

        // 2) Câmera
        var devices = WebCamTexture.devices;
        if (devices.Length == 0)
        {
            _loop = null;
            Fail("no_camera", "Nenhuma webcam encontrada.");
            yield break;
        }
        string deviceName = devices[0].name;
        if (!string.IsNullOrEmpty(Cfg.cameraDeviceName))
        {
            foreach (var d in devices)
                if (d.name == Cfg.cameraDeviceName) deviceName = d.name;
        }
        _cameraName = deviceName;

        _cam = new WebCamTexture(deviceName, Cfg.cameraWidth, Cfg.cameraHeight, Cfg.cameraFps);
        _cam.Play();

        float timeout = Time.realtimeSinceStartup + 10f;
        while (_cam.width <= 16 && Time.realtimeSinceStartup < timeout) yield return null;
        if (_cam.width <= 16)
        {
            _loop = null;
            Fail("camera_start_failed", $"A webcam '{deviceName}' não iniciou (em uso por outro programa?).");
            yield break;
        }

        CameraTexture = _cam;
        AllocateBuffers(_cam.width, _cam.height);
        if (IsCapturing) LogCaptureStart();

        // 3) Amostragem
        var wait = new WaitForSecondsRealtime(1f / Mathf.Clamp(Cfg.affectSampleRateHz, 0.2f, 15f));
        while (CurrentMode != Mode.Off)
        {
            yield return wait;
            if (CurrentMode == Mode.Off) break;
            if (IsPaused || !_cam.isPlaying) continue;
            if (_cam.width != _frame.width || _cam.height != _frame.height) AllocateBuffers(_cam.width, _cam.height);
            ProcessFrame();
        }
    }

    private bool TryCreateLandmarker(byte[] model)
    {
        try
        {
            Mediapipe.Protobuf.SetLogHandler(Mediapipe.Protobuf.DefaultLogHandler);
            var options = new FaceLandmarkerOptions(
                new MPBaseOptions(MPBaseOptions.Delegate.CPU, modelAssetBuffer: model),
                runningMode: MPRunningMode.VIDEO,
                numFaces: 1,
                minFaceDetectionConfidence: 0.5f,
                minFacePresenceConfidence: 0.5f,
                minTrackingConfidence: 0.5f,
                outputFaceBlendshapes: true,
                outputFaceTransformationMatrixes: true);
            _landmarker = MPFaceLandmarker.CreateFromOptions(options);
            _result = FaceLandmarkerResult.Alloc(1, true, true);

            _bsIndex = new Dictionary<string, int>();
            for (int i = 0; i < FaceAffectSample.BlendshapeNames.Length; i++)
                _bsIndex[FaceAffectSample.BlendshapeNames[i]] = i;
            return true;
        }
        catch (Exception e)
        {
            // DllNotFoundException, modelo inválido etc.
            Fail("landmarker_init_failed", e.GetType().Name + ": " + e.Message);
            return false;
        }
    }

    private void AllocateBuffers(int w, int h)
    {
        if (_frame != null) Destroy(_frame);
        _frame = new Texture2D(w, h, TextureFormat.RGBA32, false);
        _src = new Color32[w * h];
        _dst = new Color32[w * h];
    }

    private void ProcessFrame()
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        int w = _frame.width, h = _frame.height;
        _cam.GetPixels32(_src);

        // Texturas do Unity vêm de baixo para cima; o MediaPipe espera de cima para baixo.
        if (_cam.videoVerticallyMirrored)
        {
            Array.Copy(_src, _dst, _src.Length);
        }
        else
        {
            for (int y = 0; y < h; y++)
                Array.Copy(_src, y * w, _dst, (h - 1 - y) * w, w);
        }
        _frame.SetPixels32(_dst); // só a cópia em CPU é usada; Apply() não é necessário

        long ts = (long)(Time.realtimeSinceStartupAsDouble * 1000.0);
        if (ts <= _lastTimestampMs) ts = _lastTimestampMs + 1; // VIDEO exige timestamps crescentes
        _lastTimestampMs = ts;

        var tm = TelemetryManager.Instance;
        var sample = new FaceAffectSample
        {
            utc = DateTime.UtcNow,
            sessionMs = tm != null ? tm.SessionTimeMs : 0,
            scene = tm != null ? tm.CurrentScene : "",
        };

        bool detected;
        try
        {
            // O Packet assume a posse do Image (não é preciso Dispose).
            var image = new Mediapipe.Image(Mediapipe.ImageFormat.Types.Format.Srgba, _frame);
            var processing = new MPImageProcessingOptions(rotationDegrees: _cam.videoRotationAngle);
            detected = _landmarker.TryDetectForVideo(image, ts, processing, ref _result);
        }
        catch (Exception e)
        {
            Fail("detect_failed", e.GetType().Name + ": " + e.Message);
            return;
        }

        if (detected && _result.faceBlendshapes != null && _result.faceBlendshapes.Count > 0)
        {
            sample.ClearFace();
            sample.facePresent = true;
            foreach (var cat in _result.faceBlendshapes[0].categories)
            {
                if (cat.categoryName != null && _bsIndex.TryGetValue(cat.categoryName, out int idx))
                    sample.blendshapes[idx] = cat.score;
            }
            sample.ComputeDerived();
            if (_result.facialTransformationMatrixes != null && _result.facialTransformationMatrixes.Count > 0)
                sample.SetHeadPose(_result.facialTransformationMatrixes[0]);
        }
        else
        {
            sample.ClearFace();
        }

        LastProcessMs = (float)sw.Elapsed.TotalMilliseconds;
        Record(sample);
    }

    private void ReleaseMediaPipe()
    {
        if (_cam != null)
        {
            if (_cam.isPlaying) _cam.Stop();
            Destroy(_cam);
            _cam = null;
        }
        if (_frame != null)
        {
            Destroy(_frame);
            _frame = null;
        }
        try
        {
            _landmarker?.Close();
        }
        catch (Exception e)
        {
            Debug.LogWarning("[Affect] Erro ao fechar o FaceLandmarker: " + e.Message);
        }
        _landmarker = null;
        _lastTimestampMs = -1;
    }
#else
    private void ReleaseMediaPipe() { }
#endif
}
