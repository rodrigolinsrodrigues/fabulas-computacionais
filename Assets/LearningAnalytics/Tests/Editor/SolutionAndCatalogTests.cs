using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;

public class SolutionTrackerTests : AnalyticsTestBase
{
    [Test]
    public void EstadoDaSolucao_CalculaAcertos_PrimeiroErro_EPosicoesAlteradas()
    {
        var tm = CreateTelemetry();
        tm.StartSession("LIA", 9, "f", "4");
        SolutionTracker.LogAttempt("Musical", new[] { "Do", "Mi", "Sol", "La" }, new[] { "Do", "Mi", "Sol", "Fa" }, false, ("extra", 1));
        SolutionTracker.LogAttempt("Musical", new[] { "Do", "Re", "Sol", "Fa" }, new[] { "Do", "Mi", "Sol", "Fa" }, false);
        SolutionTracker.LogAttempt("Musical", new[] { "Do", "Mi", "Sol", "Fa" }, new[] { "Do", "Mi", "Sol", "Fa" }, true);
        tm.EndSession("teste");

        var snaps = ReadEvents().Where(e => (string)e["event"] == "solution_snapshot").Select(Data).ToList();
        Assert.AreEqual(3, snaps.Count);

        Assert.AreEqual(3.0, snaps[0]["correct_positions"]);
        Assert.AreEqual(3.0, snaps[0]["first_error"]);
        Assert.AreEqual(1.0, snaps[0]["distance"]);
        Assert.IsNull(snaps[0]["changed_positions"]);
        Assert.AreEqual(-1.0, snaps[0]["n_changed"]);
        Assert.AreEqual(1.0, snaps[0]["extra"]);

        CollectionAssert.AreEqual(new object[] { 1.0, 3.0 }, (List<object>)snaps[1]["changed_positions"]);
        Assert.AreEqual(1.0, snaps[1]["first_error"]);

        CollectionAssert.AreEqual(new object[] { 1.0 }, (List<object>)snaps[2]["changed_positions"]);
        Assert.AreEqual(0.0, snaps[2]["distance"]);
        Assert.AreEqual(-1.0, snaps[2]["first_error"]);
        Assert.AreEqual(true, snaps[2]["success"]);
    }

    [Test]
    public void SemGabarito_NaoGravaCamposDeComparacao()
    {
        var tm = CreateTelemetry();
        tm.StartSession("LIA", 9, "f", "4");
        SolutionTracker.LogAttempt("Espelhos", new[] { "r=0", "r=90" }, null, null);
        tm.EndSession("teste");
        var snap = ReadEvents().Where(e => (string)e["event"] == "solution_snapshot").Select(Data).Single();
        Assert.IsFalse(snap.ContainsKey("expected"));
        Assert.IsFalse(snap.ContainsKey("distance"));
        Assert.IsNull(snap["success"]);
    }
}

public class FaceAffectTests
{
    [Test]
    public void IndicesDerivados_SaoMediasDosBlendshapes()
    {
        var s = new FaceAffectSample();
        for (int i = 0; i < s.blendshapes.Length; i++) s.blendshapes[i] = 0f;
        void Set(string n, float v) => s.blendshapes[System.Array.IndexOf(FaceAffectSample.BlendshapeNames, n)] = v;
        Set("mouthSmileLeft", 0.8f); Set("mouthSmileRight", 0.6f);
        Set("browDownLeft", 0.2f); Set("browDownRight", 0.4f);
        Set("jawOpen", 0.5f);
        s.ComputeDerived();
        Assert.AreEqual(0.7f, s.derived[0], 1e-5f, "smile");
        Assert.AreEqual(0.3f, s.derived[2], 1e-5f, "brow_down");
        Assert.AreEqual(0.5f, s.derived[8], 1e-5f, "jaw_open");
        Assert.AreEqual(52, FaceAffectSample.BlendshapeNames.Length);
        Assert.AreEqual(FaceAffectSample.DerivedNames.Length, s.derived.Length);
    }

    [Test]
    public void SemRosto_TudoVazio()
    {
        var s = new FaceAffectSample();
        s.ClearFace();
        Assert.IsFalse(s.facePresent);
        Assert.IsTrue(s.blendshapes.All(float.IsNaN));
        Assert.IsTrue(float.IsNaN(s.headYaw));
    }
}

public class CatalogTests
{
    private static string AssetsPath => Application.dataPath;
    private static string CatalogPath => Path.Combine(AssetsPath, "LearningAnalytics", "catalogo_eventos.json");

    private static Dictionary<string, object> Catalog() => MiniJson.Obj(File.ReadAllText(CatalogPath));

    private static HashSet<string> CatalogEvents() =>
        new HashSet<string>(((List<object>)Catalog()["events"]).Cast<Dictionary<string, object>>().Select(e => (string)e["name"]));

    /// <summary>Nomes de eventos usados no código: Log("x") e Log(cond ? "x" : "y").</summary>
    private static HashSet<string> EventsUsedInCode()
    {
        var used = new HashSet<string>();
        var direct = new Regex("Log\\(\\s*\"([a-z_]+)\"");
        var ternary = new Regex("Log\\([^;\"]*\\?\\s*\"([a-z_]+)\"\\s*:\\s*\"([a-z_]+)\"");
        foreach (var file in Directory.GetFiles(AssetsPath, "*.cs", SearchOption.AllDirectories))
        {
            if (file.Contains("Tests") || file.Contains("Fases Legacy") || file.Contains("LootLocker")) continue;
            string src = File.ReadAllText(file);
            foreach (Match m in direct.Matches(src)) used.Add(m.Groups[1].Value);
            foreach (Match m in ternary.Matches(src)) { used.Add(m.Groups[1].Value); used.Add(m.Groups[2].Value); }
        }
        used.Add("solution_snapshot");   // registrado pelo SolutionTracker
        used.Add("solution_final");      // registrado pelo MirrorsTracker
        return used;
    }

    [Test]
    public void Catalogo_EhJsonValido_ComNomesUnicos()
    {
        var events = ((List<object>)Catalog()["events"]).Cast<Dictionary<string, object>>().ToList();
        Assert.Greater(events.Count, 30);
        Assert.AreEqual(events.Count, events.Select(e => e["name"]).Distinct().Count());
        foreach (var e in events)
        {
            Assert.IsNotEmpty((string)e["category"], (string)e["name"]);
            Assert.IsNotEmpty((string)e["when"], (string)e["name"]);
        }
    }

    [Test]
    public void TodoEventoUsadoNoCodigo_EstaNoCatalogo()
    {
        var missing = EventsUsedInCode().Except(CatalogEvents()).ToList();
        Assert.IsEmpty(missing, "Eventos sem documentação no catálogo: " + string.Join(", ", missing));
    }

    [Test]
    public void TodoEventoDoCatalogo_EhUsadoNoCodigo()
    {
        var unused = CatalogEvents().Except(EventsUsedInCode()).ToList();
        Assert.IsEmpty(unused, "Eventos no catálogo que o jogo não registra: " + string.Join(", ", unused));
    }

    [Test]
    public void ColunasFaciais_DoCatalogo_BatemComOCodigo()
    {
        var cols = ((List<object>)Catalog()["affect_columns"]).Cast<Dictionary<string, object>>().Select(c => (string)c["name"]).ToList();
        foreach (var d in FaceAffectSample.DerivedNames) Assert.Contains(d, cols);
    }

    [Test]
    public void CopiaDoCatalogoNoPacoteR_EstaSincronizada()
    {
        string rCopy = Path.GetFullPath(Path.Combine(AssetsPath, "..", "analysis", "fabulasLA", "inst", "extdata", "catalogo_eventos.json"));
        if (!File.Exists(rCopy)) Assert.Inconclusive("Pacote R não encontrado em analysis/fabulasLA");
        Assert.AreEqual(File.ReadAllText(CatalogPath), File.ReadAllText(rCopy),
            "Copie Assets/LearningAnalytics/catalogo_eventos.json para analysis/fabulasLA/inst/extdata/");
    }
}
