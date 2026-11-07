using UnityEngine;

/// <summary>
/// Telinha da câmera na tela de cadastro, para o pesquisador conferir se a webcam está bem
/// posicionada antes de a criança começar. Aparece só enquanto o painel de cadastro está aberto
/// (e a captura facial está ligada na configuração) e some quando o estudante clica para iniciar.
/// Nada é gravado enquanto a telinha está aberta: a câmera fica em modo de teste (prévia).
/// Desligue com "showCameraPreviewOnSignup": false no research_config.json.
/// </summary>
public class SignupCameraPreview : MonoBehaviour
{
    public static SignupCameraPreview Instance { get; private set; }

    /// <summary>A telinha está na tela agora.</summary>
    public bool Visible { get; private set; }

    private SignupManager _signup;
    private float _nextLookup;
    private bool _startedPreview;
    private bool _dismissed;          // o estudante já clicou em iniciar
    private bool _triedThisScreen;    // evita tentar ligar a câmera a cada quadro sem webcam
    private GUIStyle _label, _small;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    /// <summary>Chamado pelo SignupManager quando o cadastro é enviado.</summary>
    public static void Dismiss()
    {
        if (Instance == null) return;
        Instance._dismissed = true;
        Instance.Hide();
    }

    private bool ShouldShow()
    {
        var cfg = ResearchConfig.Current;
        if (!cfg.affectCaptureEnabled || !cfg.showCameraPreviewOnSignup || !AffectManager.PluginAvailable) return false;
        var am = AffectManager.Instance;
        var tm = TelemetryManager.Instance;
        if (am == null || am.IsCapturing || (tm != null && tm.SessionActive)) return false;
        if (ResearchSessionFlow.Instance != null && ResearchSessionFlow.Instance.OpenDialog != null) return false;
        if (ResearcherPanel.Instance != null && ResearcherPanel.Instance.IsOpen) return false;

        if (_signup == null && Time.unscaledTime >= _nextLookup)
        {
            _signup = FindObjectOfType<SignupManager>();   // só encontra se o painel estiver ativo
            _nextLookup = Time.unscaledTime + 0.3f;
        }
        bool open = _signup != null && _signup.isActiveAndEnabled;
        if (!open)
        {
            _signup = null;
            _dismissed = false;        // painel fechado: na próxima abertura a telinha volta
            _triedThisScreen = false;
        }
        return open && !_dismissed;
    }

    private void Update()
    {
        bool show = ShouldShow();
        if (show && !Visible)
        {
            Visible = true;
            var am = AffectManager.Instance;
            if (!_triedThisScreen && !am.IsPreviewing)
            {
                _triedThisScreen = true;
                _startedPreview = am.StartPreview();
            }
        }
        else if (!show && Visible)
        {
            Hide();
        }
    }

    private void Hide()
    {
        Visible = false;
        var am = AffectManager.Instance;
        if (_startedPreview && am != null && am.IsPreviewing) am.StopPreview();
        _startedPreview = false;
    }

    private void OnGUI()
    {
        if (!Visible) return;
        var am = AffectManager.Instance;
        if (am == null) return;

        float scale = Mathf.Max(1f, Screen.height / 720f);
        if (_label == null)
        {
            _label = new GUIStyle(GUI.skin.label) { richText = true, alignment = TextAnchor.MiddleCenter, wordWrap = true };
            _small = new GUIStyle(_label);
        }
        _label.fontSize = Mathf.RoundToInt(13 * scale);
        _small.fontSize = Mathf.RoundToInt(11 * scale);

        // Canto superior direito, pequeno: o formulário de cadastro fica à esquerda e o menu embaixo à direita.
        float w = 220 * scale;
        var tex = am.CameraTexture;
        bool failed = !string.IsNullOrEmpty(am.LastError) && !am.IsPreviewing;
        float h = failed ? 54 * scale : tex != null && tex.width > 0 ? w * tex.height / tex.width : w * 0.75f;
        float pad = 12 * scale, bar = 22 * scale, foot = 18 * scale;
        var box = new Rect(Screen.width - pad - w, pad, w, h + bar + foot);

        string msg;
        Color border;
        if (failed)
        {
            msg = "Câmera indisponível";
            border = new Color(0.55f, 0.55f, 0.55f);
        }
        else if (tex == null || am.Latest == null)
        {
            msg = "Ligando a câmera…";
            border = new Color(0.55f, 0.55f, 0.55f);
        }
        else
        {
            float face = am.RecentFaceRate;
            if (face >= 0.8f) { msg = "✓ Rosto detectado"; border = new Color(0.18f, 0.65f, 0.35f); }
            else if (face > 0f) { msg = "Rosto instável — ajuste a câmera ou a luz"; border = new Color(0.85f, 0.6f, 0.1f); }
            else { msg = "Nenhum rosto — ajuste a câmera"; border = new Color(0.8f, 0.2f, 0.2f); }
        }

        var old = GUI.color;
        GUI.color = border;
        GUI.DrawTexture(new Rect(box.x - 3, box.y - 3, box.width + 6, box.height + 6), Texture2D.whiteTexture);
        GUI.color = new Color(0.08f, 0.08f, 0.08f, 0.92f);
        GUI.DrawTexture(box, Texture2D.whiteTexture);
        GUI.color = old;

        var img = new Rect(box.x, box.y, w, h);
        if (failed) GUI.Label(new Rect(img.x + 6, img.y, img.width - 12, img.height), $"<color=#e0e0e0>{am.LastError}</color>", _label);
        else if (tex != null) GUI.DrawTexture(new Rect(img.x + img.width, img.y, -img.width, img.height), tex);   // espelhada

        GUI.color = border;
        GUI.DrawTexture(new Rect(box.x, img.yMax, w, bar), Texture2D.whiteTexture);
        GUI.color = old;
        GUI.Label(new Rect(box.x, img.yMax, w, bar), $"<b><color=#ffffff>{msg}</color></b>", _label);
        GUI.Label(new Rect(box.x, img.yMax + bar, w, foot), "<color=#cfcfcf>Prévia para o pesquisador · nada é gravado</color>", _small);
    }
}
