#if FABULAS_MEDIAPIPE
using System;
using System.IO;
using Mediapipe.Tasks.Vision.FaceLandmarker;
using UnityEditor;
using UnityEngine;
using MPBaseOptions = Mediapipe.Tasks.Core.BaseOptions;
using MPRunningMode = Mediapipe.Tasks.Vision.Core.RunningMode;
using MPImageProcessingOptions = Mediapipe.Tasks.Vision.Core.ImageProcessingOptions;
using MPFaceLandmarker = Mediapipe.Tasks.Vision.FaceLandmarker.FaceLandmarker;

/// <summary>
/// Teste rápido (sem webcam): carrega a biblioteca nativa e o modelo e processa
/// imagens em branco no mesmo modo (VIDEO) que o AffectManager usa.
/// Linha de comando:
///   Unity.exe -batchmode -quit -projectPath . -executeMethod MediaPipeSmokeTest.Run
/// </summary>
public static class MediaPipeSmokeTest
{
    [MenuItem("Fabulas/Learning Analytics/Teste rápido do MediaPipe (sem câmera)")]
    public static void Run()
    {
        bool ok = false;
        MPFaceLandmarker landmarker = null;
        try
        {
            var cfg = ResearchConfig.Load();
            string modelPath = Path.Combine(Application.streamingAssetsPath, cfg.modelFileName);
            Debug.Log($"[SmokeTest] Modelo: {modelPath} (existe: {File.Exists(modelPath)})");

            Mediapipe.Protobuf.SetLogHandler(Mediapipe.Protobuf.DefaultLogHandler);
            var options = new FaceLandmarkerOptions(
                new MPBaseOptions(MPBaseOptions.Delegate.CPU, modelAssetBuffer: File.ReadAllBytes(modelPath)),
                runningMode: MPRunningMode.VIDEO,
                numFaces: 1,
                outputFaceBlendshapes: true,
                outputFaceTransformationMatrixes: true);
            landmarker = MPFaceLandmarker.CreateFromOptions(options);
            Debug.Log("[SmokeTest] FaceLandmarker criado (biblioteca nativa e modelo OK).");

            var result = FaceLandmarkerResult.Alloc(1, true, true);
            var tex = new Texture2D(640, 480, TextureFormat.RGBA32, false);
            var pixels = new Color32[640 * 480];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color32(128, 128, 128, 255);
            tex.SetPixels32(pixels);

            var sw = System.Diagnostics.Stopwatch.StartNew();
            int detections = 0;
            const int frames = 10;
            for (int f = 0; f < frames; f++)
            {
                var image = new Mediapipe.Image(Mediapipe.ImageFormat.Types.Format.Srgba, tex);
                if (landmarker.TryDetectForVideo(image, 1000 + f * 500, new MPImageProcessingOptions(rotationDegrees: 0), ref result))
                    detections++;
            }
            sw.Stop();
            UnityEngine.Object.DestroyImmediate(tex);

            Debug.Log($"[SmokeTest] {frames} quadros processados em {sw.ElapsedMilliseconds} ms " +
                      $"({sw.ElapsedMilliseconds / (float)frames:0.0} ms/quadro); rostos detectados: {detections} (esperado 0 em imagem cinza).");

            var cams = WebCamTexture.devices;
            Debug.Log($"[SmokeTest] Webcams encontradas: {cams.Length}" +
                      (cams.Length > 0 ? " → " + string.Join(", ", Array.ConvertAll(cams, c => c.name)) : ""));
            ok = true;
        }
        catch (Exception e)
        {
            Debug.LogError("[SmokeTest] FALHOU: " + e);
        }
        finally
        {
            try { landmarker?.Close(); } catch (Exception e) { Debug.LogWarning("[SmokeTest] Close: " + e.Message); }
            Mediapipe.Protobuf.ResetLogHandler();
        }

        Debug.Log(ok ? "[SmokeTest] RESULTADO: OK" : "[SmokeTest] RESULTADO: FALHA");
        if (Application.isBatchMode) EditorApplication.Exit(ok ? 0 : 1);
    }
}
#endif
