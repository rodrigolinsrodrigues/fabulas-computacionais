using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Renderiza a câmera principal de uma cena para PNG, direto no editor (sem rodar o jogo).
///   Unity.exe -batchmode -quit -projectPath . -executeMethod SceneCameraCapture.Run
///             -captureScene "Assets/Scenes/Receitas/Receitas Fase 1.unity" -captureOut "C:\saida.png"
/// Interfaces em modo Screen Space Overlay não aparecem nesta captura.
/// </summary>
public static class SceneCameraCapture
{
    public static void Run()
    {
        var args = Environment.GetCommandLineArgs();
        string Arg(string name) { int i = Array.IndexOf(args, name); return i >= 0 && i + 1 < args.Length ? args[i + 1] : null; }

        string scenePath = Arg("-captureScene");
        string outPath = Arg("-captureOut");
        int width = int.TryParse(Arg("-captureWidth"), out var w) ? w : 1600;
        int height = int.TryParse(Arg("-captureHeight"), out var h) ? h : 900;

        try
        {
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            var cam = Camera.main != null ? Camera.main : UnityEngine.Object.FindObjectOfType<Camera>();
            if (cam == null) throw new Exception("Nenhuma câmera na cena.");

            var rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32) { antiAliasing = 4 };
            cam.targetTexture = rt;
            cam.aspect = (float)width / height;
            cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();
            cam.targetTexture = null;
            RenderTexture.active = null;

            Directory.CreateDirectory(Path.GetDirectoryName(outPath));
            File.WriteAllBytes(outPath, tex.EncodeToPNG());
            Debug.Log($"[SceneCameraCapture] OK {outPath} (câmera: {cam.name})");
            EditorApplication.Exit(0);
        }
        catch (Exception e)
        {
            Debug.LogError("[SceneCameraCapture] FALHOU: " + e);
            EditorApplication.Exit(1);
        }
    }
}
