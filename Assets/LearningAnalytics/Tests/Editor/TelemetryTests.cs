using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

public class TelemetryTests : AnalyticsTestBase
{
    [Test]
    public void LinhaDeEvento_EhJsonValido_ComCaracteresEspeciais()
    {
        var data = new (string, object)[]
        {
            ("texto", "aspas \" barra \\ quebra\nlinha\ttab \u0001 ç ã é"),
            ("inteiro", 42), ("longo", 9_000_000_000L), ("real", 3.14159), ("nan", float.NaN), ("inf", double.PositiveInfinity),
            ("verdadeiro", true), ("nulo", null),
            ("lista", new[] { "a", "b" }), ("numeros", new List<int> { 1, 2, 3 }), ("vazia", new string[0]),
            ("dicionario", new Dictionary<string, float> { ["x"] = 1.5f, ["y"] = float.NaN }),
            ("vetor", new UnityEngine.Vector3(1, 2.5f, -3)),
        };
        string line = TelemetryManager.BuildLine(7, 1234.56, "S1", "P-1", "Cena \"A\"", "teste_evento", data);
        var ev = MiniJson.Obj(line);

        Assert.AreEqual(7.0, ev["seq"]);
        Assert.AreEqual(32, ((string)ev["id"]).Length);
        Assert.AreEqual("Cena \"A\"", ev["scene"]);
        var d = Data(ev);
        Assert.AreEqual("aspas \" barra \\ quebra\nlinha\ttab \u0001 ç ã é", d["texto"]);
        Assert.AreEqual(9_000_000_000.0, d["longo"]);
        Assert.IsNull(d["nan"], "NaN deve virar null");
        Assert.IsNull(d["inf"], "Infinito deve virar null");
        Assert.AreEqual(true, d["verdadeiro"]);
        Assert.AreEqual(3, ((List<object>)d["numeros"]).Count);
        Assert.AreEqual(0, ((List<object>)d["vazia"]).Count);
        Assert.IsNull(((Dictionary<string, object>)d["dicionario"])["y"]);
        Assert.AreEqual(-3.0, ((List<object>)d["vetor"])[2]);
    }

    [Test]
    public void LinhaDeEvento_UsaPontoDecimal_MesmoEmCulturaBrasileira()
    {
        var old = System.Threading.Thread.CurrentThread.CurrentCulture;
        try
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pt-BR");
            string line = TelemetryManager.BuildLine(1, 10.5, "S", "P", "", "e", new (string, object)[] { ("v", 2.75) });
            StringAssert.Contains("\"t_ms\":10.5", line);
            StringAssert.Contains("\"v\":2.75", line);
            MiniJson.Parse(line);
        }
        finally
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = old;
        }
    }

    [Test]
    public void Sessao_TemSequenciaContinua_IdsUnicos_EIdentificacaoDoEstudo()
    {
        var tm = CreateTelemetry();
        tm.StartSession("MARIA", 9, "f", "4º ano");
        for (int i = 0; i < 25; i++) TelemetryManager.Log("puzzle_attempt", ("attempt", i + 1));
        tm.EndSession("back_to_menu");

        var evs = ReadEvents();
        Assert.AreEqual(27, evs.Count);
        CollectionAssert.AreEqual(Enumerable.Range(1, 27).Select(i => (double)i).ToList(), evs.Select(e => (double)e["seq"]).ToList());
        Assert.AreEqual(evs.Count, evs.Select(e => (string)e["id"]).Distinct().Count(), "ids devem ser únicos");
        Assert.IsTrue(evs.All(e => (string)e["session"] == tm.SessionId));

        var start = Data(evs[0]);
        Assert.AreEqual("session_start", evs[0]["event"]);
        Assert.AreEqual("1.1", start["schema"]);
        Assert.AreEqual("EST1", start["study_id"]);
        Assert.AreEqual("ESC1", start["school_id"]);
        Assert.AreEqual("T4A", start["class_id"]);
        Assert.AreEqual("PC-TESTE", start["machine_id"]);
        Assert.AreEqual("name", start["id_mode"]);

        var end = evs.Last();
        Assert.AreEqual("session_end", end["event"]);
        Assert.AreEqual(false, Data(end)["recovered"]);
        Assert.IsFalse(File.Exists(Path.Combine(Folder, TelemetryManager.OpenSessionMarker)), "marcador de sessão aberta deve ser removido");
    }

    [Test]
    public void ModoNome_NuncaGravaONomeNosEventos_SoNoMapa()
    {
        var tm = CreateTelemetry();
        tm.StartSession("JOAOZINHO SILVA", 10, "m", "5º ano");
        tm.EndSession("teste");
        string events = File.ReadAllText(Directory.GetFiles(Folder, "events_*.jsonl").Single());
        StringAssert.DoesNotContain("JOAOZINHO", events);
        StringAssert.Contains("JOAOZINHO SILVA", File.ReadAllText(Path.Combine(Folder, "participants_map.csv")));
        StringAssert.StartsWith("P-", tm.ParticipantId);
    }

    [Test]
    public void ModoCodigo_UsaOCodigo_ENaoCriaMapaDeNomes()
    {
        ResearchConfig.SetCurrent(new ResearchConfig { participantIdMode = "code", studyId = "EST1" });
        string code = ParticipantCodes.Generate(1, 7)[0];
        var tm = CreateTelemetry();
        tm.StartSession(code.ToLowerInvariant(), 9, "f", "4º ano");
        tm.EndSession("teste");
        Assert.AreEqual(code, tm.ParticipantId);
        Assert.IsFalse(File.Exists(Path.Combine(Folder, "participants_map.csv")));
        Assert.AreEqual("code", Data(ReadEvents()[0])["id_mode"]);
    }

    [Test]
    public void MesmoParticipante_RecebeMesmoCodigoEmSessoesDiferentes()
    {
        var tm = CreateTelemetry();
        tm.StartSession("ANA", 8, "f", "3º ano");
        string first = tm.ParticipantId;
        tm.EndSession("teste");
        tm.StartSession("ANA", 8, "f", "3º ano");
        Assert.AreEqual(first, tm.ParticipantId);
        tm.EndSession("teste");
        Assert.AreEqual(2, Directory.GetFiles(Folder, "events_*.jsonl").Length);
    }

    [Test]
    public void SemSessao_LogNaoGravaNada()
    {
        CreateTelemetry();
        TelemetryManager.Log("puzzle_attempt", ("attempt", 1));
        Assert.AreEqual(0, Directory.GetFiles(Folder, "events_*.jsonl").Length);
    }

    [Test]
    public void TelemetriaDesligada_NaoIniciaSessao()
    {
        ResearchConfig.SetCurrent(new ResearchConfig { telemetryEnabled = false });
        var tm = CreateTelemetry();
        tm.StartSession("ANA", 8, "f", "3");
        Assert.IsFalse(tm.SessionActive);
        Assert.AreEqual(0, Directory.GetFiles(Folder).Length);
    }

    [Test]
    public void SessaoInterrompida_EhRecuperadaNaAberturaSeguinte()
    {
        // Simula o que fica no disco quando o jogo fecha sem encerrar a sessão.
        string file = Path.Combine(Folder, "events_S-INT.jsonl");
        File.WriteAllLines(file, new[]
        {
            TelemetryManager.BuildLine(1, 0, "S-INT", "P-X", "MainMenu", "session_start", new (string, object)[] { ("schema", "1.1") }),
            TelemetryManager.BuildLine(2, 5000, "S-INT", "P-X", "Area1", "scene_enter", null),
            TelemetryManager.BuildLine(3, 61234.5, "S-INT", "P-X", "Area1", "door_approach", null),
        });
        File.WriteAllText(Path.Combine(Folder, TelemetryManager.OpenSessionMarker),
            "{\"session\":\"S-INT\",\"participant\":\"P-X\",\"file\":\"events_S-INT.jsonl\",\"started_utc\":\"x\"}");

        Assert.AreEqual("S-INT", TelemetryManager.RecoverInterruptedSession(Folder));

        var evs = ReadEvents(file);
        Assert.AreEqual(4, evs.Count);
        var end = evs.Last();
        Assert.AreEqual("session_end", end["event"]);
        Assert.AreEqual(4.0, end["seq"]);
        Assert.AreEqual("interrupted", Data(end)["reason"]);
        Assert.AreEqual(true, Data(end)["recovered"]);
        Assert.AreEqual(61234.5, end["t_ms"]);
        Assert.IsFalse(File.Exists(Path.Combine(Folder, TelemetryManager.OpenSessionMarker)));
        Assert.IsNull(TelemetryManager.RecoverInterruptedSession(Folder), "recuperar duas vezes não pode duplicar o session_end");
    }

    [Test]
    public void MarcadorDeSessaoJaEncerrada_NaoAcrescentaNada()
    {
        string file = Path.Combine(Folder, "events_S-OK.jsonl");
        File.WriteAllLines(file, new[]
        {
            TelemetryManager.BuildLine(1, 0, "S-OK", "P", "", "session_start", null),
            TelemetryManager.BuildLine(2, 900, "S-OK", "P", "", "session_end", new (string, object)[] { ("reason", "app_quit") }),
        });
        File.WriteAllText(Path.Combine(Folder, TelemetryManager.OpenSessionMarker), "{\"session\":\"S-OK\",\"file\":\"events_S-OK.jsonl\"}");
        Assert.IsNull(TelemetryManager.RecoverInterruptedSession(Folder));
        Assert.AreEqual(2, File.ReadAllLines(file).Length);
    }

    [Test]
    public void SessaoReal_SemEncerrar_DeixaMarcador_QueOutraExecucaoRecupera()
    {
        var tm = CreateTelemetry();
        tm.StartSession("BIA", 9, "f", "4");
        TelemetryManager.Log("door_approach", ("door", "Musical 1"));
        Assert.IsTrue(File.Exists(Path.Combine(Folder, TelemetryManager.OpenSessionMarker)));

        // "Queda": libera o arquivo sem passar pelo EndSession.
        var writerField = typeof(TelemetryManager).GetField("_writer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        ((StreamWriter)writerField.GetValue(tm)).Dispose();
        typeof(TelemetryManager).GetProperty("SessionActive").SetValue(tm, false);

        Assert.AreEqual(tm.SessionId, TelemetryManager.RecoverInterruptedSession(Folder));
        var evs = ReadEvents();
        Assert.AreEqual("interrupted", Data(evs.Last())["reason"]);
        CollectionAssert.AreEqual(new[] { 1.0, 2.0, 3.0 }, evs.Select(e => (double)e["seq"]).ToArray());
    }
}
