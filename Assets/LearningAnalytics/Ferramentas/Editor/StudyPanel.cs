using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Painel do Estudo (Fabulas > Learning Analytics > Painel do Estudo).
/// Configura a coleta sem editar JSON, gera códigos de participante com etiquetas para imprimir,
/// valida a configuração e exporta os dados coletados nesta máquina.
/// </summary>
public class StudyPanel : EditorWindow
{
    private ResearchConfig _cfg;
    private string _conditionsText = "";
    private int _codeCount = 30;
    private Vector2 _scroll;
    private string _message = "";

    public static string StudyFolder => Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Pesquisa"));

    [MenuItem("Fabulas/Learning Analytics/Painel do Estudo", priority = 0)]
    public static void Open() => GetWindow<StudyPanel>("Painel do Estudo").minSize = new Vector2(520, 640);

    private void OnEnable() => Reload();

    private void Reload()
    {
        _cfg = ResearchConfig.LoadFrom(Path.Combine(Application.streamingAssetsPath, ResearchConfig.FileName));
        _conditionsText = string.Join(", ", _cfg.conditions ?? new string[0]);
        _message = "";
    }

    private void OnGUI()
    {
        if (_cfg == null) Reload();
        _scroll = EditorGUILayout.BeginScrollView(_scroll);

        EditorGUILayout.LabelField("Identificação do estudo", EditorStyles.boldLabel);
        _cfg.studyId = EditorGUILayout.TextField("Estudo (studyId)", _cfg.studyId);
        _cfg.schoolId = EditorGUILayout.TextField("Escola (schoolId)", _cfg.schoolId);
        _cfg.classId = EditorGUILayout.TextField("Turma (classId)", _cfg.classId);
        _cfg.machineId = EditorGUILayout.TextField(new GUIContent("Máquina (machineId)", "Vazio = nome do computador"), _cfg.machineId);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Participantes e consentimento", EditorStyles.boldLabel);
        int mode = EditorGUILayout.Popup("Identificação", _cfg.UsesParticipantCodes ? 1 : 0,
            new[] { "Nome no cadastro (mapa separado)", "Código de estudo (sem nomes)" });
        _cfg.participantIdMode = mode == 1 ? "code" : "name";
        _cfg.requireConsent = EditorGUILayout.Toggle("Tela de consentimento", _cfg.requireConsent);
        _cfg.consentTextFile = EditorGUILayout.TextField("Texto (StreamingAssets)", _cfg.consentTextFile);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Condições experimentais (A/B)", EditorStyles.boldLabel);
        _conditionsText = EditorGUILayout.TextField(new GUIContent("Condições", "Separadas por vírgula. Ex.: A, B"), _conditionsText);
        int assign = Array.IndexOf(new[] { "fixed", "random", "by_participant" }, _cfg.conditionAssignment);
        assign = EditorGUILayout.Popup("Atribuição", Mathf.Max(0, assign),
            new[] { "Fixa nesta máquina", "Sorteio a cada sessão", "Por participante (estável)" });
        _cfg.conditionAssignment = new[] { "fixed", "random", "by_participant" }[assign];
        if (_cfg.conditionAssignment == "fixed") _cfg.condition = EditorGUILayout.TextField("Condição desta máquina", _cfg.condition);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Câmera (expressões faciais)", EditorStyles.boldLabel);
        _cfg.affectCaptureEnabled = EditorGUILayout.Toggle("Captura facial ligada", _cfg.affectCaptureEnabled);
        using (new EditorGUI.DisabledScope(!_cfg.affectCaptureEnabled))
        {
            _cfg.affectSampleRateHz = EditorGUILayout.Slider("Amostras por segundo", _cfg.affectSampleRateHz, 0.5f, 10f);
            _cfg.showCameraIndicator = EditorGUILayout.Toggle("Mostrar aviso de câmera", _cfg.showCameraIndicator);
            _cfg.saveRawBlendshapes = EditorGUILayout.Toggle("Gravar 52 blendshapes", _cfg.saveRawBlendshapes);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Pesquisador e exportação", EditorStyles.boldLabel);
        _cfg.researcherPin = EditorGUILayout.TextField(new GUIContent("PIN do painel (Ctrl+Shift+R)"), _cfg.researcherPin);
        _cfg.exportIncludesNameMap = EditorGUILayout.Toggle("Exportar mapa de nomes", _cfg.exportIncludesNameMap);
        _cfg.idleThresholdSeconds = EditorGUILayout.FloatField("Ociosidade (segundos)", _cfg.idleThresholdSeconds);

        EditorGUILayout.Space();
        if (GUILayout.Button("Salvar configuração", GUILayout.Height(30))) Save();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Verificação", EditorStyles.boldLabel);
        ApplyConditions();
        var issues = StudyConfigValidator.Validate(_cfg, MediaPipePresent());
        if (issues.Count == 0) EditorGUILayout.HelpBox("Configuração pronta para a coleta.", MessageType.Info);
        foreach (var i in issues)
            EditorGUILayout.HelpBox(i.message, i.level == StudyConfigValidator.Level.Error ? MessageType.Error
                : i.level == StudyConfigValidator.Level.Warning ? MessageType.Warning : MessageType.Info);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Códigos de participante", EditorStyles.boldLabel);
        _codeCount = EditorGUILayout.IntSlider("Quantidade", _codeCount, 1, 500);
        if (GUILayout.Button("Gerar códigos e etiquetas para imprimir"))
        {
            var (csv, html) = StudyTools.GenerateCodeSheet(_cfg.studyId, _cfg.schoolId, _cfg.classId, _codeCount, StudyFolder);
            _message = $"Gerados: {csv}";
            EditorUtility.RevealInFinder(html);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Dados desta máquina", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Exportar coleta (.zip)"))
        {
            string zip = DataExporter.Export(Path.Combine(StudyFolder, "Exportacoes"));
            _message = "Exportado: " + zip;
            EditorUtility.RevealInFinder(zip);
        }
        if (GUILayout.Button("Abrir pasta de dados"))
        {
            Directory.CreateDirectory(TelemetryManager.DataFolder);
            EditorUtility.RevealInFinder(TelemetryManager.DataFolder);
        }
        GUILayout.EndHorizontal();

        if (!string.IsNullOrEmpty(_message)) EditorGUILayout.HelpBox(_message, MessageType.None);
        EditorGUILayout.EndScrollView();
    }

    private void ApplyConditions()
    {
        _cfg.conditions = _conditionsText.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim()).Where(s => s.Length > 0).ToArray();
    }

    private void Save()
    {
        ApplyConditions();
        string path = Path.Combine(Application.streamingAssetsPath, ResearchConfig.FileName);
        _cfg.Save(path);
        ResearchConfig.SetCurrent(null);
        AssetDatabase.Refresh();
        _message = "Configuração salva em " + path;
    }

    private static bool MediaPipePresent() =>
        AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().Name == "Mediapipe.Runtime");
}

/// <summary>Funções do Painel do Estudo que não dependem da janela (testáveis).</summary>
public static class StudyTools
{
    /// <summary>Gera códigos e salva uma planilha (CSV) e etiquetas para imprimir (HTML).</summary>
    public static (string csv, string html) GenerateCodeSheet(string studyId, string schoolId, string classId, int count, string folder, int? seed = null)
    {
        Directory.CreateDirectory(folder);
        var codes = ParticipantCodes.Generate(count, seed);
        string stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string baseName = $"codigos_{(string.IsNullOrEmpty(studyId) ? "estudo" : studyId)}_{stamp}";
        string csv = Path.Combine(folder, baseName + ".csv");
        string html = Path.Combine(folder, baseName + ".html");

        var sb = new StringBuilder("codigo;estudo;escola;turma;nome_crianca_preencher_a_mao;observacao\n");
        foreach (var c in codes) sb.Append($"{c};{studyId};{schoolId};{classId};;\n");
        File.WriteAllText(csv, sb.ToString(), new UTF8Encoding(true));

        string E(string s) => WebUtility.HtmlEncode(s ?? "");
        var h = new StringBuilder();
        h.Append("<!doctype html><html lang=\"pt-BR\"><head><meta charset=\"utf-8\"><title>Códigos de participante</title><style>");
        h.Append("body{font-family:Arial,sans-serif;margin:12mm}h1{font-size:14pt}p{font-size:9pt;color:#555}");
        h.Append(".grade{display:grid;grid-template-columns:repeat(3,1fr);gap:4mm}");
        h.Append(".etq{border:1px dashed #999;border-radius:3mm;padding:5mm;text-align:center;page-break-inside:avoid}");
        h.Append(".cod{font-size:28pt;font-weight:bold;letter-spacing:3pt;font-family:Consolas,monospace}");
        h.Append(".inf{font-size:8pt;color:#666;margin-top:2mm}@media print{p.aviso{display:none}}</style></head><body>");
        h.Append($"<h1>Fábulas Computacionais — códigos de participante</h1>");
        h.Append($"<p class=\"aviso\">Estudo {E(studyId)} · escola {E(schoolId)} · turma {E(classId)} · {codes.Count} códigos. ");
        h.Append("Imprima, recorte e entregue um código a cada criança. Anote nome ↔ código APENAS na lista em papel guardada pelo pesquisador.</p>");
        h.Append("<div class=\"grade\">");
        foreach (var c in codes)
            h.Append($"<div class=\"etq\"><div class=\"cod\">{c}</div><div class=\"inf\">Digite este código no jogo<br>{E(studyId)} · {E(classId)}</div></div>");
        h.Append("</div></body></html>");
        File.WriteAllText(html, h.ToString(), new UTF8Encoding(false));
        return (csv, html);
    }
}
