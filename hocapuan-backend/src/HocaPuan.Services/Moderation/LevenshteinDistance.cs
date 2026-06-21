namespace HocaPuan.Services.Moderation;

/// <summary>İki string arasındaki minimum düzenleme uzaklığı (ekleme/silme/değiştirme).</summary>
internal static class LevenshteinDistance
{
    public const int MaxAllowedDistance = 1;

    public static int Compute(string source, string target)
    {
        if (source.Length == 0) return target.Length;
        if (target.Length == 0) return source.Length;

        var lengthDiff = Math.Abs(source.Length - target.Length);
        if (lengthDiff > MaxAllowedDistance)
            return lengthDiff;

        if (source.Length < target.Length)
            (source, target) = (target, source);

        var previous = new int[target.Length + 1];
        var current = new int[target.Length + 1];

        for (var j = 0; j <= target.Length; j++)
            previous[j] = j;

        for (var i = 1; i <= source.Length; i++)
        {
            current[0] = i;
            var rowMin = current[0];

            for (var j = 1; j <= target.Length; j++)
            {
                var cost = source[i - 1] == target[j - 1] ? 0 : 1;
                current[j] = Math.Min(
                    Math.Min(current[j - 1] + 1, previous[j] + 1),
                    previous[j - 1] + cost);

                rowMin = Math.Min(rowMin, current[j]);
            }

            if (rowMin > MaxAllowedDistance)
                return rowMin;

            (previous, current) = (current, previous);
        }

        return previous[target.Length];
    }
}
