using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Autoteste de ponta a ponta do learning analytics, executado no jogo compilado.
/// Só é ativado por linha de comando (sem os argumentos, não faz nada):
///
///   FabulasComputacionais.exe -analyticsSelfTest completo|interromper|codigo_invalido
///       -selfTestOut "C:\saida" -researchConfig "C:\saida\config.json" -analyticsDataFolder "C:\saida\dados"
///
///  completo        — cadastro com código + consentimento, fase com tentativas e vitória,
///                    Painel do Pesquisador (teste de câmera e exportação), volta ao menu.
///  interromper     — inicia uma sessão e "derruba" o jogo sem encerrá-la (simula queda).
///  codigo_invalido — digita um código com erro: deve recusar e não iniciar sessão.
/// Grava selftest_&lt;modo&gt;.json com cada verificação e os erros de log encontrados.
/// </summary>
public class AnalyticsSelfTest : MonoBehaviour
{
    private string _mode, _out;
    private readonly List<(string name, bool ok, string detail)> _checks = new List<(string, bool, string)>();
    private readonly List<string> _errors = new List<string>();
    private static readonly string[] OurTypes =
        { "TelemetryManager", "AffectManager", "ResearchSessionFlow", "ResearcherPanel", "DataExporter",
          "SolutionTracker", "MirrorsTracker", "ParticipantCodes", "ResearchConfig", "AnalyticsSelfTest", "SignupManager", "DataUploader", "SyncEngine", "SignupCameraPreview" };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Init()
    {
        string mode = ResearchConfig.CommandLineArg("-analyticsSelfTest");
        if (string.IsNullOrEmpty(mode)) return;
        var go = new GameObject("[AnalyticsSelfTest]");
        DontDestroyOnLoad(go);
        var t = go.AddComponent<AnalyticsSelfTest>();
        t._mode = mode;
        t._out = ResearchConfig.CommandLineArg("-selfTestOut") ?? Path.Combine(Application.persistentDataPath, "SelfTest");
        Directory.CreateDirectory(t._out);
        Application.runInBackground = true;
    }

    private void Awake() => Application.logMessageReceived += OnLog;
    private void OnDestroy() => Application.logMessageReceived -= OnLog;

    private void OnLog(string condition, string stack, LogType type)
    {
        if (type != LogType.Exception && type != LogType.Error) return;
        string all = condition + "\n" + stack;
        if (OurTypes.Any(all.Contains)) _errors.Add(condition.Split('\n')[0] + " | " + (stack ?? "").Split('\n').FirstOrDefault());
    }

    private void Check(bool ok, string name, string detail = "")
    {
        _checks.Add((name, ok, detail));
        Debug.Log($"[AnalyticsSelfTest] {(ok ? "OK  " : "FALHA")} {name} {detail}");
    }

    private IEnumerator Start()
    {
        yield return WaitScene("MainMenu", 20f);
        yield return new WaitForSecondsRealtime(2f);
        var cfg = ResearchConfig.Current;
        Check(cfg.UsesParticipantCodes && cfg.requireConsent, "configuração de teste carregada (-researchConfig)", ResearchConfig.ConfigPath);
        Check(TelemetryManager.DataFolder.StartsWith(ResearchConfig.CommandLineArg("-analyticsDataFolder") ?? "§"), "pasta de dados de teste em uso", TelemetryManager.DataFolder);

        switch (_mode)
        {
            case "completo": yield return Completo(); break;
            case "interromper": yield return Interromper(); yield break;   // o processo morre lá dentro
            case "codigo_invalido": yield return CodigoInvalido(); break;
            case "cadastro_camera": yield return CadastroCamera(); break;
            default: Check(false, "modo conhecido", _mode); break;
        }
        Finish();
    }

    // ------------------------------------------------------------------ Cenários

    private IEnumerator Completo()
    {
        string code = ParticipantCodes.Generate(1, Environment.TickCount)[0];
        yield return Signup(code, acceptConsent: true, cameraConsent: true);
        yield return WaitScene("Area1", 20f);
        var tm = TelemetryManager.Instance;
        Check(tm != null && tm.SessionActive, "sessão iniciada após consentimento");
        Check(tm != null && tm.ParticipantId == code, "participante identificado pelo código", tm?.ParticipantId);
        Check(tm != null && (tm.Condition == "A" || tm.Condition == "B"), "condição A/B atribuída", tm?.Condition);
        Check(tm != null && tm.Condition == ResearchConfig.Current.AssignCondition(code), "condição estável por participante");
        yield return new WaitForSecondsRealtime(2f);

        // Fase: tentativas, estado da solução e vitória pelos componentes reais do jogo.
        SceneManager.LoadScene("Conjuntos Fase 1");
        yield return WaitScene("Conjuntos Fase 1", 20f);
        yield return new WaitForSecondsRealtime(3f);
        var ldm = FindObjectOfType<LevelDataManager>();
        Check(ldm != null, "LevelDataManager presente na fase");
        if (ldm != null)
        {
            long before = tm.EventCount;
            ldm.GetAttempt();
            SolutionTracker.LogAttempt("Conjuntos", new[] { "quadrado", "circulo" }, new[] { "quadrado", "triangulo" }, false);
            yield return new WaitForSecondsRealtime(1f);
            ldm.GetAttempt();
            SolutionTracker.LogAttempt("Conjuntos", new[] { "quadrado", "triangulo" }, new[] { "quadrado", "triangulo" }, true);
            ldm.WinLevel();
            GameManager.Instance.UpdateProgress(SceneManager.GetActiveScene().name);
            Check(tm.EventCount - before >= 6, "eventos de tentativa, solução e vitória gravados", $"{tm.EventCount - before} novos");
        }

        SceneManager.LoadScene("Area1");
        yield return WaitScene("Area1", 20f);
        yield return new WaitForSecondsRealtime(2f);

        // Painel do Pesquisador
        var panel = ResearcherPanel.Instance;
        Check(panel != null, "Painel do Pesquisador disponível");
        panel.RequestOpen();
        Check(panel.IsOpen, "painel abre (PIN vazio na configuração de teste)");
        var am = AffectManager.Instance;
        Check(am != null && !string.IsNullOrEmpty(am.LastError) == !am.IsCapturing,
            "câmera: coleta ativa ou falha registrada sem travar o jogo", am != null ? (am.IsCapturing ? "coletando" : am.LastError) : "");
        if (am != null && !am.IsCapturing)
        {
            panel.ToggleCameraTest();
            yield return new WaitForSecondsRealtime(3f);
            Check(am.IsPreviewing || !string.IsNullOrEmpty(am.LastError), "teste de câmera liga a prévia ou explica o problema", am.IsPreviewing ? "prévia ativa" : am.LastError);
            if (am.IsPreviewing) panel.ToggleCameraTest();
        }
        string zipDuring = panel.ExportNow();
        Check(zipDuring != null && File.Exists(zipDuring), "exportação durante a sessão", zipDuring);
        panel.Close();
        Check(!panel.IsOpen, "painel fecha");

        // Fim normal
        string sessionId = tm.SessionId;
        GameManager.Instance.BackToHome();
        yield return WaitScene("MainMenu", 20f);
        yield return new WaitForSecondsRealtime(1f);
        Check(!tm.SessionActive, "sessão encerrada ao voltar ao menu");
        Check(!File.Exists(Path.Combine(TelemetryManager.DataFolder, TelemetryManager.OpenSessionMarker)), "marcador de sessão aberta removido");

        string events = Path.Combine(TelemetryManager.DataFolder, $"events_{sessionId}.jsonl");
        string text = File.Exists(events) ? File.ReadAllText(events) : "";
        Check(text.Contains("\"event\":\"session_start\"") && text.Contains("\"reason\":\"back_to_menu\""), "arquivo da sessão completo (início e fim)");
        Check(!text.Contains(code.ToLowerInvariant()) && text.Contains(code), "código gravado normalizado");
        Check(!File.Exists(Path.Combine(TelemetryManager.DataFolder, "participants_map.csv")), "modo código não grava mapa de nomes");

        string zipFinal = DataExporter.Export(Path.Combine(_out, "exportacao_final"));
        Check(File.Exists(zipFinal), "exportação final", zipFinal);

        // Servidor central: com -serverConfig ligado, tudo (inclusive sessões antigas) deve chegar lá.
        var up = DataUploader.Instance;
        if (up != null && up.Enabled)
        {
            float limit = Time.realtimeSinceStartup + 60f;
            var asked = DateTime.UtcNow;
            up.SyncNow();
            yield return new WaitForSecondsRealtime(1f);
            while (Time.realtimeSinceStartup < limit &&
                   (up.Busy || !(up.LastSuccessUtc > asked) || up.PendingEvents + up.PendingAffect > 0 || !string.IsNullOrEmpty(up.LastError)))
            {
                yield return new WaitForSecondsRealtime(1f);
                if (!up.Busy && !string.IsNullOrEmpty(up.LastError)) up.SyncNow();
            }
            Check(string.IsNullOrEmpty(up.LastError), "envio ao servidor sem erro", up.LastError);
            Check(up.PendingEvents == 0 && up.PendingAffect == 0, "servidor recebeu todos os eventos e amostras faciais",
                $"enviados {up.TotalEventsSent} eventos e {up.TotalAffectSent} amostras; pendentes {up.PendingEvents}/{up.PendingAffect}");
            Check(File.Exists(events), "arquivos locais mantidos após o envio");
        }
    }

    /// <summary>Telinha da câmera: aparece com o cadastro aberto e some ao iniciar.</summary>
    private IEnumerator CadastroCamera()
    {
        var su = FindObjectOfType<SignupManager>(true);
        Check(su != null, "painel de cadastro encontrado");
        if (su == null) yield break;
        var preview = SignupCameraPreview.Instance;
        Check(preview != null && !preview.Visible, "telinha escondida antes de abrir o cadastro");
        su.gameObject.SetActive(true);   // o que o botão "Jogar" faz
        yield return new WaitForSecondsRealtime(4f);
        bool expected = ResearchConfig.Current.affectCaptureEnabled && AffectManager.PluginAvailable;
        Check(preview.Visible == expected, "telinha da câmera aparece com o cadastro aberto", AffectManager.Instance.IsPreviewing ? "câmera ligada" : AffectManager.Instance.LastError);
        Check(TelemetryManager.Instance != null && !TelemetryManager.Instance.SessionActive, "nada é gravado enquanto a telinha está aberta");
        yield return new WaitForEndOfFrame();
        ScreenCapture.CaptureScreenshot(Path.Combine(_out, "cadastro_com_camera.png"));
        yield return new WaitForSecondsRealtime(1f);

        string code = ParticipantCodes.Generate(1, Environment.TickCount + 7)[0];
        yield return Signup(code, acceptConsent: true, cameraConsent: true);
        Check(!preview.Visible, "telinha some quando o estudante inicia");
        yield return WaitScene("Area1", 20f);
        yield return new WaitForSecondsRealtime(2f);
        Check(!preview.Visible, "telinha continua escondida durante o jogo");
        ScreenCapture.CaptureScreenshot(Path.Combine(_out, "apos_iniciar.png"));
        yield return new WaitForSecondsRealtime(1f);
        GameManager.Instance.BackToHome();
        yield return WaitScene("MainMenu", 20f);
    }

    private IEnumerator Interromper()
    {
        string code = ParticipantCodes.Generate(1, Environment.TickCount + 99)[0];
        yield return Signup(code, acceptConsent: true, cameraConsent: false);
        yield return WaitScene("Area1", 20f);
        yield return new WaitForSecondsRealtime(2f);
        var tm = TelemetryManager.Instance;
        Check(tm != null && tm.SessionActive, "sessão iniciada");
        TelemetryManager.Log("door_approach", ("door", "Musical 1"));
        Check(File.Exists(Path.Combine(TelemetryManager.DataFolder, TelemetryManager.OpenSessionMarker)), "marcador de sessão aberta existe durante a sessão");
        File.WriteAllText(Path.Combine(_out, "sessao_interrompida.txt"), tm?.SessionId ?? "");
        WriteResult(true);
        Debug.Log("[AnalyticsSelfTest] Derrubando o processo sem encerrar a sessão...");
        System.Diagnostics.Process.GetCurrentProcess().Kill();
        yield break;
    }

    private IEnumerator CodigoInvalido()
    {
        string code = ParticipantCodes.Generate(1, 4242)[0];
        char wrong = code[2] == 'A' ? 'B' : 'A';
        string typo = code.Substring(0, 2) + wrong + code.Substring(3);
        Check(!ParticipantCodes.IsValid(typo), "código com erro de digitação é inválido", typo);
        yield return Signup(typo, acceptConsent: false, cameraConsent: false);
        yield return new WaitForSecondsRealtime(2f);
        var flow = ResearchSessionFlow.Instance;
        Check(flow != null && flow.OpenDialog != null, "mensagem de código não reconhecido exibida");
        Check(TelemetryManager.Instance != null && !TelemetryManager.Instance.SessionActive, "nenhuma sessão iniciada com código inválido");
        Check(SceneManager.GetActiveScene().name == "MainMenu", "permanece no cadastro");
        flow?.CloseDialog();
    }

    // ------------------------------------------------------------------ Ajudantes

    private IEnumerator Signup(string code, bool acceptConsent, bool cameraConsent)
    {
        var su = FindObjectOfType<SignupManager>(true);
        Check(su != null, "SignupManager encontrado no menu");
        if (su == null) yield break;
        su.playerName = code;
        su.age = 9;
        su.schoolGrade = "4º ano";
        su.SetGender(1);
        su.SavePersonalDataAndStart();
        if (!acceptConsent) yield break;

        float limit = Time.realtimeSinceStartup + 8f;
        while (ResearchSessionFlow.Instance.OpenDialog == null && Time.realtimeSinceStartup < limit) yield return null;
        var dialog = ResearchSessionFlow.Instance.OpenDialog;
        Check(dialog != null, "tela de consentimento exibida");
        if (dialog == null) yield break;
        var toggle = dialog.GetComponentInChildren<Toggle>(true);
        if (toggle != null) toggle.isOn = cameraConsent;
        var agree = dialog.GetComponentsInChildren<Button>(true).FirstOrDefault(b => b.name == "BotaoConcordo");
        Check(agree != null, "botão de concordar presente");
        agree?.onClick.Invoke();
        yield return null;
        Check(ResearchSessionFlow.Instance.OpenDialog == null, "tela de consentimento fecha ao concordar");
    }

    private IEnumerator WaitScene(string name, float timeout)
    {
        float limit = Time.realtimeSinceStartup + timeout;
        while (SceneManager.GetActiveScene().name != name && Time.realtimeSinceStartup < limit) yield return null;
        Check(SceneManager.GetActiveScene().name == name, $"cena {name} carregada");
    }

    private void Finish()
    {
        bool ok = _checks.All(c => c.ok) && _errors.Count == 0;
        WriteResult(ok);
        Debug.Log($"[AnalyticsSelfTest] RESULTADO {_mode}: {(ok ? "OK" : "FALHA")}");
        Application.Quit(ok ? 0 : 1);
    }

    private void WriteResult(bool ok)
    {
        string Q(string s) => "\"" + (s ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", " ") + "\"";
        var sb = new StringBuilder();
        sb.Append("{\n  \"mode\": ").Append(Q(_mode)).Append(",\n  \"ok\": ").Append(ok && _errors.Count == 0 ? "true" : "false").Append(",\n  \"checks\": [\n");
        for (int i = 0; i < _checks.Count; i++)
            sb.Append($"    {{\"ok\": {(_checks[i].ok ? "true" : "false")}, \"name\": {Q(_checks[i].name)}, \"detail\": {Q(_checks[i].detail)}}}")
              .Append(i < _checks.Count - 1 ? ",\n" : "\n");
        sb.Append("  ],\n  \"errors\": [").Append(string.Join(", ", _errors.Select(Q))).Append("]\n}\n");
        File.WriteAllText(Path.Combine(_out, $"selftest_{_mode}.json"), sb.ToString(), new UTF8Encoding(false));
    }
}
