namespace HocaPuan.Services.Moderation;

/// <summary>
/// Kesin küfür köklerinin çekimlenmiş formlarını kelime başında yakalar.
/// Tam eşleşme ve fuzzy matching sonrası ek katman olarak kullanılır.
/// </summary>
internal static class VulgarRootMatcher
{
    internal sealed record RootMatch(string Root, string Token, string Category);

    private static readonly (string Root, string Category)[] Roots =
    [
        ("sik", "kufur_agir"),
        ("amk", "kufur_agir"),
        ("yarak", "kufur_agir"),
    ];

    private static readonly Dictionary<string, string[]> Exceptions = new(StringComparer.Ordinal)
    {
        ["sik"] = ["sikke", "sike", "sikorski"],
    };

    private const int MaxConjugationExtraLength = 8;

    public static IEnumerable<RootMatch> FindMatches(IEnumerable<string> tokens)
    {
        foreach (var token in tokens)
        {
            if (IsWhitelistedToken(token))
                continue;

            foreach (var (root, category) in Roots)
            {
                if (MatchesRootAtStart(token, root))
                    yield return new RootMatch(root, token, category);
            }
        }
    }

    public static bool IsWhitelistedToken(string token)
    {
        foreach (var exceptions in Exceptions.Values)
        {
            if (exceptions.Contains(token, StringComparer.Ordinal))
                return true;
        }

        return false;
    }

    private static bool MatchesRootAtStart(string token, string root)
    {
        if (token.Length <= root.Length)
            return false;

        if (token.Length > root.Length + MaxConjugationExtraLength)
            return false;

        if (token[0] != root[0])
            return false;

        if (token.StartsWith(root, StringComparison.Ordinal))
            return true;

        if (token.Length < root.Length + 2 || root.Length < 3)
            return false;

        if (!IsVowel(root[1]))
            return false;

        if (token[1] != root[2])
            return false;

        var shortPrefixLen = root.Length - 1;
        return LevenshteinDistance.Compute(token[..shortPrefixLen], root) <= LevenshteinDistance.MaxAllowedDistance;
    }

    private static bool IsVowel(char c) => "aeiouöüı".Contains(char.ToLowerInvariant(c));
}
