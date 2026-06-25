using HocaPuan.Core.Utils;
using Xunit;

namespace HocaPuan.Services.Tests;

public class ProfessorNameValidatorTests
{
    [Theory]
    [InlineData("Doktor Öğretim Üyesi (Unvan:Doçent)", "Doç. Dr.")]
    [InlineData("Öğretim Görevlisi (Unvan:Doçent)", "Doç. Dr.")]
    [InlineData("Öğretim Görevlisi (Unvan:Profesör)", "Prof. Dr.")]
    public void NormalizeTitle_UnvanPattern(string raw, string expected) =>
        Assert.Equal(expected, ProfessorNameValidator.NormalizeTitle(raw));

    [Fact]
    public void HasProblematicTitle_Unvan_ReturnsTrue() =>
        Assert.True(ProfessorNameValidator.HasProblematicTitle("Doktor Öğretim Üyesi (Unvan:Doçent)"));
}
