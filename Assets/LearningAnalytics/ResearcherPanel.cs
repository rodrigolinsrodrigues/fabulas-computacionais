using System;
using System.IO;
using UnityEngine;

/// <summary>
/// Painel do Pesquisador — aberto no jogo com Ctrl+Shift+R (protegido pelo PIN do research_config).
///
/// Mostra num só lugar o que o pesquisador precisa conferir na escola:
///  - estudo, escola, turma, máquina e condição configurados;
///  - sessão em andamento, participante, eventos gravados e o último evento;
///  - pasta de dados e espaço livre em disco;
///  - estado da câmera, com TESTE ao vivo (prévia da imagem + "rosto detectado"), sem gravar nada;
///  - botão para EXPORTAR a coleta num .zip (anonimizado) e abrir a pasta.
/// É uma ferramenta do pesquisador, não da criança: usa a interface simples do Unity (IMGUI).
/// </summary>
public class ResearcherPanel : MonoBehaviour
{
    public static ResearcherPanel Instance { get; private set; }

    public bool IsOpen { get; private set; }

    private bool _askingPin;
    private string _pinInput = "";
    private string _status = "";
    private Rect _window;
    private GUIStyle _label, _title, _big, _button, _box;
    private float _scale = 1f;
    private bool _previewStartedHere;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        bool ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        if (ctrl && shift && Input.GetKeyDown(KeyCode.R))
        {
            if (IsOpen || _askingPin) Close();
            else RequestOpen();
        }
    }

    public void RequestOpen()
    {
        if (string.IsNullOrEmpty(ResearchConfig.Current.researcherPin)) Open();
        else
        {
            _askingPin = true;
            _pinInput = "";
        }
    }

    public void Open()
    {
        _askingPin = false;
        IsOpen = true;
        _status = "";
        TelemetryManager.Log("researcher_panel", ("open", true));
    }

    public void Close()
    {
        if (IsOpen) TelemetryManager.Log("researcher_panel", ("open", false));
        IsOpen = false;
        _askingPin = false;
        if (_previewStartedHere && AffectManager.Instance != null) AffectManager.Instance.StopPreview();
        _previewStartedHere = false;
    }

    // ------------------------------------------------------------------ Ações (públicas para o autoteste)

    public string ExportNow()
    {
        try
        {
            string zip = DataExporter.Export();
            _status = "Exportado: " + zip;
            return zip;
        }
        catch (Exception e)
        {
            _status = "Erro ao exportar: " + e.Message;
            Debug.LogError("[ResearcherPanel] " + e);
            return null;
        }
    }

    public void ToggleCameraTest()
    {
        var am = AffectManager.Instance;
        if (am == null) return;
        if (am.IsPreviewing)
        {
            am.StopPreview();
            _previewStartedHere = false;
        }
        else if (!am.IsCapturing)
        {
            _previewStartedHere = am.StartPreview();
            if (!_previewStartedHere) _status = am.LastError;
        }
    }

    // ------------------------------------------------------------------ Interface

    private void EnsureStyles()
    {
        _scale = Mathf.Clamp(Screen.height / 900f, 0.8f, 2f);
        if (_label != null && Math.Abs(_label.fontSize - Mathf.RoundToInt(15 * _scale)) < 1) return;
        _label = new GUIStyle(GUI.skin.label) { fontSize = Mathf.RoundToInt(15 * _scale), richText = true, wordWrap = true };
        _title = new GUIStyle(_label) { fontSize = Mathf.RoundToInt(18 * _scale), fontStyle = FontStyle.Bold };
        _big = new GUIStyle(_label) { fontSize = Mathf.RoundToInt(22 * _scale), fontStyle = FontStyle.Bold };
        _button = new GUIStyle(GUI.skin.button) { fontSize = Mathf.RoundToInt(15 * _scale), fixedHeight = 34 * _scale };
        _box = new GUIStyle(GUI.skin.box);
    }

    private void OnGUI()
    {
        if (!IsOpen && !_askingPin) return;
        EnsureStyles();
        GUI.depth = -1000;

        if (_askingPin)
        {
            var r = new Rect(Screen.width / 2f - 200 * _scale, Screen.height / 2f - 90 * _scale, 400 * _scale, 180 * _scale);
            GUI.Box(r, GUIContent.none);
            GUILayout.BeginArea(new Rect(r.x + 16, r.y + 12, r.width - 32, r.height - 24));
            GUILayout.Label("Painel do Pesquisador — digite o PIN", _title);
            GUI.SetNextControlName("pin");
            _pinInput = GUILayout.PasswordField(_pinInput, '•', _label);
            GUI.FocusControl("pin");
            GUILayout.BeginHorizontal();
            bool enter = Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter);
            if (GUILayout.Button("Entrar", _button) || enter)
            {
                if (_pinInput == ResearchConfig.Current.researcherPin) Open();
                else _pinInput = "";
            }
            if (GUILayout.Button("Cancelar", _button)) Close();
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
            return;
        }

        float w = Mathf.Min(Screen.width - 40, 760 * _scale), h = Mathf.Min(Screen.height - 40, 820 * _scale);
        _window = new Rect((Screen.width - w) / 2f, (Screen.height - h) / 2f, w, h);
        GUI.Box(_window, GUIContent.none);
        GUI.Box(_window, GUIContent.none);
        GUILayout.BeginArea(new Rect(_window.x + 20, _window.y + 14, _window.width - 40, _window.height - 28));
        DrawContent();
        GUILayout.EndArea();
    }

    private void DrawContent()
    {
        var cfg = ResearchConfig.Current;
        var tm = TelemetryManager.Instance;
        var am = AffectManager.Instance;

        GUILayout.Label("Painel do Pesquisador — Fábulas Computacionais", _big);
        GUILayout.Space(6);

        GUILayout.Label("Estudo", _title);
        Row("Estudo / escola / turma", $"{Or(cfg.studyId)} / {Or(cfg.schoolId)} / {Or(cfg.classId)}");
        Row("Máquina", cfg.EffectiveMachineId);
        Row("Participantes", cfg.UsesParticipantCodes ? "por código de estudo (sem nomes)" : "por nome (mapa separado)");
        Row("Consentimento na tela", cfg.requireConsent ? "exigido" : "não exigido");
        Row("Condições", cfg.conditions != null && cfg.conditions.Length > 0 ? $"{string.Join(", ", cfg.conditions)} ({cfg.conditionAssignment})" : Or(cfg.condition));

        GUILayout.Space(8);
        GUILayout.Label("Coleta", _title);
        if (tm != null && tm.SessionActive)
        {
            Row("Sessão", $"<color=#2e8b57><b>em andamento</b></color> — {tm.SessionId}");
            Row("Participante / condição", $"{tm.ParticipantId} / {Or(tm.Condition)}");
            Row("Eventos gravados", $"{tm.EventCount} (último: {tm.LastEventName})");
            Row("Tempo de sessão", TimeSpan.FromMilliseconds(tm.SessionTimeMs).ToString(@"hh\:mm\:ss"));
        }
        else Row("Sessão", "<color=#b8860b>nenhuma em andamento</color> (começa ao concluir o cadastro)");
        Row("Pasta de dados", TelemetryManager.DataFolder);
        Row("Espaço livre", FreeSpace(TelemetryManager.DataFolder));

        GUILayout.Space(8);
        GUILayout.Label("Câmera (expressões faciais)", _title);
        Row("Configuração", cfg.affectCaptureEnabled ? "ligada no research_config" : "desligada no research_config");
        Row("Plugin MediaPipe", AffectManager.PluginAvailable ? "instalado" : "<color=#b22222>não instalado</color>");
        if (am != null)
        {
            string state = am.IsCapturing ? (am.IsPaused ? "coletando (pausada com F9)" : "<color=#2e8b57>coletando</color>")
                         : am.IsPreviewing ? "teste (nada é gravado)" : "desligada";
            Row("Estado", state);
            if (am.IsCapturing || am.IsPreviewing)
            {
                float face = am.RecentFaceRate;
                string faceTxt = face >= 0.8f ? $"<color=#2e8b57><b>✓ rosto detectado ({face:P0})</b></color>"
                               : face > 0f ? $"<color=#b8860b>rosto instável ({face:P0}) — ajuste posição/luz</color>"
                               : "<color=#b22222>nenhum rosto — ajuste a câmera</color>";
                Row("Detecção", faceTxt);
                if (am.Latest != null && am.Latest.facePresent)
                    Row("Leitura", $"sorriso {am.Latest.derived[0]:0.00} · sobrancelha {am.Latest.derived[2]:0.00} · cabeça {am.Latest.headYaw:0}°");
                Row("Processamento", $"{am.LastProcessMs:0} ms por quadro");
                if (am.CameraTexture != null)
                {
                    float pw = 320 * _scale, ph = pw * am.CameraTexture.height / Mathf.Max(1, am.CameraTexture.width);
                    var r = GUILayoutUtility.GetRect(pw, ph, GUILayout.ExpandWidth(false));
                    GUI.DrawTexture(new Rect(r.x + r.width, r.y, -r.width, r.height), am.CameraTexture); // espelhada
                }
            }
            if (!string.IsNullOrEmpty(am.LastError)) Row("Erro", $"<color=#b22222>{am.LastError}</color>");
        }

        GUILayout.FlexibleSpace();
        if (!string.IsNullOrEmpty(_status)) GUILayout.Label(_status, _label);
        GUILayout.BeginHorizontal();
        if (am != null && !am.IsCapturing && GUILayout.Button(am.IsPreviewing ? "Parar teste da câmera" : "Testar câmera", _button)) ToggleCameraTest();
        if (GUILayout.Button("Exportar coleta (.zip)", _button)) ExportNow();
        if (GUILayout.Button("Abrir pasta de dados", _button)) OpenFolder(TelemetryManager.DataFolder);
        if (GUILayout.Button("Fechar", _button)) Close();
        GUILayout.EndHorizontal();
        GUILayout.Label("<size=11>Ctrl+Shift+R abre/fecha este painel · F9 pausa a câmera durante a coleta</size>", _label);
    }

    private void Row(string key, string value)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label($"<b>{key}</b>", _label, GUILayout.Width(210 * _scale));
        GUILayout.Label(value ?? "", _label);
        GUILayout.EndHorizontal();
    }

    private static string Or(string s) => string.IsNullOrEmpty(s) ? "—" : s;

    private static string FreeSpace(string path)
    {
        try
        {
            string root = Path.GetPathRoot(Path.GetFullPath(path));
            var d = new DriveInfo(root);
            double gb = d.AvailableFreeSpace / 1024.0 / 1024 / 1024;
            return gb < 1 ? $"<color=#b22222>{gb:0.0} GB (pouco espaço)</color>" : $"{gb:0.0} GB";
        }
        catch
        {
            return "—";
        }
    }

    private static void OpenFolder(string path)
    {
        Directory.CreateDirectory(path);
        Application.OpenURL("file:///" + path.Replace("\\", "/"));
    }
}
