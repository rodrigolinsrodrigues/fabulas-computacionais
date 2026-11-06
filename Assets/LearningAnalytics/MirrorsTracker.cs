using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

/// <summary>
/// Estado da solução no puzzle Espelhos (Caminho da Luz).
///
/// Não existe gabarito fixo: a solução é física (posição e rotação dos pilares).
/// Por isso registramos:
///  - "solution_snapshot" ao disparar a luz: configuração de cada pilar;
///  - "beam_result" quando a luz apaga: quantos/quais trechos do feixe acenderam
///    e onde ele foi bloqueado (alcance do feixe = proximidade da solução);
///  - "solution_final" na vitória: a configuração vencedora, para calcular
///    depois a distância de cada tentativa até ela.
/// </summary>
public static class MirrorsTracker
{
    private static bool _running;
    private static LightPass[] _segments;
    private static readonly HashSet<string> Lit = new HashSet<string>();
    private static readonly List<string> Blocked = new List<string>();
    private static float _startTime;

    public static bool Running => _running;

    /// <summary>Chamado pelo EmitLight quando o jogador dispara a luz.</summary>
    public static void BeginAttempt()
    {
        try
        {
            _running = true;
            _startTime = Time.time;
            Lit.Clear();
            Blocked.Clear();
            _segments = UnityEngine.Object.FindObjectsOfType<LightPass>(true);

            var (names, slots) = PillarConfig();
            SolutionTracker.LogAttempt("Espelhos", slots, null, null,
                ("pillars", names),
                ("beam_segments_total", _segments.Length));
        }
        catch (Exception e)
        {
            Debug.LogWarning("[MirrorsTracker] " + e.Message);
        }
    }

    /// <summary>Chamado a cada quadro pelo EmitLight enquanto a luz do emissor estiver acesa.</summary>
    public static void Tick()
    {
        if (!_running || _segments == null) return;
        foreach (var seg in _segments)
        {
            if (seg != null && seg.ThisLight != null && seg.ThisLight.activeSelf) Lit.Add(seg.name);
        }
    }

    /// <summary>Chamado pelo StopScale quando o feixe bate em algo que não o reflete.</summary>
    public static void BeamBlocked(string segment, string hitObject)
    {
        if (!_running) return;
        Blocked.Add(segment + "->" + hitObject);
        TelemetryManager.Log("beam_blocked", ("segment", segment), ("hit", hitObject));
    }

    /// <summary>Chamado pelo EmitLight quando a luz do emissor apaga.</summary>
    public static void EndAttempt()
    {
        if (!_running) return;
        _running = false;
        var ldm = UnityEngine.Object.FindObjectOfType<LevelDataManager>();
        bool won = ldm != null && ldm.currentLevelData != null && ldm.currentLevelData.hasWin;
        var lit = new List<string>(Lit);
        lit.Sort(StringComparer.Ordinal);
        TelemetryManager.Log("beam_result",
            ("segments_lit", lit.Count),
            ("segments_total", _segments != null ? _segments.Length : 0),
            ("lit", lit),
            ("blocked", new List<string>(Blocked)),
            ("duration_s", Time.time - _startTime),
            ("success", won));
    }

    /// <summary>Chamado pelo showCanvas na vitória.</summary>
    public static void LogFinal()
    {
        var (names, slots) = PillarConfig();
        TelemetryManager.Log("solution_final", ("puzzle", "Espelhos"), ("pillars", names), ("slots", slots));
    }

    /// <summary>Pilares interativos (tags Rotate/Launch e os que já foram movidos), em ordem estável.</summary>
    private static (string[] names, string[] slots) PillarConfig()
    {
        var set = new Dictionary<int, GameObject>();
        foreach (var tag in new[] { "Rotate", "Launch" })
        {
            try
            {
                foreach (var go in GameObject.FindGameObjectsWithTag(tag)) set[go.GetInstanceID()] = go;
            }
            catch (UnityException) { /* tag inexistente nesta cena */ }
        }
        foreach (var pm in UnityEngine.Object.FindObjectsOfType<pilarMov>()) set[pm.gameObject.GetInstanceID()] = pm.gameObject;

        var list = new List<GameObject>(set.Values);
        list.Sort((a, b) => string.CompareOrdinal(PathOf(a), PathOf(b)));

        var names = new string[list.Count];
        var slots = new string[list.Count];
        for (int i = 0; i < list.Count; i++)
        {
            var t = list[i].transform;
            names[i] = list[i].name;
            Vector3 r = t.eulerAngles, p = t.position;
            slots[i] = string.Format(CultureInfo.InvariantCulture, "r={0},{1},{2};p={3:0.0},{4:0.0}",
                Snap(r.x), Snap(r.y), Snap(r.z), p.x, p.z);
        }
        return (names, slots);
    }

    private static int Snap(float angle) => (int)(Mathf.Round(Mathf.Repeat(angle, 360f) / 15f) * 15f) % 360;

    private static string PathOf(GameObject go)
    {
        var t = go.transform;
        string path = t.name;
        while (t.parent != null)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }
        return path;
    }
}
