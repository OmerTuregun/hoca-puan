using HocaPuan.Core.Utils;
using Xunit;

namespace HocaPuan.Services.Tests;

public class FacultyDepartmentNameValidatorTests
{
    [Theory]
    [InlineData("Mühendislik Fakültesi")]
    [InlineData("Rektörlük")]
    [InlineData("Denizcilik Fakültesi")]
    [InlineData("Kocaeli Meslek Yüksekokulu")]
    public void IsDisplayableFacultyName_ValidNames_True(string name) =>
        Assert.True(FacultyDepartmentNameValidator.IsDisplayableFacultyName(name));

    [Theory]
    [InlineData("Bilinmiyor")]
    [InlineData("310439 Orcıd:0000-0002-0521-5794 Birlikte Çalıştığı Kişiler Akademik Görevler")]
    [InlineData("Kademik Görevler 2024 Öğretim Görevlisi Kocaeli Üniversitesi Rektörlük Öğrenim Bilgisi")]
    [InlineData("Öğretim Görevlisi")]
    [InlineData("Örevlisi Zonguldak Bülent Ecevit Üniversitesi Rektörlük")]
    [InlineData("")]
    public void IsDisplayableFacultyName_JunkNames_False(string name) =>
        Assert.False(FacultyDepartmentNameValidator.IsDisplayableFacultyName(name));

    [Fact]
    public void IsDisplayableFacultyName_ForeignUniversityInName_False()
    {
        Assert.False(FacultyDepartmentNameValidator.IsDisplayableFacultyName(
            "Örevlisi Zonguldak Bülent Ecevit Üniversitesi Rektörlük",
            "Kocaeli Üniversitesi"));
    }

    [Fact]
    public void IsDisplayableFacultyName_HostUniversityInName_True()
    {
        Assert.True(FacultyDepartmentNameValidator.IsDisplayableFacultyName(
            "Kocaeli Üniversitesi Rektörlük",
            "Kocaeli Üniversitesi"));
    }

    [Fact]
    public void TrySalvage_ExtractsRektorlukFromCvGarbage()
    {
        var raw = "Görevler 2025 Öğretim Görevlisi Kocaeli Üniversitesi Rektörlük 2010-2017 Araştırma Görevlisi";
        var ok = FacultyDepartmentNameValidator.TrySalvage(raw, out var faculty, out _);
        Assert.True(ok);
        Assert.Contains("Rektörlük", faculty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void SanitizeFacultyName_JunkReturnsBilinmiyor()
    {
        var result = FacultyDepartmentNameValidator.SanitizeFacultyName(
            "310439 Orcıd:0000-0002-0521-5794 Akademik Görevler");
        Assert.Equal("Bilinmiyor", result);
    }
}
