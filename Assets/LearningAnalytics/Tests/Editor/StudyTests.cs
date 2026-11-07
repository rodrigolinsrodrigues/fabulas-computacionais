using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using NUnit.Framework;

public class ParticipantCodeTests
{
    [Test]
    public void CodigosGerados_SaoValidos_Unicos_ESemCaracteresAmbiguos()
    {
        var codes = ParticipantCodes.Generate(2000, 123);
        Assert.AreEqual(2000, codes.Distinct().Count());
        foreach (var c in codes)
        {
            Assert.AreEqual(ParticipantCodes.CodeLength, c.Length);
            Assert.IsTrue(ParticipantCodes.IsValid(c), c);
            Assert.IsFalse(c.IndexOfAny("01OIL".ToCharArray()) >= 0, c);
        }
    }

    [Test]
    public void QualquerErroDeUmCaractere_EhDetectado()
    {
        foreach (var code in ParticipantCodes.Generate(50, 9))
            for (int pos = 0; pos < code.Length; pos++)
                foreach (char ch in ParticipantCodes.Alphabet)
                {
                    if (ch == code[pos]) continue;
                    string typo = code.Substring(0, pos) + ch + code.Substring(pos + 1);
                    Assert.IsFalse(ParticipantCodes.IsValid(typo), $"{code} -> {typo} deveria ser inválido");
                }
    }

    [Test]
    public void TrocaDeCaracteresVizinhos_EhDetectada()
    {
        foreach (var code in ParticipantCodes.Generate(300, 5))
            for (int pos = 0; pos < code.Length - 1; pos++)
            {
                if (code[pos] == code[pos + 1]) continue;
                var arr = code.ToCharArray();
                (arr[pos], arr[pos + 1]) = (arr[pos + 1], arr[pos]);
                Assert.IsFalse(ParticipantCodes.IsValid(new string(arr)), $"{code} -> {new string(arr)}");
            }
    }

    [Test]
    public void Normalizacao_AceitaMinusculas_EspacosEHifen()
    {
        string code = ParticipantCodes.Generate(1, 1)[0];
        Assert.IsTrue(ParticipantCodes.IsValid(code.ToLowerInvariant()));
        Assert.IsTrue(ParticipantCodes.IsValid(" " + code.Substring(0, 2) + "-" + code.Substring(2) + " "));
        Assert.IsFalse(ParticipantCodes.IsValid(""));
        Assert.IsFalse(ParticipantCodes.IsValid(null));
        Assert.IsFalse(ParticipantCodes.IsValid(code + "A"));
        Assert.IsFalse(ParticipantCodes.IsValid("MARIA"));
    }
}

public class ConfigTests
{
    [Test]
    public void LeituraDoJson_PreencheCampos_EUsaPadroesNosAusentes()
    {
        string path = Path.Combine(Path.GetTempPath(), "cfg_teste.json");
        File.WriteAllText(path, "{\"studyId\":\"E1\",\"participantIdMode\":\"CODE\",\"conditions\":[\"A\",\"B\"],\"conditionAssignment\":\"by_participant\",\"requireConsent\":true}");
        var c = ResearchConfig.LoadFrom(path);
        Assert.AreEqual("E1", c.studyId);
        Assert.IsTrue(c.UsesParticipantCodes, "modo deve ser normalizado para minúsculas");
        Assert.IsTrue(c.requireConsent);
        Assert.AreEqual(2, c.conditions.Length);
        Assert.IsTrue(c.telemetryEnabled, "campo ausente usa o padrão");
        Assert.IsFalse(c.affectCaptureEnabled);
        File.Delete(path);
    }

    [Test]
    public void ArquivoInexistente_UsaPadroesSeguros()
    {
        var c = ResearchConfig.LoadFrom(Path.Combine(Path.GetTempPath(), "nao_existe_123.json"));
        Assert.IsFalse(c.affectCaptureEnabled);
        Assert.IsFalse(c.UsesParticipantCodes);
    }

    [Test]
    public void SalvarELer_PreservaOsValores()
    {
        string path = Path.Combine(Path.GetTempPath(), "cfg_roundtrip.json");
        var c = new ResearchConfig { studyId = "X", schoolId = "Y", conditions = new[] { "A", "B" }, affectSampleRateHz = 3.5f };
        c.Save(path);
        var r = ResearchConfig.LoadFrom(path);
        Assert.AreEqual("Y", r.schoolId);
        Assert.AreEqual(3.5f, r.affectSampleRateHz);
        CollectionAssert.AreEqual(new[] { "A", "B" }, r.conditions);
        File.Delete(path);
    }

    [Test]
    public void CondicaoPorParticipante_EhEstavel_EEquilibrada()
    {
        var c = new ResearchConfig { conditions = new[] { "A", "B" }, conditionAssignment = "by_participant" };
        var codes = ParticipantCodes.Generate(1000, 3);
        foreach (var code in codes.Take(50)) Assert.AreEqual(c.AssignCondition(code), c.AssignCondition(code));
        int a = codes.Count(code => c.AssignCondition(code) == "A");
        Assert.That(a, Is.InRange(420, 580), $"A recebeu {a} de 1000");
    }

    [Test]
    public void CondicaoFixa_ESemCondicoes()
    {
        Assert.AreEqual("B", new ResearchConfig { conditions = new[] { "A", "B" }, condition = "B" }.AssignCondition("X"));
        Assert.AreEqual("", new ResearchConfig().AssignCondition("X"));
        Assert.AreEqual("A", new ResearchConfig { conditions = new[] { "A", "B" } }.AssignCondition("X"));
    }

    [Test]
    public void HashEstavel_TemValorConhecido()
    {
        // FNV-1a de "a" = 0xE40C292C: garante o mesmo resultado em qualquer máquina e no pacote R.
        Assert.AreEqual(0xE40C292Cu, ResearchConfig.StableHash("a"));
    }

    [Test]
    public void Validador_AvisaSobreRiscos()
    {
        string sa = Path.GetTempPath();
        var risky = new ResearchConfig { affectCaptureEnabled = true, requireConsent = false, participantIdMode = "xpto" };
        var msgs = StudyConfigValidator.Validate(risky, pluginAvailable: false, streamingAssetsPath: sa).Select(i => i.ToString()).ToList();
        Assert.IsTrue(msgs.Any(m => m.Contains("[Error]") && m.Contains("MediaPipe")));
        Assert.IsTrue(msgs.Any(m => m.Contains("[Error]") && m.Contains("participantIdMode")));
        Assert.IsTrue(msgs.Any(m => m.Contains("[Warning]") && m.Contains("consentimento")));
        Assert.IsTrue(msgs.Any(m => m.Contains("[Warning]") && m.Contains("PIN")));
        Assert.IsTrue(msgs.Any(m => m.Contains("studyId")));

        var good = new ResearchConfig { studyId = "E", schoolId = "S", classId = "T", participantIdMode = "code", researcherPin = "8642" };
        Assert.IsFalse(StudyConfigValidator.Validate(good, true, sa).Any(i => i.level != StudyConfigValidator.Level.Info));
    }
}

public class ExportTests : AnalyticsTestBase
{
    [Test]
    public void Exportacao_GeraZipComManifesto_EChecksumsCorretos_SemMapaDeNomes()
    {
        var tm = CreateTelemetry();
        tm.StartSession("CARLA", 9, "f", "4");
        TelemetryManager.Log("door_approach", ("door", "Musical 1"));
        tm.EndSession("teste");
        File.WriteAllText(Path.Combine(Folder, "affect_S1.csv"), "utc,t_ms\n1,2\n");
        File.WriteAllText(Path.Combine(Folder, "outro_arquivo.txt"), "não deve ir");

        string outDir = Path.Combine(Folder, "saida");
        string zip = DataExporter.Export(outDir);
        StringAssert.Contains("coleta_EST1_ESC1_PC-TESTE_", Path.GetFileName(zip));

        using var za = ZipFile.OpenRead(zip);
        var names = za.Entries.Select(e => e.FullName).ToList();
        Assert.IsTrue(names.Any(n => n.StartsWith("events_")));
        Assert.Contains("affect_S1.csv", names);
        Assert.Contains("manifest.json", names);
        Assert.IsFalse(names.Contains("participants_map.csv"), "o mapa de nomes não pode sair por padrão");
        Assert.IsFalse(names.Contains("outro_arquivo.txt"));

        var manifest = MiniJson.Obj(new StreamReader(za.GetEntry("manifest.json").Open()).ReadToEnd());
        Assert.AreEqual("EST1", manifest["study_id"]);
        Assert.AreEqual(false, manifest["includes_name_map"]);
        foreach (Dictionary<string, object> f in (List<object>)manifest["files"])
        {
            using var ms = new MemoryStream();
            za.GetEntry((string)f["name"]).Open().CopyTo(ms);
            Assert.AreEqual(f["sha256"], DataExporter.Sha256(ms.ToArray()), $"checksum de {f["name"]}");
            Assert.AreEqual((double)ms.Length, f["bytes"]);
        }
    }

    [Test]
    public void Exportacao_IncluiMapa_QuandoPedido()
    {
        var tm = CreateTelemetry();
        tm.StartSession("CARLA", 9, "f", "4");
        tm.EndSession("teste");
        string zip = DataExporter.Export(Path.Combine(Folder, "saida2"), includeNameMap: true);
        using var za = ZipFile.OpenRead(zip);
        Assert.IsTrue(za.Entries.Any(e => e.FullName == "participants_map.csv"));
    }

    [Test]
    public void Exportacao_DuranteSessaoAberta_Funciona()
    {
        var tm = CreateTelemetry();
        tm.StartSession("CARLA", 9, "f", "4");
        string zip = DataExporter.Export(Path.Combine(Folder, "saida3"));
        Assert.IsTrue(File.Exists(zip));
        TelemetryManager.Log("door_leave", ("door", "x"));   // arquivo continua gravável
        tm.EndSession("teste");
    }
}

public class StudyToolsTests
{
    [Test]
    public void FolhaDeCodigos_GeraCsvEHtml_ComCodigosValidos()
    {
        string dir = Path.Combine(Path.GetTempPath(), "codigos_teste_" + System.Guid.NewGuid().ToString("N").Substring(0, 6));
        var (csv, html) = StudyTools.GenerateCodeSheet("E1", "ESC", "T4", 12, dir, seed: 42);
        var lines = File.ReadAllLines(csv).Skip(1).ToList();
        Assert.AreEqual(12, lines.Count);
        Assert.IsTrue(lines.All(l => ParticipantCodes.IsValid(l.Split(';')[0])));
        string page = File.ReadAllText(html);
        foreach (var l in lines) StringAssert.Contains(l.Split(';')[0], page);
        Directory.Delete(dir, true);
    }
}
