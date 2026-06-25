using System.Globalization;
using System.Text.RegularExpressions;

namespace HocaPuan.Core.Utils;

/// <summary>
/// Hoca adı ve ünvan alanlarındaki YÖK scraper hatalarını tespit eder ve düzeltir.
/// </summary>
public static class ProfessorNameValidator
{
    private static readonly CultureInfo Tr = CultureInfo.GetCultureInfo("tr-TR");

    private static readonly Regex MultiSpace = new(@"\s+", RegexOptions.Compiled);

    private static readonly Regex UnvanInTitle = new(
        @"\(Unvan:\s*([^)]+)\)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly Regex TitleLeakedIntoName = new(
        @"^(doktor\s+)?öğretim\s+(üyesi|görevlisi)|araştırma\s+görevlisi|prof\.?\s*dr|doç\.?\s*dr|dr\.?\s*öğr",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    public static string Normalize(string? raw) =>
        MultiSpace.Replace(raw ?? string.Empty, " ").Trim();

    public static bool HasProblematicTitle(string? title)
    {
        var t = Normalize(title);
        if (string.IsNullOrEmpty(t)) return false;
        if (UnvanInTitle.IsMatch(t)) return true;
        if (t.Length > 35) return true;
        if (TitleLeakedIntoName.IsMatch(t)) return true;
        return false;
    }

    public static bool HasProblematicNameParts(string? firstName, string? lastName)
    {
        var first = Normalize(firstName);
        var last = Normalize(lastName);
        if (first.Length > 40 || last.Length > 50) return true;
        if (TitleLeakedIntoName.IsMatch(first) || TitleLeakedIntoName.IsMatch(last)) return true;
        if (first.Contains('(') || last.Contains('(')) return true;
        return false;
    }

    /// <summary>Ünvan alanını kısa gösterim biçimine çevirir.</summary>
    public static string NormalizeTitle(string? rawTitle)
    {
        var t = Normalize(rawTitle);
        if (string.IsNullOrEmpty(t)) return "Öğr. Gör.";

        var unvan = UnvanInTitle.Match(t);
        if (unvan.Success)
        {
            var rank = unvan.Groups[1].Value.Trim();
            return MapRankToShortTitle(rank);
        }

        var upper = t.ToUpper(Tr);
        return upper switch
        {
            "DOKTOR ÖĞRETİM ÜYESİ" => "Dr. Öğr. Üyesi",
            "DOÇENT" => "Doç. Dr.",
            "PROFESÖR" => "Prof. Dr.",
            "ÖĞRETİM GÖREVLİSİ" => "Öğr. Gör.",
            "ARAŞTIRMA GÖREVLİSİ" => "Arş. Gör.",
            _ when upper.StartsWith("DOKTOR ÖĞRETİM ÜYESİ") => "Dr. Öğr. Üyesi",
            _ when upper.StartsWith("ÖĞRETİM GÖREVLİSİ") => "Öğr. Gör.",
            _ when upper.StartsWith("ARAŞTIRMA GÖREVLİSİ") => "Arş. Gör.",
            _ => Tr.TextInfo.ToTitleCase(t.ToLower(Tr))
        };
    }

    private static string MapRankToShortTitle(string rank)
    {
        var r = rank.Trim().ToLower(Tr);
        if (r.Contains("profesör") || r.Contains("profesor")) return "Prof. Dr.";
        if (r.Contains("doçent") || r.Contains("docent")) return "Doç. Dr.";
        if (r.Contains("dr. öğr") || r.Contains("doktor öğretim")) return "Dr. Öğr. Üyesi";
        if (r.Contains("öğretim görevlisi") || r.Contains("ogretim gorevlisi")) return "Öğr. Gör.";
        if (r.Contains("araştırma görevlisi") || r.Contains("arastirma gorevlisi")) return "Arş. Gör.";
        return "Öğr. Gör.";
    }
}
