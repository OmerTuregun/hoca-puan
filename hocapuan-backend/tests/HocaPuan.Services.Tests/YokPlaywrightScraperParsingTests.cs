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

    [Fact]
    public void ParseFacultyDepartmentFromPath_VanYyu_ComputerEngineering_FourLevelPath()
    {
        var path = "VAN YÜZÜNCÜ YIL ÜNİVERSİTESİ/MÜHENDİSLİK FAKÜLTESİ/BİLGİSAYAR MÜHENDİSLİĞİ BÖLÜMÜ/BİLGİSAYAR BİLİMLERİ ANABİLİM DALI/";
        var (faculty, dept) = YokPlaywrightScraperService.ParseFacultyDepartmentFromPath(
            path, "Van Yüzüncü Yıl Üniversitesi");

        Assert.Equal("Mühendislik Fakültesi", faculty);
        Assert.Equal("Bilgisayar Mühendisliği Bölümü", dept);
    }

    [Fact]
    public void ParseFacultyDepartmentFromPath_VanYyu_MaritimeEngineering_FourLevelPath()
    {
        var path = "VAN YÜZÜNCÜ YIL ÜNİVERSİTESİ/DENİZCİLİK FAKÜLTESİ/GEMİ MAKİNALARI İŞLETME MÜHENDİSLİĞİ BÖLÜMÜ/GEMİ MAKİNELERİ İŞLETME MÜHENDİSLİĞİ ANABİLİM DALI/";
        var (faculty, dept) = YokPlaywrightScraperService.ParseFacultyDepartmentFromPath(
            path, "Van Yüzüncü Yıl Üniversitesi");

        Assert.Equal("Denizcilik Fakültesi", faculty);
        Assert.Equal("Gemi Makinaları İşletme Mühendisliği Bölümü", dept);
    }

    [Fact]
    public void ParseFacultyDepartmentFromPath_VanYyu_Dentistry_FourLevelPath()
    {
        var path = "VAN YÜZÜNCÜ YIL ÜNİVERSİTESİ/DİŞ HEKİMLİĞİ FAKÜLTESİ/KLİNİK BİLİMLER BÖLÜMÜ/PEDODONTİ ANABİLİM DALI/";
        var (faculty, dept) = YokPlaywrightScraperService.ParseFacultyDepartmentFromPath(
            path, "Van Yüzüncü Yıl Üniversitesi");

        Assert.Equal("Diş Hekimliği Fakültesi", faculty);
        Assert.Equal("Klinik Bilimler Bölümü", dept);
    }

    [Fact]
    public void ParseFacultyDepartmentFromPath_VanYyu_MyoPath_StillWorks()
    {
        var path = "VAN YÜZÜNCÜ YIL ÜNİVERSİTESİ/BAŞKALE MESLEK YÜKSEKOKULU/BİLGİSAYAR TEKNOLOJİLERİ BÖLÜMÜ/BİLGİSAYAR PROGRAMCILIĞI PR./";
        var (faculty, dept) = YokPlaywrightScraperService.ParseFacultyDepartmentFromPath(
            path, "Van Yüzüncü Yıl Üniversitesi");

        Assert.Equal("Başkale Meslek Yüksekokulu", faculty);
        Assert.Equal("Bilgisayar Teknolojileri Bölümü", dept);
    }
}
