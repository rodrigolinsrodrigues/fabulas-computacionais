using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// "Estado da solução a cada tentativa": registra o que o jogador montou no
/// momento em que testou a solução, comparado com a resposta esperada e com a
/// tentativa anterior na mesma fase.
///
/// Cada puzzle descreve sua solução como uma lista de "posições" (slots):
///   Musical   → nota em cada posição da partitura
///   Conjuntos → objetos (tipo) dentro de cada espaço
///   Receitas  → insumo + ação em cada passo
///   Espelhos  → rotação/posição de cada pilar (não há gabarito fixo)
///
/// Gera o evento "solution_snapshot" no log de eventos (events_*.jsonl).
/// </summary>
public static class SolutionTracker
{
    /// <summary>Última solução enviada, por sessão + cena.</summary>
    private static readonly Dictionary<string, string[]> LastSlots = new Dictionary<string, string[]>();

    /// <param name="puzzle">"Musical", "Conjuntos", "Receitas" ou "Espelhos".</param>
    /// <param name="slots">O que o jogador colocou em cada posição ("" = vazio).</param>
    /// <param name="expected">Resposta esperada em cada posição, ou null quando não há gabarito.</param>
    /// <param name="success">Veredito do próprio jogo (null se ainda não for conhecido).</param>
    /// <param name="extra">Campos específicos do puzzle.</param>
    public static void LogAttempt(string puzzle, string[] slots, string[] expected, bool? success,
        params (string key, object value)[] extra)
    {
        var tm = TelemetryManager.Instance;
        if (tm == null || !tm.SessionActive || slots == null) return;

        try
        {
            string scene = SceneManager.GetActiveScene().name;
            string key = tm.SessionId + "|" + scene;

            int filled = 0, correct = 0, firstError = -1;
            for (int i = 0; i < slots.Length; i++)
            {
                if (!string.IsNullOrEmpty(slots[i])) filled++;
                if (expected != null && i < expected.Length)
                {
                    if (slots[i] == expected[i]) correct++;
                    else if (firstError < 0) firstError = i;
                }
            }

            // Posições alteradas desde a tentativa anterior nesta fase.
            List<int> changed = null;
            if (LastSlots.TryGetValue(key, out var prev))
            {
                changed = new List<int>();
                int n = Mathf.Max(prev.Length, slots.Length);
                for (int i = 0; i < n; i++)
                {
                    string a = i < prev.Length ? prev[i] : null;
                    string b = i < slots.Length ? slots[i] : null;
                    if (a != b) changed.Add(i);
                }
            }
            LastSlots[key] = (string[])slots.Clone();

            var data = new List<(string key, object value)>
            {
                ("puzzle", puzzle),
                ("attempt", CurrentAttempt()),
                ("slots", slots),
                ("total_positions", slots.Length),
                ("filled_positions", filled),
            };
            if (expected != null)
            {
                data.Add(("expected", expected));
                data.Add(("correct_positions", correct));
                data.Add(("first_error", firstError));
                data.Add(("distance", slots.Length - correct));
            }
            data.Add(("changed_positions", changed));            // null na 1ª tentativa
            data.Add(("n_changed", changed?.Count ?? -1));
            data.Add(("success", success));
            if (extra != null) data.AddRange(extra);

            TelemetryManager.Log("solution_snapshot", data.ToArray());
        }
        catch (Exception e)
        {
            // A coleta nunca deve quebrar o jogo.
            Debug.LogWarning("[SolutionTracker] Falha ao registrar a solução: " + e.Message);
        }
    }

    private static int CurrentAttempt()
    {
        var ldm = UnityEngine.Object.FindObjectOfType<LevelDataManager>();
        return ldm != null && ldm.currentLevelData != null ? ldm.currentLevelData.attempCounter : -1;
    }

    /// <summary>Nome do clipe de um AudioSource ("" se não houver).</summary>
    public static string ClipName(GameObject go)
    {
        if (go == null) return "";
        var src = go.GetComponent<AudioSource>();
        return src != null && src.clip != null ? src.clip.name : "";
    }

    public static string NameOf(GameObject go) => go != null ? go.name : "";
}
