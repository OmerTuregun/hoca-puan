using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace HocaPuan.Services.Moderation;

internal static partial class TextNormalizer
{
    private static readonly CultureInfo Tr = CultureInfo.GetCultureInfo("tr-TR");

    private static readonly Dictionary<char, char> LeetMap = new()
    {
        ['4'] = 'a', ['@'] = 'a',
        ['3'] = 'e',
        ['1'] = 'i', ['!'] = 'i', ['|'] = 'i',
        ['0'] = 'o',
        ['5'] = 's', ['$'] = 's',
        ['7'] = 't',
    };

    /// <summary>Eşleştirme için metnin birden fazla normalize edilmiş varyantını üretir.</summary>
    public static IEnumerable<string> BuildMatchVariants(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) yield break;

        var lowered = text.ToLower(Tr);
        yield return lowered;

        var asciiFolded = FoldTurkishChars(lowered);
        yield return asciiFolded;

        var leet = ApplyLeet(asciiFolded);
        yield return leet;

        var joined = JoinSeparatedLetters(lowered);
        yield return joined;
        yield return FoldTurkishChars(joined);
        yield return ApplyLeet(FoldTurkishChars(joined));

        var lettersOnly = LettersOnlyPerToken(lowered);
        yield return lettersOnly;
        yield return FoldTurkishChars(lettersOnly);
        yield return ApplyLeet(FoldTurkishChars(lettersOnly));
        yield return CollapseRepeatedLetters(lettersOnly);

        var collapsed = CollapseRepeatedLetters(lowered);
        yield return collapsed;
        yield return FoldTurkishChars(collapsed);

        yield return CollapseRepeatedLetters(leet);
    }

    public static string NormalizeWord(string word)
    {
        var w = word.ToLower(Tr);
        w = FoldTurkishChars(w);
        w = ApplyLeet(w);
        w = CollapseRepeatedLetters(w);
        return w;
    }

    public static string NormalizePhrase(string phrase)
    {
        var tokens = phrase.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        return string.Join(' ', tokens.Select(NormalizeWord).Where(t => t.Length > 0));
    }

    public static bool IsMultiWordPhrase(string entry) =>
        entry.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length > 1;

    public static string FoldTurkishChars(string text)
    {
        var sb = new StringBuilder(text.Length);
        foreach (var ch in text)
        {
            sb.Append(ch switch
            {
                'ı' or 'I' or 'İ' or 'i' => 'i',
                'ş' or 'Ş' => 's',
                'ç' or 'Ç' => 'c',
                'ğ' or 'Ğ' => 'g',
                'ü' or 'Ü' => 'u',
                'ö' or 'Ö' => 'o',
                _ => char.ToLower(ch, Tr)
            });
        }
        return sb.ToString();
    }

    public static string ApplyLeet(string text)
    {
        var sb = new StringBuilder(text.Length);
        foreach (var ch in text)
            sb.Append(LeetMap.TryGetValue(ch, out var mapped) ? mapped : ch);
        return sb.ToString();
    }

    public static string JoinSeparatedLetters(string text)
    {
        var tokens = text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        return string.Join(' ', tokens.Select(t => SeparatorBetweenLetters().Replace(t, "$1")));
    }

    public static string LettersOnlyPerToken(string text)
    {
        var tokens = text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        var cleaned = tokens.Select(token =>
        {
            var sb = new StringBuilder();
            foreach (var ch in token)
            {
                if (char.IsLetter(ch))
                    sb.Append(char.ToLower(ch, Tr));
            }
            return sb.ToString();
        }).Where(t => t.Length > 0);

        return string.Join(' ', cleaned);
    }

    public static string CollapseRepeatedLetters(string text)
    {
        return RepeatedLetter().Replace(text, "$1");
    }

    public static Regex BuildWordRegex(string normalizedWord)
    {
        var escaped = Regex.Escape(normalizedWord);
        return new Regex($@"(?<![\p{{L}}\d]){escaped}(?![\p{{L}}\d])",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
    }

    /// <summary>Çok kelimeli ifadeler; kelimeler arasında ayırıcı karakterlere izin verir.</summary>
    public static Regex BuildPhraseRegex(string normalizedPhrase)
    {
        var parts = normalizedPhrase.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2)
            throw new ArgumentException("Phrase must contain at least two words.", nameof(normalizedPhrase));

        var pattern = string.Join(@"[\W_]*", parts.Select(Regex.Escape));
        return new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
    }

    [GeneratedRegex(@"([\p{L}])[\*\.·_\-/\\|]+(?=[\p{L}])", RegexOptions.Compiled)]
    private static partial Regex SeparatorBetweenLetters();

    [GeneratedRegex(@"(.)\1+", RegexOptions.Compiled)]
    private static partial Regex RepeatedLetter();
}
