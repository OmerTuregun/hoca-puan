using HocaPuan.Core.Entities;
using HocaPuan.Data;
using HocaPuan.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HocaPuan.Services.Tests;

public class UniversityServiceProfessorCountTests
{
    [Fact]
    public async Task GetAllAsync_UsesLiveProfessorCount_NotStaleColumn()
    {
        await using var db = CreateDb();
        var service = new UniversityService(db);

        var uni = new University
        {
            Name = "Afyon Kocatepe Üniversitesi",
            ShortName = "AKU",
            City = "Afyon",
            Type = UniversityType.Devlet,
            TotalProfessors = 0,
        };
        var faculty = new Faculty { Name = "Mühendislik Fakültesi", University = uni };
        var dept = new Department { Name = "Bilgisayar Mühendisliği Bölümü", Faculty = faculty };
        db.Add(new Professor
        {
            FirstName = "Ali",
            LastName = "Veli",
            Title = "Dr.",
            University = uni,
            Department = dept,
        });
        await db.SaveChangesAsync();

        var list = await service.GetAllAsync();
        var item = Assert.Single(list);

        Assert.Equal(0, uni.TotalProfessors);
        Assert.Equal(1, item.TotalProfessors);
    }

    [Fact]
    public async Task GetByIdAsync_UsesLiveProfessorCount_NotStaleColumn()
    {
        await using var db = CreateDb();
        var service = new UniversityService(db);

        var uni = new University
        {
            Name = "Kocaeli Üniversitesi",
            ShortName = "KOU",
            City = "Kocaeli",
            Type = UniversityType.Devlet,
            TotalProfessors = 0,
        };
        var faculty = new Faculty { Name = "Mühendislik Fakültesi", University = uni };
        var dept = new Department { Name = "Bilgisayar Mühendisliği Bölümü", Faculty = faculty };
        db.AddRange(
            new Professor { FirstName = "A", LastName = "B", Title = "Dr.", University = uni, Department = dept },
            new Professor { FirstName = "C", LastName = "D", Title = "Dr.", University = uni, Department = dept });
        await db.SaveChangesAsync();

        var detail = await service.GetByIdAsync(uni.Id);

        Assert.NotNull(detail);
        Assert.Equal(2, detail!.TotalProfessors);
    }

    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }
}
