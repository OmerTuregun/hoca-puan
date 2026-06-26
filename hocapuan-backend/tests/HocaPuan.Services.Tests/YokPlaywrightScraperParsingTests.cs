using HocaPuan.Core.Utils;
using HocaPuan.Services;
using Xunit;

namespace HocaPuan.Services.Tests;

public class YokPlaywrightScraperParsingTests
{
    [Fact]
    public void ParseFacultyDepartmentFromPath_CleanUniversityPath_ParsesCorrectly()
    {
        var path = "ABDULLAH GÜL ÜNİVERSİTESİ/MÜHENDİSLİK FAKÜLTESİ/BİLGİSAYAR MÜHENDİSLİĞİ BÖLÜMÜ";
        var (faculty, dept) = YokPlaywrightScraperService.ParseFacultyDepartmentFromPath(
            path, "Abdullah Gül Üniversitesi");

        Assert.Contains("Mühendislik Fakültesi", faculty, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Bölümü", dept, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ParseFacultyDepartmentFromPath_CvGarbage_ReturnsBilinmiyor()
    {
        var path = "1474 Orcıd:0000-0002-0521-5794 Öğrenim Bilgisi 2021 Doktora Erciyes Üniversitesi Fen Bilimleri Enstitüsü";
        var (faculty, dept) = YokPlaywrightScraperService.ParseFacultyDepartmentFromPath(
            path, "Erciyes Üniversitesi");

        Assert.Equal(FacultyDepartmentNameValidator.Unknown, faculty);
        Assert.Equal(FacultyDepartmentNameValidator.Unknown, dept);
    }

    [Fact]
    public void ParseFacultyDepartmentFromPath_MidWordTruncation_ReturnsBilinmiyor()
    {
        var path = "Örevlisi Zonguldak Bülent Ecevit Üniversitesi Rektörlük";
        var (faculty, dept) = YokPlaywrightScraperService.ParseFacultyDepartmentFromPath(
            path, "Zonguldak Bülent Ecevit Üniversitesi");

        Assert.Equal(FacultyDepartmentNameValidator.Unknown, faculty);
        Assert.Equal(FacultyDepartmentNameValidator.Unknown, dept);
    }

    [Fact]
    public void ParseFacultyDepartmentFromPath_ValidFacultyOnly_StillWorks()
    {
        var path = "MÜHENDİSLİK FAKÜLTESİ/BİLGİSAYAR MÜHENDİSLİĞİ BÖLÜMÜ";
        var (faculty, dept) = YokPlaywrightScraperService.ParseFacultyDepartmentFromPath(
            path, "Kocaeli Üniversitesi");

        Assert.Contains("Mühendislik Fakültesi", faculty, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Bölümü", dept, StringComparison.OrdinalIgnoreCase);
    }
}
