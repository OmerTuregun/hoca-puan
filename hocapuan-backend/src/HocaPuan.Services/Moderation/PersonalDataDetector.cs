using System.Text.RegularExpressions;

namespace HocaPuan.Services.Moderation;

internal static partial class PersonalDataDetector
{
    public static bool ContainsValidTcKimlikNo(string text)
    {
        foreach (Match match in TcCandidate().Matches(text))
        {
            if (IsValidTcKimlikNo(match.Value))
                return true;
        }
        return false;
    }

    public static bool ContainsPhoneNumber(string text)
    {
        return TurkishPhone().IsMatch(text);
    }

    public static bool ContainsEmail(string text)
    {
        return EmailAddress().IsMatch(text);
    }

    public static bool IsValidTcKimlikNo(string digits)
    {
        if (digits.Length != 11 || !digits.All(char.IsDigit))
            return false;

        if (digits[0] == '0')
            return false;

        var d = digits.Select(c => c - '0').ToArray();

        var odd = d[0] + d[2] + d[4] + d[6] + d[8];
        var even = d[1] + d[3] + d[5] + d[7];
        var digit10 = ((odd * 7) - even) % 10;
        if (digit10 < 0) digit10 += 10;
        if (d[9] != digit10)
            return false;

        var sumFirst10 = 0;
        for (var i = 0; i < 10; i++)
            sumFirst10 += d[i];

        return d[10] == sumFirst10 % 10;
    }

    [GeneratedRegex(@"\b\d{11}\b", RegexOptions.Compiled)]
    private static partial Regex TcCandidate();

    [GeneratedRegex(
        @"(?:\+90[\s\-.]?)?0?\s*5\d{2}[\s\-.]?\d{3}[\s\-.]?\d{2}[\s\-.]?\d{2}\b|" +
        @"\b5\d{2}[\s\-.]?\d{3}[\s\-.]?\d{2}[\s\-.]?\d{2}\b|" +
        @"\(\s*0?5\d{2}\s*\)[\s\-.]?\d{3}[\s\-.]?\d{2}[\s\-.]?\d{2}",
        RegexOptions.Compiled)]
    private static partial Regex TurkishPhone();

    [GeneratedRegex(
        @"\b[A-Za-z0-9._%+\-]+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,}\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex EmailAddress();
}
