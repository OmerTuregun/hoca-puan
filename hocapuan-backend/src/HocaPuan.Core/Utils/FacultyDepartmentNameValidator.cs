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

    private static readonly Regex EducationContextMarker = new(
        @"öğrenim\s*bilgisi|ogrenim\s*bilgisi|doktora|yüksek\s*lisans|yuksek\s*lisans|tezli|tezsiz",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly Regex InstituteSuffix = new(
        @"enstitüsü|enstitusu|enstitü\b|enstitu\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly string[] FacultyUnitSuffixes =
    [
        "UYGULAMA VE ARAŞTIRMA MERKEZİ", "UYGULAMA VE ARASTIRMA MERKEZI",
        "ARAŞTIRMA MERKEZİ", "ARASTIRMA MERKEZI",
        "UYGULAMALI BİLİMLER YÜKSEKOKULU", "MESLEK YÜKSEKOKULU",
        "ENSTİTÜSÜ", "ENSTITUSU", "YÜKSEKOKULU", "YUKSEKOKULU",
        "FAKÜLTESİ", "FAKULTESI", "KONSERVATUVAR", "REKTÖRLÜK", "REKTORLUK", " MYO"
    ];

    private static readonly Regex FacultyExtract = new(
        @"(?<![\p{L}])((?:(?!öğretim\s+görevlisi|ogretim\s+gorevlisi|araştırma\s+görevlisi|arastirma\s+gorevlisi)[^\n\r/|]){0,80}?" +
        @"(?:FAKÜLTESİ|FAKULTESI|FAKÜLTESI|ENSTİTÜSÜ|ENSTITUSU|ENSTİTÜSU|" +
        @"YÜKSEKOKULU|YUKSEKOKULU|MESLEK\s+YÜKSEKOKULU|MESLEK\s+YUKSEKOKULU|" +
        @"UYGULAMALI\s+BİLİMLER\s+YÜKSEKOKULU|UYGULAMALI\s+BILIMLER\s+YUKSEKOKULU|" +
        @"KONSERVATUVAR|REKTÖRLÜK|REKTORLUK|" +
        @"UYGULAMA\s+VE\s+ARAŞTIRMA\s+MERKEZİ|UYGULAMA\s+VE\s+ARASTIRMA\s+MERKEZI|" +
        @"ARAŞTIRMA\s+MERKEZİ|ARASTIRMA\s+MERKEZI)" +
        @"|(?:(?!öğretim\s+görevlisi|ogretim\s+gorevlisi|araştırma\s+görevlisi|arastirma\s+gorevlisi)[^\n\r/|]){0,80}?\sMYO\b)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex DepartmentExtract = new(
        @"(?i)([^\n\r/|]{2,180}?(BÖLÜMÜ|BOLUMU|BÖLÜMU|ANABİLİM\s+DALI|ANABILIM\s+DALI|ANA\s+BİLİM\s+DALI|PROGRAMI))\b",
        RegexOptions.Compiled);

    private static readonly Regex DepartmentCleanupJunkMarker = new(
        @"(orcid|orcıd|akademik|öğrenim\s*bilgisi|ogrenim\s*bilgisi|araştırma\s*görevlisi|arastirma\s*gorevlisi)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly Regex DepartmentPrefixDisqualifier = new(
        @"öğrenim\s*bilgisi|ogrenim\s*bilgisi|doktora|yüksek\s*lisans|yuksek\s*lisans|tezli|tezsiz|" +
        @"akademik\s*görev|akademik\s*gorev|birlikte\s*çalıştığı|birlikte\s*calistigi|" +
        @"orcid|orcıd|araştırma\s*görevlisi|arastirma\s*gorevlisi|öğretim\s*görevlisi|ogretim\s*gorevlisi",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    /// <summary>diag-data-quality.sh ile uyumlu geçerli bölüm adı kalıpları (kriter C).</summary>
    private static readonly Regex DiagValidDepartmentNaming = new(
        @"(bölümü|bolumu|anabilim|programı|programi|\bpr\.)|" +
        @"(mühendisliği|muhendisligi|hemşireliği|hemsireligi|mimarlığı|mimarligi)\s*$|" +
        @"^(hukuk|tıp|tip|eczacılık|eczacilik|adalet|mimarlık|mimarlik|hemşirelik|hemsirelik|" +
        @"diş hekimliği|dis hekimligi|islami ilimler)(\s+pr\.)?$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

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

    /// <summary>YÖK liste satırı veya detay sayfası CV dökümü mü?</summary>
    public static bool IsCvContentDump(string? text)
    {
        var n = Normalize(text);
        if (string.IsNullOrEmpty(n)) return false;

        var upper = n.ToUpper(Tr);
        string[] cvHeaders =
        [
            "ÖĞRENİM BİLGİSİ", "OGRENIM BILGISI",
            "AKADEMİK GÖREVLER", "AKADEMIK GOREVLER",
            "DOKTORA", "YÜKSEK LİSANS", "YUKSEK LISANS",
            "ORCID", "ORCİD",
            "BİRLİKTE ÇALIŞTIĞI", "BIRLIKTE CALISTIGI",
            "YÖNETİLEN TEZLER", "YONETILEN TEZLER"
        ];

        foreach (var header in cvHeaders)
        {
            if (upper.StartsWith(header, StringComparison.Ordinal)) return true;
        }

        if ((upper.Contains("ORCID", StringComparison.Ordinal) || upper.Contains("ORCİD", StringComparison.Ordinal))
            && EducationContextMarker.IsMatch(upper))
            return true;

        return false;
    }

    /// <summary>Eğitim geçmişindeki enstitü kaydını fakülte olarak kabul etme.</summary>
    public static bool IsInstituteMatchInEducationContext(string text, int matchIndex, string matchedValue)
    {
        if (!InstituteSuffix.IsMatch(matchedValue)) return false;
        var prefix = matchIndex > 0 ? text[..matchIndex] : string.Empty;
        var window = prefix.Length > 140 ? prefix[^140..] : prefix;
        return EducationContextMarker.IsMatch(window);
    }

    /// <summary>CV/çöp metninden fakülte ve bölüm adı çıkarmayı dener.</summary>
    public static bool TrySalvage(string? raw, out string faculty, out string department)
    {
        faculty = Unknown;
        department = Unknown;
        var text = Normalize(raw);
        if (string.IsNullOrEmpty(text)) return false;

        if (!TryFindFacultyMatch(text, out var facultyMatch)) return false;

        faculty = SanitizeFacultyName(facultyMatch.Value);
        if (faculty == Unknown) return false;

        var tail = text[Math.Min(text.Length, facultyMatch.Index + facultyMatch.Length)..];
        var deptMatch = DepartmentExtract.Match(tail);
        if (deptMatch.Success)
            department = SanitizeDepartmentName(deptMatch.Groups[1].Value);

        return faculty != Unknown;
    }

    /// <summary>Department temizliği hedefi — diag kriter A ve/veya B (kriter C hariç).</summary>
    public static bool ShouldCleanupDepartment(string? name)
    {
        var n = Normalize(name);
        if (string.IsNullOrEmpty(n)) return false;
        if (n.Equals(Unknown, StringComparison.OrdinalIgnoreCase)) return false;
        if (n.Length > 80) return true;
        return DepartmentCleanupJunkMarker.IsMatch(n);
    }

    /// <summary>Çöp bölüm metninden yalnızca bölüm adı kurtarmayı dener (eğitim geçmişi önekliyse reddeder).</summary>
    public static bool TrySalvageDepartmentName(string? raw, out string department)
    {
        department = Unknown;
        var text = Normalize(raw);
        if (string.IsNullOrEmpty(text)) return false;

        Match? lastMatch = null;
        foreach (Match m in DepartmentExtract.Matches(text))
        {
            if (m.Success) lastMatch = m;
        }

        if (lastMatch is not { Success: true }) return false;

        var prefix = lastMatch.Index > 0 ? text[..lastMatch.Index] : string.Empty;
        if (!string.IsNullOrEmpty(prefix) && DepartmentPrefixDisqualifier.IsMatch(prefix))
            return false;

        department = SanitizeDepartmentName(lastMatch.Groups[1].Value);
        return department != Unknown;
    }

    /// <summary>diag-data-quality.sh kriter C — geçerli adlandırma kalıbı var mı?</summary>
    public static bool HasDiagValidDepartmentNaming(string? name)
    {
        var n = Normalize(name);
        if (string.IsNullOrEmpty(n)) return false;
        return DiagValidDepartmentNaming.IsMatch(n);
    }

    /// <summary>Slash ayrıştırması yetmediğinde anahtar kelime ile fakülte/bölüm çıkarımı.</summary>
    public static bool TryExtractFacultyDepartmentFromKeywords(string? text, out string faculty, out string department)
    {
        faculty = Unknown;
        department = Unknown;
        var t = Normalize(text);
        if (string.IsNullOrEmpty(t)) return false;

        if (!TryFindFacultyMatch(t, out var facultyMatch)) return false;

        faculty = SanitizeFacultyName(facultyMatch.Value);
        if (faculty == Unknown) return false;

        var tail = t[Math.Min(t.Length, facultyMatch.Index + facultyMatch.Length)..];
        var deptMatch = DepartmentExtract.Match(tail);
        if (deptMatch.Success)
            department = SanitizeDepartmentName(deptMatch.Groups[1].Value);

        return faculty != Unknown || department != Unknown;
    }

    private static bool TryFindFacultyMatch(string text, out (int Index, int Length, string Value) match)
    {
        match = default;
        foreach (Match facultyMatch in FacultyExtract.Matches(text))
        {
            if (!facultyMatch.Success || facultyMatch.Groups.Count < 2) continue;
            var candidate = facultyMatch.Groups[1].Value;
            if (IsInstituteMatchInEducationContext(text, facultyMatch.Groups[1].Index, candidate))
                continue;
            if (SanitizeFacultyName(candidate) == Unknown)
                continue;
            match = (facultyMatch.Groups[1].Index, facultyMatch.Groups[1].Length, candidate);
            return true;
        }

        foreach (var suffix in FacultyUnitSuffixes)
        {
            var searchFrom = text.Length;
            while (searchFrom > 0)
            {
                var idx = text.LastIndexOf(suffix, searchFrom - 1, StringComparison.OrdinalIgnoreCase);
                if (idx < 0) break;

                var sliceStart = Math.Max(0, idx - 80);
                foreach (var sep in new[] { '/', '|', '\n', '\r' })
                {
                    var sepIdx = text.LastIndexOf(sep, idx - 1, idx - sliceStart);
                    if (sepIdx >= sliceStart) sliceStart = sepIdx + 1;
                }

                foreach (var marker in new[]
                         {
                             "öğretim görevlisi", "ogretim gorevlisi",
                             "araştırma görevlisi", "arastirma gorevlisi",
                             "akademik görevler", "akademik gorevler",
                             "öğrenim bilgisi", "ogrenim bilgisi"
                         })
                {
                    var markerIdx = text.LastIndexOf(marker, idx, StringComparison.OrdinalIgnoreCase);
                    if (markerIdx >= sliceStart)
                        sliceStart = markerIdx + marker.Length;
                }

                var candidate = text[sliceStart..(idx + suffix.Length)].Trim();
                if (!IsInstituteMatchInEducationContext(text, sliceStart, candidate))
                {
                    var sanitized = SanitizeFacultyName(candidate);
                    if (sanitized != Unknown)
                    {
                        match = (sliceStart, candidate.Length, candidate);
                        return true;
                    }
                }

                searchFrom = idx;
            }
        }

        return false;
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
        if (string.Equals(hostKey, embeddedKey, StringComparison.OrdinalIgnoreCase))
            return true;

        var hostTokens = TokenizeUniversityKey(hostKey);
        var embeddedTokens = TokenizeUniversityKey(embeddedKey);
        if (hostTokens.Count == 0 || embeddedTokens.Count == 0)
            return false;

        // Tam token eşleşmesi — "tepe" ⊂ "kocatepe" gibi alt-dizi eşleşmelerini reddet
        if (hostTokens.Any(h => embeddedTokens.Contains(h, StringComparer.OrdinalIgnoreCase)))
            return true;

        var hostPrimary = hostTokens[0];
        var embeddedPrimary = embeddedTokens[0];
        return hostPrimary.Length >= 4 &&
               embeddedPrimary.Length >= 4 &&
               string.Equals(hostPrimary, embeddedPrimary, StringComparison.OrdinalIgnoreCase);
    }

    private static List<string> TokenizeUniversityKey(string key) =>
        key.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(t => t.Length >= 2)
            .ToList();

    private static string ToTitleCaseTr(string s) =>
        string.IsNullOrWhiteSpace(s) ? string.Empty : Tr.TextInfo.ToTitleCase(s.ToLower(Tr));
}
