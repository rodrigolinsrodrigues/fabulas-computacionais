using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Códigos de participante para estudos sem nomes no computador.
///
/// Formato: 4 caracteres + 1 dígito verificador (ex.: "K7M2X"), usando um alfabeto de 31
/// caracteres sem ambíguos (sem 0/O, 1/I/L). O código é válido quando
///     Σ (posição × valor do caractere) ≡ 0 (mod 31),   posições 1..5.
/// Como 31 é primo e os pesos 1..5 são distintos, isso detecta com certeza qualquer troca de UM
/// caractere e qualquer inversão de dois caracteres vizinhos — os erros de digitação mais comuns.
/// (O mesmo algoritmo está no pacote R fabulasLA: codigo_valido() / gerar_codigos().)
/// </summary>
public static class ParticipantCodes
{
    public const string Alphabet = "23456789ABCDEFGHJKMNPQRSTUVWXYZ";
    public const int BodyLength = 4;
    public static int CodeLength => BodyLength + 1;

    /// <summary>Remove espaços e hífens e coloca em maiúsculas.</summary>
    public static string Normalize(string code)
    {
        if (string.IsNullOrEmpty(code)) return "";
        var sb = new StringBuilder(code.Length);
        foreach (char c in code.ToUpperInvariant())
            if (c != ' ' && c != '-' && c != '_' && c != '.') sb.Append(c);
        return sb.ToString();
    }

    public static bool IsValid(string code)
    {
        string c = Normalize(code);
        if (c.Length != CodeLength) return false;
        int sum = 0;
        for (int i = 0; i < c.Length; i++)
        {
            int v = Alphabet.IndexOf(c[i]);
            if (v < 0) return false;
            sum += (i + 1) * v;
        }
        return sum % Alphabet.Length == 0;
    }

    /// <summary>Dígito verificador: o valor c que torna Σ (i+1)·vᵢ + 5·c ≡ 0 (mod 31).</summary>
    public static char CheckChar(string body)
    {
        int n = Alphabet.Length;               // 31 (primo)
        int sum = 0;
        for (int i = 0; i < body.Length; i++) sum += (i + 1) * Alphabet.IndexOf(body[i]);
        int weight = body.Length + 1;          // peso da posição do verificador (5)
        int inverse = ModInverse(weight, n);
        int c = ((n - sum % n) % n * inverse) % n;
        return Alphabet[c];
    }

    private static int ModInverse(int a, int m)
    {
        for (int x = 1; x < m; x++)
            if (a * x % m == 1) return x;
        throw new ArgumentException("Sem inverso modular.");
    }

    /// <summary>Gera 'count' códigos distintos e válidos.</summary>
    public static List<string> Generate(int count, int? seed = null)
    {
        var rng = seed.HasValue ? new Random(seed.Value) : new Random();
        var set = new HashSet<string>();
        var list = new List<string>(count);
        long max = (long)Math.Pow(Alphabet.Length, BodyLength);
        if (count > max) throw new ArgumentException($"No máximo {max} códigos.");
        while (list.Count < count)
        {
            var sb = new StringBuilder(BodyLength);
            for (int i = 0; i < BodyLength; i++) sb.Append(Alphabet[rng.Next(Alphabet.Length)]);
            string body = sb.ToString();
            string code = body + CheckChar(body);
            if (set.Add(code)) list.Add(code);
        }
        return list;
    }
}
