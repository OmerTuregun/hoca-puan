using HocaPuan.Services.Moderation;
using Xunit;

namespace HocaPuan.Services.Tests;

public class LevenshteinDistanceTests
{
    [Theory]
    [InlineData("gerzekali", "gerizekali", 1)]
    [InlineData("aptl", "aptal", 1)]
    [InlineData("sier", "siker", 1)]
    [InlineData("abc", "abcd", 1)]
    [InlineData("abc", "ab", 1)]
    public void Compute_ReturnsExpectedDistance(string source, string target, int expected)
    {
        var distance = LevenshteinDistance.Compute(source, target);
        Assert.Equal(expected, distance);
    }

    [Fact]
    public void Compute_UnrelatedWords_ReturnsGreaterThanOne()
    {
        Assert.True(LevenshteinDistance.Compute("merhaba", "gerizekali") > LevenshteinDistance.MaxAllowedDistance);
    }

    [Fact]
    public void Compute_LengthDifferenceGreaterThanOne_SkipsEarly()
    {
        var distance = LevenshteinDistance.Compute("ab", "abcde");
        Assert.True(distance > LevenshteinDistance.MaxAllowedDistance);
    }
}
