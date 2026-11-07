using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Fluxo de início da sessão de pesquisa, chamado pelo SignupManager ao concluir o cadastro:
///  1. No modo "code", valida o código do participante (com dígito verificador).
///  2. Se research_config exigir ("requireConsent"), mostra a tela de consentimento
///     (texto em StreamingAssets/consent_text.txt), com opção separada para a câmera.
///  3. Inicia a sessão de telemetria (só com consentimento) e segue para o jogo.
/// Recusar o consentimento NÃO impede a criança de jogar: apenas nada é registrado.
///
/// A interface é criada por código (uGUI + TextMeshPro), sem depender de cenas, e funciona
/// com mouse, teclado e controle.
/// </summary>
public class ResearchSessionFlow : MonoBehaviour
{
    public static ResearchSessionFlow Instance { get; private set; }

    /// <summary>Diálogo aberto no momento (ou null). Exposto para o autoteste.</summary>
    public GameObject OpenDialog { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    /// <summary>Verifica o participante, pede consentimento se preciso, inicia a sessão e chama onReady.</summary>
    public void Begin(string nameOrCode, int age, string gender, string schoolGrade, Action onReady)
    {
        var cfg = ResearchConfig.Current;
        if (OpenDialog != null) return;   // já há um diálogo aberto

        if (cfg.UsesParticipantCodes && !ParticipantCodes.IsValid(nameOrCode))
        {
            ShowDialog("Código não reconhecido",
                $"O código \"{nameOrCode}\" não é válido.\nConfira o código na sua etiqueta e digite de novo (são {ParticipantCodes.CodeLength} letras e números).",
                ("OK", null));
            return;
        }

        if (!cfg.telemetryEnabled)
        {
            onReady?.Invoke();
            return;
        }

        if (!cfg.requireConsent)
        {
            TelemetryManager.Instance?.StartSession(nameOrCode, age, gender, schoolGrade, true, true);
            onReady?.Invoke();
            return;
        }

        ShowConsent(cfg.affectCaptureEnabled, (agreed, camera) =>
        {
            if (agreed) TelemetryManager.Instance?.StartSession(nameOrCode, age, gender, schoolGrade, true, camera);
            onReady?.Invoke();
        });
    }

    // ------------------------------------------------------------------ Consentimento

    public static string LoadConsentText()
    {
        string path = Path.Combine(Application.streamingAssetsPath, ResearchConfig.Current.consentTextFile ?? "consent_text.txt");
        try
        {
            if (File.Exists(path)) return File.ReadAllText(path);
        }
        catch (Exception e)
        {
            Debug.LogWarning("[ResearchSessionFlow] Não foi possível ler o texto de consentimento: " + e.Message);
        }
        return "Este jogo faz parte de uma pesquisa. Enquanto você joga, o jogo registra o que acontece nas fases " +
               "(por exemplo, quantas tentativas você fez). Seu nome não é registrado. Você pode parar quando quiser.";
    }

    private void ShowConsent(bool askCamera, Action<bool, bool> done)
    {
        Toggle cameraToggle = null;
        var dialog = BuildDialog("Participação na pesquisa", LoadConsentText(), out RectTransform content);

        if (askCamera)
        {
            cameraToggle = BuildToggle(content, "Autorizo o uso da câmera (só números sobre as expressões, nenhuma imagem é gravada)");
        }

        var buttons = BuildButtonRow(content);
        var agree = BuildButton(buttons, "Concordo e quero participar", new Color(0.18f, 0.55f, 0.27f));
        var decline = BuildButton(buttons, "Não quero participar", new Color(0.55f, 0.2f, 0.2f));
        agree.name = "BotaoConcordo";
        decline.name = "BotaoRecuso";

        agree.onClick.AddListener(() =>
        {
            bool camera = cameraToggle != null && cameraToggle.isOn;
            CloseDialog();
            done(true, camera);
        });
        decline.onClick.AddListener(() =>
        {
            CloseDialog();
            done(false, false);
        });
        Select(agree.gameObject);
    }

    // ------------------------------------------------------------------ Diálogo genérico

    public void ShowDialog(string title, string body, params (string label, Action action)[] actions)
    {
        BuildDialog(title, body, out RectTransform content);
        var row = BuildButtonRow(content);
        Button first = null;
        foreach (var (label, action) in actions)
        {
            var b = BuildButton(row, label, new Color(0.2f, 0.35f, 0.6f));
            b.onClick.AddListener(() =>
            {
                CloseDialog();
                action?.Invoke();
            });
            if (first == null) first = b;
        }
        if (first != null) Select(first.gameObject);
    }

    public void CloseDialog()
    {
        if (OpenDialog != null) Destroy(OpenDialog);
        OpenDialog = null;
    }

    // ------------------------------------------------------------------ Construção da interface

    private GameObject BuildDialog(string title, string body, out RectTransform content)
    {
        EnsureEventSystem();
        var root = new GameObject("[DialogoPesquisa]", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        DontDestroyOnLoad(root);
        var canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 5000;
        var scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        var shade = NewUI("Fundo", root.transform, typeof(Image));
        Stretch(shade);
        shade.GetComponent<Image>().color = new Color(0, 0, 0, 0.75f);

        var card = NewUI("Cartao", root.transform, typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        card.anchorMin = card.anchorMax = card.pivot = new Vector2(0.5f, 0.5f);
        card.sizeDelta = new Vector2(1200, 0);
        card.GetComponent<Image>().color = new Color(0.98f, 0.97f, 0.93f);
        var layout = card.GetComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(60, 60, 50, 50);
        layout.spacing = 28;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandHeight = false;
        card.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var t = NewText(card, title, 46, FontStyles.Bold, new Color(0.12f, 0.31f, 0.47f));
        t.alignment = TextAlignmentOptions.Center;
        NewText(card, body, 30, FontStyles.Normal, new Color(0.15f, 0.15f, 0.15f));

        OpenDialog = root;
        content = card;
        return root;
    }

    private static Toggle BuildToggle(RectTransform parent, string label)
    {
        var row = NewUI("LinhaCamera", parent, typeof(HorizontalLayoutGroup), typeof(Toggle));
        var h = row.GetComponent<HorizontalLayoutGroup>();
        h.spacing = 20;
        h.childControlWidth = false;
        h.childControlHeight = false;
        h.childAlignment = TextAnchor.MiddleLeft;

        var box = NewUI("Caixa", row, typeof(Image));
        box.sizeDelta = new Vector2(48, 48);
        box.GetComponent<Image>().color = Color.white;
        var outline = box.gameObject.AddComponent<UnityEngine.UI.Outline>();   // o projeto tem outro "Outline" (QuickOutline)
        outline.effectColor = new Color(0.3f, 0.3f, 0.3f);
        outline.effectDistance = new Vector2(3, 3);
        var check = NewUI("Marca", box, typeof(Image));
        check.anchorMin = new Vector2(0.18f, 0.18f);
        check.anchorMax = new Vector2(0.82f, 0.82f);
        check.offsetMin = check.offsetMax = Vector2.zero;
        check.GetComponent<Image>().color = new Color(0.18f, 0.55f, 0.27f);

        var text = NewText(row, label, 28, FontStyles.Normal, new Color(0.15f, 0.15f, 0.15f));
        text.rectTransform.sizeDelta = new Vector2(1000, 80);

        var toggle = row.GetComponent<Toggle>();
        toggle.targetGraphic = box.GetComponent<Image>();
        toggle.graphic = check.GetComponent<Image>();
        toggle.isOn = false;
        return toggle;
    }

    private static RectTransform BuildButtonRow(RectTransform parent)
    {
        var row = NewUI("Botoes", parent, typeof(HorizontalLayoutGroup));
        var h = row.GetComponent<HorizontalLayoutGroup>();
        h.spacing = 30;
        h.childAlignment = TextAnchor.MiddleCenter;
        h.childControlWidth = true;
        h.childControlHeight = true;
        h.childForceExpandWidth = true;
        return row;
    }

    private static Button BuildButton(RectTransform parent, string label, Color color)
    {
        var go = NewUI("Botao", parent, typeof(Image), typeof(Button), typeof(LayoutElement));
        go.GetComponent<Image>().color = color;
        go.GetComponent<LayoutElement>().minHeight = 90;
        var b = go.GetComponent<Button>();
        var colors = b.colors;
        colors.highlightedColor = new Color(1f, 0.95f, 0.6f);
        colors.selectedColor = new Color(1f, 0.95f, 0.6f);
        b.colors = colors;
        var txt = NewText(go, label, 30, FontStyles.Bold, Color.white);
        txt.alignment = TextAlignmentOptions.Center;
        Stretch(txt.rectTransform);
        return b;
    }

    private static RectTransform NewUI(string name, Transform parent, params Type[] components)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        foreach (var c in components) go.AddComponent(c);
        return go.GetComponent<RectTransform>();
    }

    private static TextMeshProUGUI NewText(Transform parent, string text, float size, FontStyles style, Color color)
    {
        var rt = NewUI("Texto", parent, typeof(TextMeshProUGUI));
        var t = rt.GetComponent<TextMeshProUGUI>();
        t.text = text;
        t.fontSize = size;
        t.fontStyle = style;
        t.color = color;
        t.enableWordWrapping = true;
        return t;
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    private static void EnsureEventSystem()
    {
        if (EventSystem.current != null) return;
        var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        DontDestroyOnLoad(es);
    }

    private static void Select(GameObject go)
    {
        if (EventSystem.current == null) return;
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(go);
    }
}
