using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Fabulas.LearningAnalytics.Editor
{
    /// <summary>
    /// Liga/desliga o símbolo FABULAS_MEDIAPIPE conforme o MediaPipe Unity Plugin
    /// esteja instalado e copia o modelo face_landmarker para StreamingAssets.
    ///
    /// Fica em um assembly próprio (Fabulas.LearningAnalytics.Editor.asmdef) para
    /// continuar compilando mesmo se o plugin for removido; assim o símbolo pode
    /// ser retirado e o projeto volta a compilar sozinho.
    /// </summary>
    [InitializeOnLoad]
    public static class MediaPipeSetup
    {
        private const string Define = "FABULAS_MEDIAPIPE";
        private const string RuntimeAssembly = "Mediapipe.Runtime";
        private const string ModelName = "face_landmarker_v2_with_blendshapes";
        private const string ModelDestRelative = "MediaPipe/" + ModelName + ".bytes";

        static MediaPipeSetup()
        {
            EditorApplication.delayCall += () => Sync(false);
        }

        [MenuItem("Fabulas/Learning Analytics/Configurar MediaPipe")]
        private static void SyncFromMenu() => Sync(true);

        /// <summary>
        /// Para linha de comando:
        /// Unity.exe -batchmode -quit -projectPath . -executeMethod Fabulas.LearningAnalytics.Editor.MediaPipeSetup.SyncFromCommandLine
        /// </summary>
        public static void SyncFromCommandLine() => Sync(true);

        [MenuItem("Fabulas/Learning Analytics/Abrir pasta de dados coletados")]
        private static void OpenDataFolder()
        {
            string path = Path.Combine(Application.persistentDataPath, "LearningAnalytics");
            Directory.CreateDirectory(path);
            EditorUtility.RevealInFinder(path);
        }

        private static bool PluginPresent() =>
            AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().Name == RuntimeAssembly);

        private static void Sync(bool verbose)
        {
            bool present = PluginPresent();
            bool changed = false;
            foreach (var target in new[] { NamedBuildTarget.Standalone, CurrentTarget() }.Distinct())
                changed |= SetDefine(target, present);

            if (present) CopyModelIfMissing(verbose);

            if (verbose || changed)
            {
                Debug.Log(present
                    ? $"[LearningAnalytics] MediaPipe Unity Plugin encontrado; símbolo {Define} ativo."
                    : $"[LearningAnalytics] MediaPipe Unity Plugin não instalado; captura facial desativada (símbolo {Define} removido).");
            }
        }

        private static NamedBuildTarget CurrentTarget() =>
            NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);

        private static bool SetDefine(NamedBuildTarget target, bool enabled)
        {
            var symbols = PlayerSettings.GetScriptingDefineSymbols(target)
                .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .ToList();
            bool has = symbols.Contains(Define);
            if (has == enabled) return false;

            if (enabled) symbols.Add(Define);
            else symbols.Remove(Define);
            PlayerSettings.SetScriptingDefineSymbols(target, string.Join(";", symbols));
            return true;
        }

        private static void CopyModelIfMissing(bool verbose)
        {
            string dest = Path.Combine(Application.streamingAssetsPath, ModelDestRelative);
            if (File.Exists(dest))
            {
                if (verbose) Debug.Log($"[LearningAnalytics] Modelo já presente em {dest}");
                return;
            }

            // Procura o modelo dentro do pacote (tarball ou unitypackage).
            string source = AssetDatabase.FindAssets(ModelName)
                .Select(AssetDatabase.GUIDToAssetPath)
                .FirstOrDefault(p => p.EndsWith(ModelName + ".bytes", StringComparison.OrdinalIgnoreCase));

            if (source == null)
            {
                Debug.LogWarning(
                    "[LearningAnalytics] Modelo " + ModelName + ".bytes não encontrado no pacote. " +
                    "Baixe 'face_landmarker.task' (https://storage.googleapis.com/mediapipe-models/face_landmarker/face_landmarker/float16/1/face_landmarker.task), " +
                    "renomeie para " + ModelName + ".bytes e coloque em Assets/StreamingAssets/MediaPipe/.");
                return;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(dest));
            File.Copy(Path.GetFullPath(source), dest);
            AssetDatabase.Refresh();
            Debug.Log($"[LearningAnalytics] Modelo copiado de {source} para {dest}");
        }
    }
}
