using System.Globalization;

namespace HocaPuan.Core.Utils;

/// <summary>Arama önerileri için relevance skoru (yüksek = daha iyi eşleşme).</summary>
public static class SearchRelevance
{
    private static readonly CultureInfo Tr = CultureInfo.GetCultureInfo("tr-TR");

    public static int Score(string haystack, string normalizedQuery)
    {
        if (string.IsNullOrEmpty(normalizedQuery)) return 0;

        var folded = TurkishSearchNormalizer.Fold(haystack);
        if (folded.StartsWith(normalizedQuery, StringComparison.Ordinal)) return 100;

        foreach (var word in folded.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            if (word.StartsWith(normalizedQuery, StringComparison.Ordinal))
                return 85;
        }

        if (folded.Contains(normalizedQuery, StringComparison.Ordinal))
            return 50;

        return 0;
    }

    /// <summary>Yaygın bölüm adları için ek puan (ör. "bilgisayar" → Mühendisliği).</summary>
    public static int DepartmentBonus(string departmentName, string normalizedQuery)
    {
        var foldedName = TurkishSearchNormalizer.Fold(departmentName);
        var bonus = 0;

        if (normalizedQuery.StartsWith("bilgisayar", StringComparison.Ordinal)
            && foldedName.Contains("muhendisligi", StringComparison.Ordinal))
            bonus += 20;

        if (normalizedQuery.StartsWith("elektrik", StringComparison.Ordinal)
            && foldedName.Contains("muhendisligi", StringComparison.Ordinal))
            bonus += 15;

        if (normalizedQuery.StartsWith("makine", StringComparison.Ordinal)
            && foldedName.Contains("muhendisligi", StringComparison.Ordinal))
            bonus += 15;

        // Kısa, okunabilir bölüm adlarına hafif öncelik
        if (departmentName.Length <= 45) bonus += 3;

        return bonus;
    }

    public static string NormalizeDepartmentKey(string name)
    {
        var folded = TurkishSearchNormalizer.Fold(name);
        return folded
            .Replace(" bolumu", "", StringComparison.Ordinal)
            .Replace(" anabilim dali", "", StringComparison.Ordinal)
            .Trim();
    }
}
