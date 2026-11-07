using System;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class SignupManager : MonoBehaviour
{
    public string playerName;
    public string gender, schoolGrade;
    public string schoolName = "";
    public int age;
    public InputField inputAge, inputName, inputSchoolGrade;
    public InputField inputSchoolName;   // criado em tempo de execução (CriarCampoEscola)

    private void Awake()
    {
        CriarCampoEscola();
    }

    // Campo "ESCOLA": arte (rótulo + caixa no estilo dos demais, em Resources/campo_escola.png)
    // posicionada sobre a arte do cadastro (DataBG), à direita de SÉRIE, e um campo de texto copiado
    // do campo de série (mesma fonte e cores). A arte foi feita com as letras da própria arte do jogo.
    private void CriarCampoEscola()
    {
        if (inputSchoolName != null || inputSchoolGrade == null) return;
        var fundo = transform.Find("DataBG") as RectTransform;
        var arte = Resources.Load<Sprite>("campo_escola");
        var fundoImg = fundo != null ? fundo.GetComponent<Image>() : null;
        if (fundo == null || arte == null || fundoImg == null || fundoImg.sprite == null) return;

        // Converte coordenadas em pixels da arte de fundo (origem no canto superior esquerdo) para o canvas
        Vector2 px = fundoImg.sprite.rect.size;
        Vector2 escala = new Vector2(fundo.sizeDelta.x / px.x, fundo.sizeDelta.y / px.y);
        Vector2 canto = fundo.anchoredPosition + new Vector2(-fundo.sizeDelta.x / 2f, fundo.sizeDelta.y / 2f);
        Vector2 Canvas(float x, float y) => canto + new Vector2(x * escala.x, -y * escala.y);

        // Arte: ocupa x 812..1098 e y 130..277 nos pixels da arte de fundo
        var go = new GameObject("SchoolArt", typeof(RectTransform), typeof(Image));
        var rt = (RectTransform)go.transform;
        rt.SetParent(transform, false);
        rt.SetSiblingIndex(fundo.GetSiblingIndex() + 1);
        rt.anchorMin = rt.anchorMax = fundo.anchorMin;
        rt.sizeDelta = new Vector2(286 * escala.x, 147 * escala.y);
        rt.anchoredPosition = (Canvas(812, 130) + Canvas(1098, 277)) / 2f;
        var img = go.GetComponent<Image>();
        img.sprite = arte;
        img.raycastTarget = false;

        // Campo de texto: cópia do campo de série, dentro da caixa (x 821..1098, y 184..277)
        var campo = Instantiate(inputSchoolGrade.gameObject, transform);
        campo.name = "SchoolName Field";
        campo.transform.SetSiblingIndex(rt.GetSiblingIndex() + 1);
        var crt = (RectTransform)campo.transform;
        var serie = (RectTransform)inputSchoolGrade.transform;
        crt.anchorMin = crt.anchorMax = serie.anchorMin;
        crt.sizeDelta = new Vector2((1098 - 821) * escala.x * serie.sizeDelta.x / (318f * escala.x), serie.sizeDelta.y);
        crt.anchoredPosition = (Canvas(821, 184) + Canvas(1098, 277)) / 2f;

        inputSchoolName = campo.GetComponent<InputField>();
        inputSchoolName.onEndEdit = new InputField.EndEditEvent();       // tira os eventos copiados do campo de série
        inputSchoolName.onValueChanged = new InputField.OnChangeEvent();
        inputSchoolName.contentType = InputField.ContentType.Standard;
        inputSchoolName.characterValidation = InputField.CharacterValidation.None;
        inputSchoolName.characterLimit = 60;
        inputSchoolName.text = "";
        if (inputSchoolName.textComponent != null)
        {
            inputSchoolName.textComponent.resizeTextForBestFit = false;   // o campo de série ajusta a fonte para os dígitos grandes
            inputSchoolName.textComponent.fontSize = 40;
            inputSchoolName.textComponent.alignment = TextAnchor.MiddleLeft;
        }
        if (inputSchoolName.placeholder is Text ph)
        {
            ph.text = "Sua escola";
            ph.resizeTextForBestFit = false;
            ph.fontSize = 34;
            ph.horizontalOverflow = HorizontalWrapMode.Overflow;
            ph.alignment = TextAnchor.MiddleLeft;
            var prt = ph.rectTransform;
            prt.anchorMin = Vector2.zero; prt.anchorMax = Vector2.one;
            prt.offsetMin = new Vector2(10, 6); prt.offsetMax = new Vector2(-10, -6);
        }
        inputSchoolName.onValueChanged.AddListener(v => schoolName = v.Trim());
    }

    void Start()
    {
        SetGender(0);

        // Learning analytics: no modo "code" o campo de nome pede o código do participante.
        if (ResearchConfig.Current.UsesParticipantCodes && inputName != null && inputName.placeholder is Text placeholder)
        {
            placeholder.text = "Código do participante";
        }
    }
    // O painel de cadastro abre por cima do menu e tem o próprio título; o logo do menu
    // (mesma imagem) fica escondido enquanto o cadastro está aberto, para não aparecer duas vezes.
    private GameObject _logoDoMenu;

    private void OnEnable()
    {
        var titulo = transform.Find("Title");
        var imagem = titulo != null ? titulo.GetComponent<Image>() : null;
        if (imagem == null || imagem.sprite == null) return;
        foreach (var outra in FindObjectsOfType<Image>())
        {
            if (outra.sprite == imagem.sprite && !outra.transform.IsChildOf(transform))
            {
                _logoDoMenu = outra.gameObject;
                _logoDoMenu.SetActive(false);
                break;
            }
        }
    }

    private void OnDisable()
    {
        if (_logoDoMenu != null) _logoDoMenu.SetActive(true);
        _logoDoMenu = null;
    }

    public void EnterName()
    {
        playerName = inputName.text.ToUpper();

    }
    public void SetBirthday()
    {
        int.TryParse(inputAge.text, out age);
    }

    public void SetSchoolGrade()
    {
        schoolGrade = inputSchoolGrade.text;
    }

    public void SetGender(int option)
    {
        if (option == 0)
        {
            gender = "m";
        }
        else if (option == 1)
        {
            gender = "f";
        }
        else
        {
            gender = "outro";
        }
    }

    /// <summary>Pode ser ligado a um Toggle "Autorizo o uso da câmera" no painel de cadastro.</summary>
    public void SetCameraConsent(bool value)
    {
        if (AffectManager.Instance != null)
        {
            AffectManager.Instance.SetConsent(value);
        }
    }

    public void SavePersonalDataAndStart()
    {
        DataManager.Instance.playerName = playerName;
        DataManager.Instance.SetPlayerNameOnUI(playerName);
        DataManager.Instance.playerAge = age;
        DataManager.Instance.playerGender = gender;
        DataManager.Instance.schoolGrade = schoolGrade;

        if (age != 0 && playerName != "" && playerName.Length >= 3 && schoolGrade != "")
        {
            SignupCameraPreview.Dismiss();   // a telinha da câmera some quando o estudante inicia
            TelemetryManager.EscolaInformada = schoolName;   // vai para o evento session_start (school_name)
            // Learning analytics: valida o código (modo "code"), pede consentimento (se exigido),
            // inicia a sessão de coleta e só então começa o jogo.
            if (ResearchSessionFlow.Instance != null)
            {
                ResearchSessionFlow.Instance.Begin(playerName, age, gender, schoolGrade, StartGameplay);
            }
            else
            {
                StartGameplay();
            }
        }
    }

    private void StartGameplay()
    {
        SceneManager.LoadScene("Area1");
        GameManager.Instance.StartGame();
    }
}

