using HocaPuan.Core.Entities;
using HocaPuan.Core.Utils;
using HocaPuan.Data;
using HocaPuan.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HocaPuan.Services.Tests;

public class FacultyDepartmentCleanupServiceTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task CleanupDepartmentsAsync_JunkCvDepartment_MovesProfessorToBilinmiyor()
    {
        await using var db = CreateDb();
        var university = new University
        {
            Name = "Test Üniversitesi",
            ShortName = "TU",
            City = "Ankara",
            Type = UniversityType.Devlet,
        };
        var faculty = new Faculty { Name = "Mühendislik Fakültesi", University = university };
        var junkDept = new Department
        {
            Name = "1474 Orcıd:0000-0003-0903-5800 Öğrenim Bilgisi 2021 Doktora Erciyes Üniversitesi Fen Bilimleri Enstitüsü",
            Faculty = faculty,
        };
        var professor = new Professor
        {
            FirstName = "Ali",
            LastName = "Veli",
            Title = "Dr.",
            University = university,
            Department = junkDept,
        };
        db.AddRange(university, faculty, junkDept, professor);
        await db.SaveChangesAsync();

        var result = await new FacultyDepartmentCleanupService(db).CleanupDepartmentsAsync();

        Assert.Equal(1, result.DepartmentsScanned);
        Assert.Equal(0, result.DepartmentsSalvaged);
        Assert.Equal(1, result.DepartmentsMovedToUnknown);
        Assert.Equal(1, result.ProfessorsReassigned);
        Assert.Equal(1, result.DepartmentsSoftDeleted);

        var updatedProf = await db.Professors.Include(p => p.Department).ThenInclude(d => d.Faculty)
            .SingleAsync(p => p.Id == professor.Id);
        Assert.Equal("Bilinmiyor", updatedProf.Department.Name);
        Assert.Equal("Bilinmiyor", updatedProf.Department.Faculty.Name);

        var junk = await db.Departments.SingleAsync(d => d.Id == junkDept.Id);
        Assert.True(junk.IsDeleted);
    }

    [Fact]
    public async Task CleanupDepartmentsAsync_ValidHukukPr_IsNotTouched()
    {
        await using var db = CreateDb();
        var university = new University
        {
            Name = "Ankara Bilim Üniversitesi",
            ShortName = "ABU",
            City = "Ankara",
            Type = UniversityType.Vakif,
        };
        var faculty = new Faculty { Name = "Hukuk Fakültesi", University = university };
        var validDept = new Department { Name = "Hukuk Pr.", Faculty = faculty };
        var professor = new Professor
        {
            FirstName = "Ayşe",
            LastName = "Yılmaz",
            Title = "Öğr. Gör.",
            University = university,
            Department = validDept,
        };
        db.AddRange(university, faculty, validDept, professor);
        await db.SaveChangesAsync();

        var result = await new FacultyDepartmentCleanupService(db).CleanupDepartmentsAsync();

        Assert.Equal(0, result.DepartmentsScanned);
        Assert.Equal(0, result.ProfessorsReassigned);

        var unchanged = await db.Departments.SingleAsync(d => d.Id == validDept.Id);
        Assert.False(unchanged.IsDeleted);
        Assert.Equal(validDept.Id, (await db.Professors.SingleAsync()).DepartmentId);
    }

    [Fact]
    public async Task CleanupDepartmentsAsync_ValidBilgisayarMuhendisligiBolumu_IsNotTouched()
    {
        await using var db = CreateDb();
        var university = new University
        {
            Name = "Kocaeli Üniversitesi",
            ShortName = "KOÜ",
            City = "Kocaeli",
            Type = UniversityType.Devlet,
        };
        var faculty = new Faculty { Name = "Mühendislik Fakültesi", University = university };
        var validDept = new Department { Name = "Bilgisayar Mühendisliği Bölümü", Faculty = faculty };
        db.AddRange(university, faculty, validDept);
        await db.SaveChangesAsync();

        var result = await new FacultyDepartmentCleanupService(db).CleanupDepartmentsAsync();
        Assert.Equal(0, result.DepartmentsScanned);
    }

    [Theory]
    [InlineData("Hukuk Pr.")]
    [InlineData("Enerji Sistemleri Mühendisliği")]
    [InlineData("Bilgisayar Mühendisliği Bölümü")]
    public void ShouldCleanupDepartment_ValidNames_False(string name) =>
        Assert.False(FacultyDepartmentNameValidator.ShouldCleanupDepartment(name));

    [Theory]
    [InlineData("Orcıd:0000-0001 Akademik Görevler 2020 Temel Eğitim Bölümü")]
    [InlineData("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")]
    public void ShouldCleanupDepartment_JunkNames_True(string name) =>
        Assert.True(FacultyDepartmentNameValidator.ShouldCleanupDepartment(name));

    [Fact]
    public void TrySalvageDepartmentName_EducationPrefixBeforeBolumu_ReturnsFalse()
    {
        var raw = "Akademik Görevler Öğrenim Bilgisi 2020 Doktora Ankara Üniversitesi Edebiyat Fakültesi Türk Dili Ve Edebiyatı Bölümü";
        var ok = FacultyDepartmentNameValidator.TrySalvageDepartmentName(raw, out var dept);
        Assert.False(ok);
        Assert.Equal("Bilinmiyor", dept);
    }

    [Theory]
    [InlineData("Hukuk Pr.")]
    [InlineData("Gıda Mühendisliği")]
    [InlineData("Bilgisayar Mühendisliği Bölümü")]
    public void HasDiagValidDepartmentNaming_ValidExamples_True(string name) =>
        Assert.True(FacultyDepartmentNameValidator.HasDiagValidDepartmentNaming(name));

    [Fact]
    public void HasDiagValidDepartmentNaming_GenelKulturDersleri_False() =>
        Assert.False(FacultyDepartmentNameValidator.HasDiagValidDepartmentNaming("Genel Kültür Dersleri"));
}
