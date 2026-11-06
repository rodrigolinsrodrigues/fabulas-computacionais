using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Teste rápido do log de eventos e do "estado da solução" (sem jogar).
/// Cria uma sessão de teste, registra duas tentativas simuladas, mostra as linhas
/// geradas e apaga os arquivos de teste no final.
///   Unity.exe -batchmode -quit -projectPath . -executeMethod TelemetrySmokeTest.Run
/// </summary>
public static class TelemetrySmokeTest
{
    private const string TestName = "__TESTE_AUTOMATICO__";

    [MenuItem("Fabulas/Learning Analytics/Teste rápido da telemetria")]
    public static void Run()
    {
        bool ok = false;
        var go = new GameObject("[TelemetrySmokeTest]");
        string eventsPath = null;
        try
        {
            var tm = go.AddComponent<TelemetryManager>();
            typeof(TelemetryManager).GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)
                .SetValue(null, tm);

            tm.StartSession(TestName, 9, "outro", "4º ano");
            eventsPath = Path.Combine(TelemetryManager.DataFolder, $"events_{tm.SessionId}.jsonl");

            SolutionTracker.LogAttempt("Musical",
                new[] { "Do", "Mi", "Sol", "La" }, new[] { "Do", "Mi", "Sol", "Fa" }, false,
                ("repeat", new[] { 0, 2 }));
            SolutionTracker.LogAttempt("Musical",
                new[] { "Do", "Mi", "Sol", "Fa" }, new[] { "Do", "Mi", "Sol", "Fa" }, true);
            tm.EndSession("smoke_test");

            var lines = File.ReadAllLines(eventsPath);
            foreach (var l in lines) Debug.Log("[TelemetrySmokeTest] " + l);

            var snaps = lines.Where(l => l.Contains("\"event\":\"solution_snapshot\"")).ToArray();
            ok = snaps.Length == 2
                 && snaps[0].Contains("\"slots\":[\"Do\",\"Mi\",\"Sol\",\"La\"]")
                 && snaps[0].Contains("\"correct_positions\":3")
                 && snaps[0].Contains("\"first_error\":3")
                 && snaps[0].Contains("\"changed_positions\":null")
                 && snaps[0].Contains("\"repeat\":[0,2]")
                 && snaps[1].Contains("\"changed_positions\":[3]")
                 && snaps[1].Contains("\"distance\":0")
                 && snaps[1].Contains("\"success\":true")
                 && !lines.Any(l => l.Contains(TestName));   // nome nunca vai para o log
        }
        catch (Exception e)
        {
            Debug.LogError("[TelemetrySmokeTest] FALHOU: " + e);
        }
        finally
        {
            Cleanup(eventsPath);
            UnityEngine.Object.DestroyImmediate(go);
        }

        Debug.Log(ok ? "[TelemetrySmokeTest] RESULTADO: OK" : "[TelemetrySmokeTest] RESULTADO: FALHA");
        if (Application.isBatchMode) EditorApplication.Exit(ok ? 0 : 1);
    }

    private static void Cleanup(string eventsPath)
    {
        try
        {
            if (eventsPath != null && File.Exists(eventsPath)) File.Delete(eventsPath);
            string map = Path.Combine(TelemetryManager.DataFolder, "participants_map.csv");
            if (File.Exists(map))
            {
                var kept = File.ReadAllLines(map).Where(l => !l.Contains(TestName)).ToArray();
                if (kept.Length <= 1) File.Delete(map);          // só sobrou o cabeçalho
                else File.WriteAllLines(map, kept);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("[TelemetrySmokeTest] Limpeza: " + e.Message);
        }
    }
}
