using HocaPuan.Core.Validation;
using Xunit;

namespace HocaPuan.Services.Tests;

public class EduTrEmailAttributeTests
{
    private readonly EduTrEmailAttribute _attribute = new();

    [Theory]
    [InlineData("ogrenci@university.edu.tr")]
    [InlineData("OGRENCI@UNIVERSITY.EDU.TR")]
    public void IsValid_EduTrEmail_ReturnsTrue(string email)
    {
        Assert.True(_attribute.IsValid(email));
    }

    [Theory]
    [InlineData("ogrenci@gmail.com")]
    [InlineData("ogrenci@university.edu")]
    public void IsValid_NonEduTrEmail_ReturnsFalse(string email)
    {
        Assert.False(_attribute.IsValid(email));
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
