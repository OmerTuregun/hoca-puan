using System.Text.RegularExpressions;
using HocaPuan.Core.Interfaces.Moderation;
using HocaPuan.Core.Interfaces.Services;
using HocaPuan.Core.Moderation;
using Microsoft.Extensions.Logging;

namespace HocaPuan.Services.Moderation;

public class ContentModerationService : IContentModerationService
{
    private const string PersonalDataReason =
        "Yorumunuzda kişisel veri paylaşımı tespit edildi. Lütfen TC kimlik no, telefon numarası gibi bilgileri yorumdan çıkarın.";

    private const string InappropriateContentReason =
        "Yorumunuz uygunsuz içerik barındırıyor. Lütfen yorumunuzu düzenleyip tekrar deneyin.";

    private const int MinLetterCountForCapsCheck = 10;
    private const double ExcessiveCapsRatio = 0.5;

    private static readonly HashSet<string> AutoRejectCategories = new(StringComparer.OrdinalIgnoreCase)
    {
        "tehdit_siddet",
        "ayrimcilik_nefret",
        "hakaret_kucumseme",
        "argo_kucuk_dusurucu",
    };

    private static readonly HashSet<string> ManualReviewCategories = new(StringComparer.OrdinalIgnoreCase)
    {
        "kufur_orta",
        "hakaret_kisilik",
    };

    private static readonly Dictionary<string, string> CategoryDisplayNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ["kufur_agir"] = "Küfür",
        ["kufur_orta"] = "Küfür",
        ["hakaret_kisilik"] = "Hakaret",
        ["hakaret_kucumseme"] = "Hakaret",
        ["argo_kucuk_dusurucu"] = "Hakaret",
        ["ayrimcilik_nefret"] = "Nefret",
        ["tehdit_siddet"] = "Tehdit",
    };

    private static readonly Dictionary<string, string> ManualReviewCategoryReasons = new(StringComparer.OrdinalIgnoreCase)
    {
        ["kufur_orta"] = "Orta düzey küfür",
        ["hakaret_kisilik"] = "Kişilik hakareti",
    };

    private static readonly Regex ExcessivePunctuationRegex = new(@"[!?]{3,}", RegexOptions.Compiled);

    private readonly IBannedWordsProvider _bannedWordsProvider;
    private readonly ILogger<ContentModerationService> _logger;
    private readonly List<(string RawCategory, Regex Pattern)> _wordPatterns;
    private readonly List<(string RawCategory, Regex Pattern)> _phrasePatterns;
    private readonly FuzzyWordIndex _fuzzyWordIndex;

    public ContentModerationService(
        IBannedWordsProvider bannedWordsProvider,
        ILogger<ContentModerationService> logger)
    {
        _bannedWordsProvider = bannedWordsProvider;
        _logger = logger;
        (_wordPatterns, _phrasePatterns) = BuildPatterns();
        _fuzzyWordIndex = new FuzzyWordIndex(bannedWordsProvider);
    }

    public ModerationResult Moderate(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return new ModerationResult { IsAllowed = true };
        }

        if (PersonalDataDetector.ContainsValidTcKimlikNo(text) ||
            PersonalDataDetector.ContainsPhoneNumber(text) ||
            PersonalDataDetector.ContainsEmail(text))
        {
            return new ModerationResult
            {
                IsAllowed = false,
                RejectionReason = PersonalDataReason,
                MatchedCategories = ["KisiselVeri"]
            };
        }

        var (rawMatches, fuzzyMatches) = CollectRawCategoryMatches(text);

        LogFuzzyMatches(fuzzyMatches);

        if (rawMatches.Contains("kufur_agir"))
        {
            return Reject(rawMatches);
        }

        if (rawMatches.Any(c => AutoRejectCategories.Contains(c)))
        {
            return Reject(rawMatches);
        }

        var manualReviewReasons = new List<string>();

        foreach (var category in ManualReviewCategories)
        {
            if (!rawMatches.Contains(category)) continue;
            if (ManualReviewCategoryReasons.TryGetValue(category, out var reason))
                manualReviewReasons.Add(reason);
        }

        if (HasExcessiveCapsRatio(text))
            manualReviewReasons.Add("Aşırı büyük harf kullanımı");

        if (ExcessivePunctuationRegex.IsMatch(text))
            manualReviewReasons.Add("Aşırı ünlem/soru işareti");

        if (manualReviewReasons.Count > 0)
        {
            return new ModerationResult
            {
                IsAllowed = true,
                RequiresManualReview = true,
                ManualReviewReasons = manualReviewReasons.Distinct().ToList(),
                MatchedCategories = rawMatches
                    .Select(c => CategoryDisplayNames.GetValueOrDefault(c, c))
                    .Distinct()
                    .ToList()
            };
        }

        return new ModerationResult { IsAllowed = true };
    }

    private (HashSet<string> Matches, List<FuzzyMatch> FuzzyMatches) CollectRawCategoryMatches(string text)
    {
        var matches = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var fuzzyMatches = new List<FuzzyMatch>();
        var variants = TextNormalizer.BuildMatchVariants(text).Distinct().ToList();

        foreach (var (rawCategory, pattern) in _wordPatterns)
        {
            if (variants.Any(v => MatchesWordPattern(pattern, v)))
                matches.Add(rawCategory);
        }

        foreach (var (rawCategory, pattern) in _phrasePatterns)
        {
            if (variants.Any(v => pattern.IsMatch(v)))
                matches.Add(rawCategory);
        }

        CollectFuzzyMatches(variants, matches, fuzzyMatches);
        CollectRootMatches(variants, matches);

        return (matches, fuzzyMatches);
    }

    private void CollectRootMatches(
        IReadOnlyList<string> variants,
        HashSet<string> matches)
    {
        if (matches.Contains("kufur_agir"))
            return;

        var tokens = CollectNormalizedTokens(variants);
        var exactMatchedTokens = CollectExactMatchedTokens(variants);

        foreach (var rootMatch in VulgarRootMatcher.FindMatches(tokens))
        {
            if (exactMatchedTokens.Contains(rootMatch.Token))
                continue;

            matches.Add(rootMatch.Category);

            _logger.LogInformation(
                "Root match: user token '{UserToken}' ~ vulgar root '{Root}' (category: {Category})",
                rootMatch.Token,
                rootMatch.Root,
                rootMatch.Category);
        }
    }

    private void CollectFuzzyMatches(
        IReadOnlyList<string> variants,
        HashSet<string> matches,
        List<FuzzyMatch> fuzzyMatches)
    {
        var tokens = CollectNormalizedTokens(variants);
        var exactMatchedTokens = CollectExactMatchedTokens(variants);

        foreach (var token in tokens)
        {
            if (token.Length < 4)
                continue;

            if (exactMatchedTokens.Contains(token))
                continue;

            if (VulgarRootMatcher.IsWhitelistedToken(token))
                continue;

            foreach (var fuzzy in _fuzzyWordIndex.FindMatches(token))
            {
                matches.Add(fuzzy.Category);
                fuzzyMatches.Add(fuzzy);
            }
        }
    }

    private static HashSet<string> CollectNormalizedTokens(IEnumerable<string> variants)
    {
        var tokens = new HashSet<string>(StringComparer.Ordinal);
        foreach (var variant in variants)
        {
            foreach (var raw in variant.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
            {
                var normalized = TextNormalizer.NormalizeWord(raw);
                if (normalized.Length > 0)
                    tokens.Add(normalized);
            }
        }

        return tokens;
    }

    private HashSet<string> CollectExactMatchedTokens(IReadOnlyList<string> variants)
    {
        var matched = new HashSet<string>(StringComparer.Ordinal);

        foreach (var variant in variants)
        {
            foreach (var raw in variant.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
            {
                var normalized = TextNormalizer.NormalizeWord(raw);
                if (normalized.Length == 0)
                    continue;

                foreach (var (_, pattern) in _wordPatterns)
                {
                    if (pattern.IsMatch(normalized) || pattern.IsMatch(variant))
                    {
                        matched.Add(normalized);
                        break;
                    }
                }
            }
        }

        return matched;
    }

    private void LogFuzzyMatches(IReadOnlyList<FuzzyMatch> fuzzyMatches)
    {
        foreach (var fuzzy in fuzzyMatches)
        {
            _logger.LogInformation(
                "Fuzzy match: user token '{UserToken}' ~ banned word '{BannedWord}' (category: {Category})",
                fuzzy.UserToken,
                fuzzy.BannedWord,
                fuzzy.Category);
        }
    }

    private static ModerationResult Reject(IEnumerable<string> rawMatches) =>
        new()
        {
            IsAllowed = false,
            RejectionReason = InappropriateContentReason,
            MatchedCategories = rawMatches
                .Select(c => CategoryDisplayNames.GetValueOrDefault(c, c))
                .Distinct()
                .ToList()
        };

    private static bool HasExcessiveCapsRatio(string text)
    {
        var letters = text.Where(char.IsLetter).ToList();
        if (letters.Count < MinLetterCountForCapsCheck)
            return false;

        var upperCount = letters.Count(char.IsUpper);
        return (double)upperCount / letters.Count >= ExcessiveCapsRatio;
    }

    private (List<(string RawCategory, Regex Pattern)>, List<(string RawCategory, Regex Pattern)>) BuildPatterns()
    {
        var wordPatterns = new List<(string, Regex)>();
        var phrasePatterns = new List<(string, Regex)>();

        foreach (var (rawCategory, entries) in _bannedWordsProvider.GetRawWordsByCategory())
        {
            foreach (var entry in entries)
            {
                if (TextNormalizer.IsMultiWordPhrase(entry))
                {
                    var normalized = TextNormalizer.NormalizePhrase(entry);
                    if (normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length < 2)
                        continue;

                    phrasePatterns.Add((rawCategory, TextNormalizer.BuildPhraseRegex(normalized)));
                    continue;
                }

                var normalizedWord = TextNormalizer.NormalizeWord(entry);
                if (normalizedWord.Length < 2) continue;

                wordPatterns.Add((rawCategory, TextNormalizer.BuildWordRegex(normalizedWord)));
            }
        }

        return (wordPatterns, phrasePatterns);
    }

    private static bool MatchesWordPattern(Regex pattern, string variant)
    {
        if (pattern.IsMatch(variant)) return true;

        foreach (var token in variant.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
        {
            if (token.Length > 0 && pattern.IsMatch(token))
                return true;
        }

        return false;
    }
}
