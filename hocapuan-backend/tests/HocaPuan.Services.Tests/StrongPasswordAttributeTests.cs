using HocaPuan.Core.Validation;
using Xunit;

namespace HocaPuan.Services.Tests;

public class StrongPasswordAttributeTests
{
    private readonly StrongPasswordAttribute _attribute = new();

    [Theory]
    [InlineData("Abc12345")]
    public void IsValid_StrongPassword_ReturnsTrue(string password)
    {
        Assert.True(_attribute.IsValid(password));
    }

    [Theory]
    [InlineData("abc12345")]
    [InlineData("ABCDEFGH")]
    [InlineData("Ab1")]
    [InlineData("Abcdefgh")]
    public void IsValid_WeakPassword_ReturnsFalse(string password)
    {
        Assert.False(_attribute.IsValid(password));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void IsValid_NullOrEmpty_ReturnsTrue(object? value)
    {
        Assert.True(_attribute.IsValid(value));
    }
}
