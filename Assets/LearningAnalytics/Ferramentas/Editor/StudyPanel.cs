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
    private ServerSyncConfig _server;
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
        _server = ServerSyncConfig.LoadFrom(Path.Combine(Application.streamingAssetsPath, ServerSyncConfig.FileName));
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
            _cfg.showCameraPreviewOnSignup = EditorGUILayout.Toggle(new GUIContent("Prévia da câmera no cadastro", "Telinha para o pesquisador conferir a posição da câmera; some quando o estudante inicia"), _cfg.showCameraPreviewOnSignup);
            _cfg.saveRawBlendshapes = EditorGUILayout.Toggle("Gravar 52 blendshapes", _cfg.saveRawBlendshapes);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Pesquisador e exportação", EditorStyles.boldLabel);
        _cfg.researcherPin = EditorGUILayout.TextField(new GUIContent("PIN do painel (Ctrl+Shift+R)"), _cfg.researcherPin);
        _cfg.exportIncludesNameMap = EditorGUILayout.Toggle("Exportar mapa de nomes", _cfg.exportIncludesNameMap);
        _cfg.idleThresholdSeconds = EditorGUILayout.FloatField("Ociosidade (segundos)", _cfg.idleThresholdSeconds);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Servidor central (Supabase)", EditorStyles.boldLabel);
        _server.enabled = EditorGUILayout.Toggle(new GUIContent("Enviar ao servidor", "Os dados continuam salvos no computador; o envio é uma cópia."), _server.enabled);
        using (new EditorGUI.DisabledScope(!_server.enabled))
        {
            _server.serverUrl = EditorGUILayout.TextField(new GUIContent("URL do projeto", "https://<projeto>.supabase.co"), _server.serverUrl);
            _server.anonKey = EditorGUILayout.TextField(new GUIContent("Chave pública (anon)", "Settings > API Keys: anon / publishable. NUNCA a service_role/secret."), _server.anonKey);
            _server.uploadKey = EditorGUILayout.PasswordField(new GUIContent("Chave de envio do estudo", "Gerada abaixo; só permite enviar dados deste estudo."), _server.uploadKey);
            _server.syncIntervalSeconds = EditorGUILayout.FloatField("Intervalo de envio (s)", _server.syncIntervalSeconds);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Gerar chaves do estudo"))
            {
                if (string.IsNullOrWhiteSpace(_cfg.studyId)) _message = "Informe o studyId antes de gerar as chaves.";
                else if (string.IsNullOrEmpty(_server.uploadKey) || EditorUtility.DisplayDialog("Gerar novas chaves",
                         "Já existe uma chave de envio. Gerar novas exige cadastrar o estudo de novo no Supabase. Continuar?", "Gerar", "Cancelar"))
                {
                    var keys = StudyTools.GenerateServerKeys(_cfg.studyId, StudyFolder);
                    _server.uploadKey = keys.uploadKey;
                    _message = "Chaves geradas e guardadas em " + keys.file +
                               ". Abra o arquivo, copie o comando SQL e rode no SQL Editor do Supabase. Depois clique em Salvar configuração.";
                    EditorUtility.RevealInFinder(keys.file);
                }
            }
            if (GUILayout.Button("Testar conexão"))
            {
                var engine = new SyncEngine(TelemetryManager.DataFolder, _server.EffectiveStudyIdFor(_cfg), _server.uploadKey,
                    (rpc, body) => DataUploader.Post(_server.serverUrl, _server.anonKey, rpc, body, 15000));
                _message = engine.TestConnection(out bool ok);
            }
            GUILayout.EndHorizontal();
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Salvar configuração", GUILayout.Height(30))) Save();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Verificação", EditorStyles.boldLabel);
        ApplyConditions();
        var issues = StudyConfigValidator.Validate(_cfg, MediaPipePresent());
        issues.AddRange(StudyConfigValidator.ValidateServer(_server, _cfg));
        if (!issues.Any(i => i.level != StudyConfigValidator.Level.Info)) EditorGUILayout.HelpBox("Configuração pronta para a coleta.", MessageType.Info);
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
        _server.serverUrl = (_server.serverUrl ?? "").Trim().TrimEnd('/');
        _server.Save(Path.Combine(Application.streamingAssetsPath, ServerSyncConfig.FileName));
        ServerSyncConfig.SetCurrent(null);
        AssetDatabase.Refresh();
        _message = "Configuração salva em " + path;
    }

    private static bool MediaPipePresent() =>
        AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().Name == "Mediapipe.Runtime");
}

/// <summary>Funções do Painel do Estudo que não dependem da janela (testáveis).</summary>
public static class StudyTools
{
    /// <summary>
    /// Gera a chave de envio (vai para o jogo) e a chave de leitura (só para a análise) do estudo
    /// e grava em Pesquisa/servidor_&lt;estudo&gt;.txt, junto com o comando SQL que cadastra o estudo.
    /// As chaves não são mostradas na tela nem enviadas a lugar nenhum.
    /// </summary>
    public static (string uploadKey, string readKey, string file) GenerateServerKeys(string studyId, string folder)
    {
        string upload = "env_" + RandomKey(32);
        string read = "lei_" + RandomKey(32);
        Directory.CreateDirectory(folder);
        string file = Path.Combine(folder, $"servidor_{studyId}.txt");
        string sqlStudy = studyId.Replace("'", "''");
        var lines = new[]
        {
            $"Fábulas Computacionais — chaves do estudo {studyId} (gerado em {DateTime.Now:yyyy-MM-dd HH:mm})",
            "A chave de ENVIO pode ser pública (só grava). A de LEITURA é secreta: guarde este arquivo em local seguro.",
            "",
            "1) Cadastre o estudo no Supabase: SQL Editor > New query > cole e rode:",
            "",
            $"   select public.fc_create_study('{sqlStudy}', '{upload}', '{read}');",
            "",
            "2) Chave de ENVIO (já foi colocada no jogo, em StreamingAssets/server_sync.json):",
            $"   {upload}",
            "",
            "3) Chave de LEITURA (para baixar os dados no R; não vai para o jogo):",
            $"   {read}",
            "",
            "   No R:  Sys.setenv(FABULAS_CHAVE_LEITURA = \"" + read + "\")",
            "          fabulasLA::ler_coleta_servidor(url, chave_publica, \"" + studyId + "\")",
        };
        File.WriteAllText(file, string.Join("\n", lines) + "\n", new UTF8Encoding(false));
        return (upload, read, file);
    }

    private static string RandomKey(int length)
    {
        const string alphabet = "abcdefghijkmnpqrstuvwxyz23456789";
        var bytes = new byte[length];
        using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create()) rng.GetBytes(bytes);
        var sb = new StringBuilder(length);
        foreach (var b in bytes) sb.Append(alphabet[b % alphabet.Length]);   // 32 símbolos: sem viés
        return sb.ToString();
    }

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
