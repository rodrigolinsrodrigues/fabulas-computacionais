using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Ferramenta de documentação: percorre as cenas da build e salva uma captura de tela de cada uma.
/// Só é ativada quando o jogo é aberto com o argumento -screenshotTour (sem ele, não faz nada).
///
///   FabulasComputacionais.exe -screen-fullscreen 0 -screen-width 1600 -screen-height 900 -screenshotTour "C:\saida"
///
/// Argumentos opcionais: -screenshotWait <segundos> (padrão 7) e -screenshotScenes "Cena A;Cena B".
/// </summary>
public class ScreenshotTour : MonoBehaviour
{
    private static readonly string[] DefaultScenes =
    {
        "MainMenu", "Area1", "Area2", "Area3",
        "Musical 1", "Musical 2", "Espelhos Fase 1", "Espelhos Fase 2",
        "Conjuntos Fase 1", "Conjuntos Fase 2",
    };

    private string _outDir;
    private float _wait = 7f;
    private string[] _scenes = DefaultScenes;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Init()
    {
        var args = Environment.GetCommandLineArgs();
        int i = Array.IndexOf(args, "-screenshotTour");
        if (i < 0) return;

        var go = new GameObject("[ScreenshotTour]");
        DontDestroyOnLoad(go);
        var tour = go.AddComponent<ScreenshotTour>();
        tour._outDir = i + 1 < args.Length && !args[i + 1].StartsWith("-")
            ? args[i + 1]
            : Path.Combine(Application.persistentDataPath, "Screenshots");

        int w = Array.IndexOf(args, "-screenshotWait");
        if (w >= 0 && w + 1 < args.Length && float.TryParse(args[w + 1], System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var secs)) tour._wait = secs;

        int s = Array.IndexOf(args, "-screenshotScenes");
        if (s >= 0 && s + 1 < args.Length) tour._scenes = args[s + 1].Split(';');

        Application.runInBackground = true;
    }

    private IEnumerator Start()
    {
        Directory.CreateDirectory(_outDir);
        Debug.Log($"[ScreenshotTour] Salvando em {_outDir}");

        for (int n = 0; n < _scenes.Length; n++)
        {
            // "Cena>musical" abre a partitura do puzzle Musical antes da captura.
            var parts = _scenes[n].Split('>');
            string scene = parts[0].Trim();
            string action = parts.Length > 1 ? parts[1].Trim() : "";
            if (SceneManager.GetActiveScene().name != scene)
            {
                if (!Application.CanStreamedLevelBeLoaded(scene))
                {
                    Debug.LogWarning($"[ScreenshotTour] Cena '{scene}' não está na build; pulando.");
                    continue;
                }
                Time.timeScale = 1f;
                SceneManager.LoadScene(scene);
            }

            yield return new WaitForSecondsRealtime(_wait);

            if (action == "musical")
            {
                var starter = FindObjectOfType<StartMusicalGame>(true);
                if (starter != null)
                {
                    starter.Open();
                    yield return new WaitForSecondsRealtime(2f);
                }
            }
            yield return new WaitForEndOfFrame();

            string suffix = action.Length > 0 ? "_" + action : "";
            string file = Path.Combine(_outDir, $"{n + 1:00}_{scene.Replace(' ', '_')}{suffix}.png");
            ScreenCapture.CaptureScreenshot(file);
            Debug.Log($"[ScreenshotTour] {file}");
            yield return null;
            yield return null;
        }

        yield return new WaitForSecondsRealtime(1f);
        Application.Quit();
    }
}
