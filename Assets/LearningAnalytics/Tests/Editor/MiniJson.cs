using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

/// <summary>
/// Parser JSON mínimo e estrito, usado só nos testes para garantir que as linhas gravadas
/// pelo jogo são JSON válido (o Unity não tem um parser genérico embutido).
/// Retorna Dictionary&lt;string, object&gt;, List&lt;object&gt;, string, double, bool ou null.
/// </summary>
public static class MiniJson
{
    public static object Parse(string json)
    {
        int i = 0;
        var v = Value(json, ref i);
        Ws(json, ref i);
        if (i != json.Length) throw new FormatException($"Lixo após o JSON na posição {i}");
        return v;
    }

    public static Dictionary<string, object> Obj(string json) => (Dictionary<string, object>)Parse(json);

    private static void Ws(string s, ref int i)
    {
        while (i < s.Length && (s[i] == ' ' || s[i] == '\t' || s[i] == '\n' || s[i] == '\r')) i++;
    }

    private static object Value(string s, ref int i)
    {
        Ws(s, ref i);
        if (i >= s.Length) throw new FormatException("JSON terminou inesperadamente");
        char c = s[i];
        if (c == '{') return Object(s, ref i);
        if (c == '[') return Array(s, ref i);
        if (c == '"') return Str(s, ref i);
        if (c == 't') { Expect(s, ref i, "true"); return true; }
        if (c == 'f') { Expect(s, ref i, "false"); return false; }
        if (c == 'n') { Expect(s, ref i, "null"); return null; }
        return Number(s, ref i);
    }

    private static void Expect(string s, ref int i, string word)
    {
        if (string.CompareOrdinal(s, i, word, 0, word.Length) != 0) throw new FormatException($"Esperado '{word}' na posição {i}");
        i += word.Length;
    }

    private static Dictionary<string, object> Object(string s, ref int i)
    {
        var d = new Dictionary<string, object>();
        i++;
        Ws(s, ref i);
        if (s[i] == '}') { i++; return d; }
        while (true)
        {
            Ws(s, ref i);
            if (s[i] != '"') throw new FormatException($"Chave esperada na posição {i}");
            string k = Str(s, ref i);
            Ws(s, ref i);
            if (s[i] != ':') throw new FormatException($"':' esperado na posição {i}");
            i++;
            if (d.ContainsKey(k)) throw new FormatException($"Chave duplicada: {k}");
            d[k] = Value(s, ref i);
            Ws(s, ref i);
            if (s[i] == ',') { i++; continue; }
            if (s[i] == '}') { i++; return d; }
            throw new FormatException($"',' ou '}}' esperado na posição {i}");
        }
    }

    private static List<object> Array(string s, ref int i)
    {
        var l = new List<object>();
        i++;
        Ws(s, ref i);
        if (s[i] == ']') { i++; return l; }
        while (true)
        {
            l.Add(Value(s, ref i));
            Ws(s, ref i);
            if (s[i] == ',') { i++; continue; }
            if (s[i] == ']') { i++; return l; }
            throw new FormatException($"',' ou ']' esperado na posição {i}");
        }
    }

    private static string Str(string s, ref int i)
    {
        var sb = new StringBuilder();
        i++;
        while (true)
        {
            if (i >= s.Length) throw new FormatException("String não terminada");
            char c = s[i++];
            if (c == '"') return sb.ToString();
            if (c < 0x20) throw new FormatException("Caractere de controle sem escape");
            if (c != '\\') { sb.Append(c); continue; }
            char e = s[i++];
            switch (e)
            {
                case '"': sb.Append('"'); break;
                case '\\': sb.Append('\\'); break;
                case '/': sb.Append('/'); break;
                case 'b': sb.Append('\b'); break;
                case 'f': sb.Append('\f'); break;
                case 'n': sb.Append('\n'); break;
                case 'r': sb.Append('\r'); break;
                case 't': sb.Append('\t'); break;
                case 'u': sb.Append((char)Convert.ToInt32(s.Substring(i, 4), 16)); i += 4; break;
                default: throw new FormatException($"Escape inválido \\{e}");
            }
        }
    }

    private static double Number(string s, ref int i)
    {
        int start = i;
        if (s[i] == '-') i++;
        while (i < s.Length && (char.IsDigit(s[i]) || s[i] == '.' || s[i] == 'e' || s[i] == 'E' || s[i] == '+' || s[i] == '-')) i++;
        string tok = s.Substring(start, i - start);
        if (!double.TryParse(tok, NumberStyles.Float, CultureInfo.InvariantCulture, out double v))
            throw new FormatException($"Número inválido '{tok}' na posição {start}");
        return v;
    }
}
