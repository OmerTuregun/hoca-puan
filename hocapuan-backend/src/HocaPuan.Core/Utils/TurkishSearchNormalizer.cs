using System.Globalization;
using System.Text;

namespace HocaPuan.Core.Utils;

/// <summary>
/// Arama önerileri için Türkçe karakter duyarsız eşleştirme (İ/i, ı/I vb.).
/// Moderation katmanındaki <c>TextNormalizer.FoldTurkishChars</c> ile aynı kurallar.
/// </summary>
public static class TurkishSearchNormalizer
{
    private static readonly CultureInfo Tr = CultureInfo.GetCultureInfo("tr-TR");

    public static string Fold(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

        var sb = new StringBuilder(text.Length);
        foreach (var ch in text.Trim())
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

    /// <summary>PostgreSQL ILIKE için geniş ön filtre desenleri (Türkçe İ/i varyantları).</summary>
    public static IReadOnlyList<string> GetIlikePatterns(string query)
    {
        var trimmed = query.Trim();
        if (trimmed.Length < 2) return [];

        var patterns = new HashSet<string>(StringComparer.Ordinal);
        patterns.Add($"%{trimmed}%");

        var folded = Fold(trimmed);
        if (!string.Equals(folded, trimmed, StringComparison.OrdinalIgnoreCase))
            patterns.Add($"%{folded}%");

        var turkishCapitalI = trimmed.Replace('i', 'İ').Replace('I', 'İ');
        if (!string.Equals(turkishCapitalI, trimmed, StringComparison.Ordinal))
            patterns.Add($"%{turkishCapitalI}%");

        return patterns.ToList();
    }

    public static bool Matches(string haystack, string normalizedQuery) =>
        !string.IsNullOrEmpty(normalizedQuery) && Fold(haystack).Contains(normalizedQuery, StringComparison.Ordinal);

    public static bool StartsWithMatch(string haystack, string normalizedQuery) =>
        !string.IsNullOrEmpty(normalizedQuery) && Fold(haystack).StartsWith(normalizedQuery, StringComparison.Ordinal);
}
