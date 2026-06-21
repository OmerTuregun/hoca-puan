using HocaPuan.Core.Interfaces.Moderation;

namespace HocaPuan.Services.Moderation;

internal readonly record struct FuzzyMatch(string Category, string UserToken, string BannedWord);

/// <summary>
/// Yasaklı tekil kelimeleri uzunluğa göre gruplayarak Levenshtein fuzzy eşleştirme sağlar.
/// </summary>
internal sealed class FuzzyWordIndex
{
    private const int MinTokenLength = 4;

    private readonly Dictionary<int, List<(string Category, string Word)>> _wordsByLength;

    public FuzzyWordIndex(IBannedWordsProvider bannedWordsProvider)
    {
        _wordsByLength = new Dictionary<int, List<(string, string)>>();

        foreach (var (category, entries) in bannedWordsProvider.GetRawWordsByCategory())
        {
            foreach (var entry in entries)
            {
                if (TextNormalizer.IsMultiWordPhrase(entry))
                    continue;

                var normalized = TextNormalizer.NormalizeWord(entry);
                if (normalized.Length < 2)
                    continue;

                if (!_wordsByLength.TryGetValue(normalized.Length, out var bucket))
                {
                    bucket = [];
                    _wordsByLength[normalized.Length] = bucket;
                }

                if (bucket.Any(e => e.Word == normalized))
                    continue;

                bucket.Add((category, normalized));
            }
        }
    }

    public IEnumerable<FuzzyMatch> FindMatches(string normalizedToken)
    {
        if (normalizedToken.Length < MinTokenLength)
            yield break;

        var minLength = normalizedToken.Length - LevenshteinDistance.MaxAllowedDistance;
        var maxLength = normalizedToken.Length + LevenshteinDistance.MaxAllowedDistance;

        for (var length = minLength; length <= maxLength; length++)
        {
            if (!_wordsByLength.TryGetValue(length, out var candidates))
                continue;

            foreach (var (category, bannedWord) in candidates)
            {
                if (LevenshteinDistance.Compute(normalizedToken, bannedWord) <= LevenshteinDistance.MaxAllowedDistance)
                    yield return new FuzzyMatch(category, normalizedToken, bannedWord);
            }
        }
    }
}
