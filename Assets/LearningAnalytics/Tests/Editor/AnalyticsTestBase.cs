using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Base dos testes: pasta de dados temporária, configuração controlada e uma instância do
/// TelemetryManager criada sem tocar nos dados reais do pesquisador.
/// </summary>
public abstract class AnalyticsTestBase
{
    protected string Folder;
    protected GameObject Host;
    protected TelemetryManager Tm;

    [SetUp]
    public void BaseSetUp()
    {
        Folder = Path.Combine(Path.GetTempPath(), "fabulas_testes_" + System.Guid.NewGuid().ToString("N").Substring(0, 8));
        Directory.CreateDirectory(Folder);
        TelemetryManager.DataFolderOverride = Folder;
        ResearchConfig.SetCurrent(new ResearchConfig { studyId = "EST1", schoolId = "ESC1", classId = "T4A", machineId = "PC-TESTE" });
    }

    [TearDown]
    public void BaseTearDown()
    {
        if (Tm != null && Tm.SessionActive) Tm.EndSession("teste");
        if (Host != null) Object.DestroyImmediate(Host);
        SetInstance(null);
        TelemetryManager.DataFolderOverride = null;
        ResearchConfig.SetCurrent(null);
        try { Directory.Delete(Folder, true); } catch { /* arquivos podem estar em uso no Windows */ }
    }

    /// <summary>Cria o TelemetryManager (no editor o Awake não roda, então registramos a instância à mão).</summary>
    protected TelemetryManager CreateTelemetry()
    {
        Host = new GameObject("[TesteTelemetria]");
        Tm = Host.AddComponent<TelemetryManager>();
        SetInstance(Tm);
        return Tm;
    }

    protected static void SetInstance(TelemetryManager tm) =>
        typeof(TelemetryManager).GetProperty("Instance", BindingFlags.Public | BindingFlags.Static).SetValue(null, tm);

    protected List<Dictionary<string, object>> ReadEvents(string file = null)
    {
        file ??= Directory.GetFiles(Folder, "events_*.jsonl").Single();
        return File.ReadAllLines(file).Where(l => l.Trim().Length > 0).Select(MiniJson.Obj).ToList();
    }

    protected static Dictionary<string, object> Data(Dictionary<string, object> ev) => (Dictionary<string, object>)ev["data"];
}
