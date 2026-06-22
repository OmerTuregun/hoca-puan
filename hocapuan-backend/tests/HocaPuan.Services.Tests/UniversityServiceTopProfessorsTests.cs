using HocaPuan.Core.Entities;
using HocaPuan.Data;
using HocaPuan.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HocaPuan.Services.Tests;

public class UniversityServiceTopProfessorsTests
{
    [Fact]
    public async Task GetTopProfessorsAsync_OrdersByApprovedReviewCountDescending()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var db = new AppDbContext(options);
        var service = new UniversityService(db);

        var university = new University
        {
            Name = "Test Üniversitesi",
            ShortName = "TU",
            City = "İstanbul",
            Type = UniversityType.Devlet,
        };
        var faculty = new Faculty { Name = "Mühendislik", University = university };
        var department = new Department { Name = "Bilgisayar", Faculty = faculty };
        var author = new User
        {
            Username = "yazar",
            Email = "yazar@itu.edu.tr",
            PasswordHash = "hash",
            IsEmailVerified = true,
        };

        var popular = new Professor
        {
            FirstName = "Popüler",
            LastName = "Hoca",
            Title = "Prof. Dr.",
            University = university,
            Department = department,
            TotalReviews = 5,
            AverageQuality = 4.5,
        };
        var quiet = new Professor
        {
            FirstName = "Az",
            LastName = "Yorumlu",
            Title = "Dr.",
            University = university,
            Department = department,
            TotalReviews = 1,
            AverageQuality = 3.0,
        };
        var none = new Professor
        {
            FirstName = "Yorum",
            LastName = "Yok",
            Title = "Arş. Gör.",
            University = university,
            Department = department,
            TotalReviews = 0,
        };

        db.Add(author);
        db.AddRange(popular, quiet, none);
        await db.SaveChangesAsync();

        var result = await service.GetTopProfessorsAsync(university.Id, limit: 10);

        Assert.Equal(2, result.Count);
        Assert.Contains("Popüler", result[0].FullName);
        Assert.Equal(5, result[0].TotalReviews);
        Assert.Equal("Mühendislik", result[0].FacultyName);
        Assert.Equal("Bilgisayar", result[0].DepartmentName);
    }
}
