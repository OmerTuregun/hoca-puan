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
    public void IsDisplayableFacultyName_KocatepeTepeFragment_False()
    {
        Assert.False(FacultyDepartmentNameValidator.IsDisplayableFacultyName(
            "Tepe Üniversitesi Rektörlük",
            "Afyon Kocatepe Üniversitesi"));
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

    [Fact]
    public void TrySalvage_EducationInstitute_ReturnsFalse()
    {
        var raw = "1474 Orcıd:0000-0002-0521-5794 Öğrenim Bilgisi 2021 Doktora Erciyes Üniversitesi Fen Bilimleri Enstitüsü";
        var ok = FacultyDepartmentNameValidator.TrySalvage(raw, out var faculty, out _);
        Assert.False(ok);
        Assert.Equal("Bilinmiyor", faculty);
    }

    [Fact]
    public void TryExtract_EducationInstitute_ReturnsBilinmiyor()
    {
        var raw = "1474 Orcıd:0000-0002-0521-5794 Öğrenim Bilgisi 2021 Doktora Erciyes Üniversitesi Fen Bilimleri Enstitüsü";
        var ok = FacultyDepartmentNameValidator.TryExtractFacultyDepartmentFromKeywords(raw, out var faculty, out _);
        Assert.False(ok);
        Assert.Equal("Bilinmiyor", faculty);
    }

    [Theory]
    [InlineData("ÖĞRENİM BİLGİSİ 2020 Lisans İstanbul Üniversitesi Mühendislik Fakültesi")]
    [InlineData("AKADEMİK GÖREVLER 2021 Doçent Ankara Üniversitesi Tıp Fakültesi")]
    [InlineData("ORCID: 0000-0001 Öğrenim Bilgisi Doktora")]
    public void IsCvContentDump_CvHeaders_True(string text) =>
        Assert.True(FacultyDepartmentNameValidator.IsCvContentDump(text));

    [Fact]
    public void IsDisplayableFacultyName_MidWordTruncation_False()
    {
        Assert.False(FacultyDepartmentNameValidator.IsDisplayableFacultyName(
            "Örevlisi Zonguldak Bülent Ecevit Üniversitesi Rektörlük"));
    }
}
