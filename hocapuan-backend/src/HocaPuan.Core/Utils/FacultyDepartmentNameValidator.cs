using System.Globalization;
using System.Text.RegularExpressions;

namespace HocaPuan.Core.Utils;

/// <summary>
/// YÖK scraper çıktısındaki fakülte/bölüm adlarını doğrular ve gerekirse çöp metinden kurtarır.
/// </summary>
public static class FacultyDepartmentNameValidator
{
    private static readonly CultureInfo Tr = CultureInfo.GetCultureInfo("tr-TR");

    private static readonly Regex MultiSpace = new(@"\s+", RegexOptions.Compiled);

    private static readonly Regex JunkMarkers = new(
        @"orcid|akademik\s*görev|akademik\s*gorev|öğrenim\s*bilgisi|ogrenim\s*bilgisi|" +
        @"birlikte\s*çalıştığı|birlikte\s*calistigi|personel\s*id|personelid|" +
        @"yabancı\s*diller\s*bölümü\s*öğrenim|yabanci\s*diller|" +
        @"araştırma\s*görevlisi|arastirma\s*gorevlisi|öğretim\s*görevlisi|ogretim\s*gorevlisi",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly Regex StandaloneYear = new(@"\b(19|20)\d{2}\b", RegexOptions.Compiled);

    private static readonly Regex MultipleUniversities = new(
        @"üniversitesi.*üniversitesi|universitesi.*universitesi",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly Regex YearRange = new(@"\b(19|20)\d{2}\s*[-–—]\s*(19|20)\d{2}\b", RegexOptions.Compiled);

    private static readonly Regex ValidFacultyUnit = new(
        @"fakültesi|fakultesi|enstitüsü|enstitusu|enstitü\b|enstitu\b|" +
        @"yüksekokulu|yuksekokulu|meslek\s*yüksekokulu|meslek\s*yuksekokulu|\bmyo\b|" +
        @"konservatuvar|rektörlük|rektorluk|" +
        @"uygulama\s*ve\s*araştırma\s*merkezi|uygulama\s*ve\s*arastirma\s*merkezi|" +
        @"araştırma\s*merkezi|arastirma\s*merkezi",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly Regex ValidDepartmentUnit = new(
        @"bölümü|bolumu|anabilim\s*dali|ana\s*bilim\s*dali|programı|programi\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    /// <summary>CV metninden kopmuş unvan parçaları (ör. "Araştırma Görevlisi" → "Örevlisi").</summary>
    private static readonly Regex TruncatedTitleStart = new(
        @"^(örevlisi|görevlisi|gorevlisi|kademik|ştığı|stigi|emik|ademik|visitesi|vler)\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly Regex EmbeddedUniversity = new(
        @"[\p{L}'\s]+?\s+üniversitesi",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly Regex FacultyExtract = new(
        @"((?:[^\n\r/|]{2,140}?(?:FAKÜLTESİ|FAKULTESI|FAKÜLTESI|ENSTİTÜSÜ|ENSTITUSU|ENSTİTÜSU|" +
        @"YÜKSEKOKULU|YUKSEKOKULU|MESLEK\s+YÜKSEKOKULU|MESLEK\s+YUKSEKOKULU|" +
        @"UYGULAMALI\s+BİLİMLER\s+YÜKSEKOKULU|UYGULAMALI\s+BILIMLER\s+YUKSEKOKULU|" +
        @"KONSERVATUVAR|REKTÖRLÜK|REKTORLUK|" +
        @"UYGULAMA\s+VE\s+ARAŞTIRMA\s+MERKEZİ|UYGULAMA\s+VE\s+ARASTIRMA\s+MERKEZI|" +
        @"ARAŞTIRMA\s+MERKEZİ|ARASTIRMA\s+MERKEZI))" +
        @"|(?:[^\n\r/|]{2,120}?\sMYO\b))",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly Regex DepartmentExtract = new(
        @"(?i)([^\n\r/|]{2,180}?(BÖLÜMÜ|BOLUMU|BÖLÜMU|ANABİLİM\s+DALI|ANABILIM\s+DALI|ANA\s+BİLİM\s+DALI|PROGRAMI))\b",
        RegexOptions.Compiled);

    public const string Unknown = "Bilinmiyor";

    public static string Normalize(string? raw) =>
        MultiSpace.Replace(raw ?? string.Empty, " ").Trim();

    /// <summary>UI'da gösterilebilir fakülte adı mı?</summary>
    /// <param name="hostUniversityName">Kayıtlı üniversite adı — metinde başka üniversite geçiyorsa reddedilir.</param>
    public static bool IsDisplayableFacultyName(string? name, string? hostUniversityName = null)
    {
        var n = Normalize(name);
        if (string.IsNullOrEmpty(n)) return false;
        if (n.Equals(Unknown, StringComparison.OrdinalIgnoreCase)) return false;
        if (!string.IsNullOrWhiteSpace(hostUniversityName) && ContainsForeignUniversity(n, hostUniversityName))
            return false;
        return IsValidUnitName(n, ValidFacultyUnit);
    }

    /// <summary>UI'da gösterilebilir bölüm adı mı?</summary>
    public static bool IsDisplayableDepartmentName(string? name)
    {
        var n = Normalize(name);
        if (string.IsNullOrEmpty(n)) return false;
        if (n.Equals(Unknown, StringComparison.OrdinalIgnoreCase)) return false;
        return IsValidUnitName(n, ValidDepartmentUnit);
    }

    /// <summary>Scraper kaydı için kabul edilebilir fakülte adı — geçersizse Bilinmiyor döner.</summary>
    public static string SanitizeFacultyName(string? name)
    {
        var n = Normalize(name);
        if (IsDisplayableFacultyName(n)) return ToTitleCaseTr(n);
        return Unknown;
    }

    public static string SanitizeDepartmentName(string? name)
    {
        var n = Normalize(name);
        if (IsDisplayableDepartmentName(n)) return ToTitleCaseTr(n);
        return Unknown;
    }

    /// <summary>CV/çöp metninden fakülte ve bölüm adı çıkarmayı dener.</summary>
    public static bool TrySalvage(string? raw, out string faculty, out string department)
    {
        faculty = Unknown;
        department = Unknown;
        var text = Normalize(raw);
        if (string.IsNullOrEmpty(text)) return false;

        var facultyMatch = FacultyExtract.Match(text);
        if (!facultyMatch.Success) return false;

        faculty = SanitizeFacultyName(facultyMatch.Groups[1].Value);
        if (faculty == Unknown) return false;

        var tail = text[Math.Min(text.Length, facultyMatch.Index + facultyMatch.Length)..];
        var deptMatch = DepartmentExtract.Match(tail);
        if (deptMatch.Success)
            department = SanitizeDepartmentName(deptMatch.Groups[1].Value);

        return faculty != Unknown;
    }

    /// <summary>Kesinlikle çöp — kurtarma denenmeden atlanmalı.</summary>
    public static bool IsObviouslyJunk(string? name)
    {
        var n = Normalize(name);
        if (string.IsNullOrEmpty(n)) return true;
        if (JunkMarkers.IsMatch(n)) return true;
        if (YearRange.IsMatch(n)) return true;
        if (StandaloneYear.IsMatch(n)) return true;
        if (MultipleUniversities.IsMatch(n)) return true;
        if (char.IsDigit(n[0])) return true;
        if (n.Length > 80) return true;
        if (n.Length > 100 && !ValidFacultyUnit.IsMatch(n) && !ValidDepartmentUnit.IsMatch(n)) return true;
        return false;
    }

    private static bool IsValidUnitName(string n, Regex unitPattern)
    {
        if (IsObviouslyJunk(n)) return false;
        if (TruncatedTitleStart.IsMatch(n)) return false;
        if (!unitPattern.IsMatch(n)) return false;
        // Ünvan ifadeleri birim adı değildir
        if (Regex.IsMatch(n, @"^(öğretim|ogretim|araştırma|arastirma)\s+(görevlisi|gorevlisi)\b", RegexOptions.IgnoreCase))
            return false;
        return true;
    }

    /// <summary>Fakülte adında kayıtlı üniversiteden farklı bir üniversite adı geçiyor mu?</summary>
    public static bool ContainsForeignUniversity(string facultyName, string hostUniversityName)
    {
        var hostKey = ExtractUniversityKey(hostUniversityName);
        if (string.IsNullOrEmpty(hostKey)) return false;

        foreach (Match match in EmbeddedUniversity.Matches(facultyName))
        {
            var embeddedKey = ExtractUniversityKey(match.Value);
            if (string.IsNullOrEmpty(embeddedKey)) continue;
            if (!UniversityKeysMatch(hostKey, embeddedKey))
                return true;
        }

        return false;
    }

    private static string ExtractUniversityKey(string name)
    {
        var n = Normalize(name).ToLower(Tr);
        var idx = n.IndexOf("üniversitesi", StringComparison.Ordinal);
        if (idx < 0) idx = n.IndexOf("universitesi", StringComparison.Ordinal);
        if (idx < 0) return n.Trim();
        return n[..idx].Trim();
    }

    private static bool UniversityKeysMatch(string hostKey, string embeddedKey)
    {
        if (hostKey == embeddedKey) return true;
        if (hostKey.Contains(embeddedKey, StringComparison.Ordinal) ||
            embeddedKey.Contains(hostKey, StringComparison.Ordinal))
            return true;

        var hostTokens = hostKey.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var embeddedTokens = embeddedKey.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (hostTokens.Length == 0 || embeddedTokens.Length == 0) return false;

        // "Kocaeli" ↔ "Kocaeli Üniversitesi" gibi kısa ad eşleşmeleri
        return hostTokens[0].Length >= 4 &&
               embeddedTokens[0].Length >= 4 &&
               (hostTokens[0] == embeddedTokens[0] ||
                hostKey.Contains(embeddedTokens[0], StringComparison.Ordinal) ||
                embeddedKey.Contains(hostTokens[0], StringComparison.Ordinal));
    }

    private static string ToTitleCaseTr(string s) =>
        string.IsNullOrWhiteSpace(s) ? string.Empty : Tr.TextInfo.ToTitleCase(s.ToLower(Tr));
}
