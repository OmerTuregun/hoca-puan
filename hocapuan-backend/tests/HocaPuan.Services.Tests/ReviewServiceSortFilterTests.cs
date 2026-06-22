using System.Text.Json;
using HocaPuan.Core.DTOs.Common;
using HocaPuan.Core.DTOs.Professor;
using HocaPuan.Core.Entities;
using HocaPuan.Core.Interfaces.Moderation;
using HocaPuan.Core.Interfaces.Services;
using HocaPuan.Core.Moderation;
using HocaPuan.Data;
using HocaPuan.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace HocaPuan.Services.Tests;

public class ReviewServiceSortFilterTests
{
    private static (AppDbContext Db, ReviewService Service, int ProfessorId) CreateWithReviews()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var db = new AppDbContext(options);
        var service = new ReviewService(
            db,
            new NoOpProfessorService(),
            new NoOpModerationService(),
            NullLogger<ReviewService>.Instance);

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
        var professor = new Professor
        {
            FirstName = "Ali",
            LastName = "Veli",
            Title = "Dr.",
            University = university,
            Department = department,
        };

        var reviews = new[]
        {
            new Review
            {
                Professor = professor,
                User = author,
                Year = 2024,
                QualityRating = 3,
                DifficultyRating = 3,
                WouldTakeAgain = true,
                AttendanceMandatory = false,
                Comment = "Orta seviye bir ders deneyimi yaşadım burada.",
                Status = ReviewStatus.Approved,
                TagsJson = JsonSerializer.Serialize(new[] { "Sıkıcı" }),
                ThumbsUp = 1,
                ThumbsDown = 0,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            },
            new Review
            {
                Professor = professor,
                User = author,
                Year = 2024,
                QualityRating = 5,
                DifficultyRating = 2,
                WouldTakeAgain = true,
                AttendanceMandatory = false,
                Comment = "Harika bir hoca, dersleri çok verimli geçti.",
                Status = ReviewStatus.Approved,
                TagsJson = JsonSerializer.Serialize(new[] { "Sınava Dayalı", "İlham Verici" }),
                ThumbsUp = 10,
                ThumbsDown = 2,
                CreatedAt = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc),
            },
            new Review
            {
                Professor = professor,
                User = author,
                Year = 2023,
                QualityRating = 1,
                DifficultyRating = 5,
                WouldTakeAgain = false,
                AttendanceMandatory = true,
                Comment = "Zor bir dönemdi, sınavlar çok zordu maalesef.",
                Status = ReviewStatus.Approved,
                TagsJson = JsonSerializer.Serialize(new[] { "Sınava Dayalı" }),
                ThumbsUp = 0,
                ThumbsDown = 0,
                CreatedAt = new DateTime(2023, 3, 1, 0, 0, 0, DateTimeKind.Utc),
            },
            new Review
            {
                Professor = professor,
                User = author,
                Year = 2025,
                QualityRating = 4,
                DifficultyRating = 4,
                WouldTakeAgain = true,
                AttendanceMandatory = false,
                Comment = "Bekleyen yorum — listede görünmemeli.",
                Status = ReviewStatus.Pending,
                TagsJson = "[]",
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            },
        };

        db.Add(author);
        db.AddRange(reviews);
        db.SaveChanges();

        return (db, service, professor.Id);
    }

    [Fact]
    public async Task GetByProfessorAsync_SortByNewest_OrdersDescendingCreatedAt()
    {
        var (_, service, professorId) = CreateWithReviews();

        var result = await service.GetByProfessorAsync(professorId, 1, 10, sortBy: "newest");

        Assert.Equal(3, result.Items.Count);
        Assert.Equal(5, result.Items[0].QualityRating);
        Assert.Equal(3, result.Items[1].QualityRating);
        Assert.Equal(1, result.Items[2].QualityRating);
    }

    [Fact]
    public async Task GetByProfessorAsync_SortByOldest_OrdersAscendingCreatedAt()
    {
        var (_, service, professorId) = CreateWithReviews();

        var result = await service.GetByProfessorAsync(professorId, 1, 10, sortBy: "oldest");

        Assert.Equal(1, result.Items[0].QualityRating);
        Assert.Equal(5, result.Items[^1].QualityRating);
    }

    [Fact]
    public async Task GetByProfessorAsync_SortByMostHelpful_OrdersByNetVotes()
    {
        var (_, service, professorId) = CreateWithReviews();

        var result = await service.GetByProfessorAsync(professorId, 1, 10, sortBy: "mostHelpful");

        Assert.Equal(5, result.Items[0].QualityRating);
        Assert.Equal(3, result.Items[1].QualityRating);
    }

    [Fact]
    public async Task GetByProfessorAsync_SortByHighestRating_OrdersByQualityDescending()
    {
        var (_, service, professorId) = CreateWithReviews();

        var result = await service.GetByProfessorAsync(professorId, 1, 10, sortBy: "highestRating");

        Assert.Equal(new[] { 5, 3, 1 }, result.Items.Select(i => i.QualityRating));
    }

    [Fact]
    public async Task GetByProfessorAsync_SortByLowestRating_OrdersByQualityAscending()
    {
        var (_, service, professorId) = CreateWithReviews();

        var result = await service.GetByProfessorAsync(professorId, 1, 10, sortBy: "lowestRating");

        Assert.Equal(new[] { 1, 3, 5 }, result.Items.Select(i => i.QualityRating));
    }

    [Fact]
    public async Task GetByProfessorAsync_FilterByTag_ReturnsMatchingReviewsOnly()
    {
        var (_, service, professorId) = CreateWithReviews();

        var result = await service.GetByProfessorAsync(
            professorId, 1, 10, tag: "Sınava Dayalı");

        Assert.Equal(2, result.TotalCount);
        Assert.All(result.Items, r => Assert.Contains("Sınava Dayalı", r.Tags));
    }

    private sealed class NoOpProfessorService : IProfessorService
    {
        public Task<ProfessorDetailDto?> GetByIdAsync(int id) => Task.FromResult<ProfessorDetailDto?>(null);
        public Task<PagedResultDto<ProfessorListItemDto>> SearchAsync(ProfessorSearchDto searchDto) =>
            Task.FromResult(new PagedResultDto<ProfessorListItemDto>());
        public Task<ProfessorDetailDto> CreateAsync(CreateProfessorDto dto) => throw new NotImplementedException();
        public Task<ProfessorDetailDto> UpdateAsync(int id, UpdateProfessorDto dto) => throw new NotImplementedException();
        public Task<bool> DeleteAsync(int id) => Task.FromResult(false);
        public Task RecalculateStatsAsync(int professorId) => Task.CompletedTask;
    }

    private sealed class NoOpModerationService : IContentModerationService
    {
        public ModerationResult Moderate(string text) =>
            new() { IsAllowed = true, RequiresManualReview = false };
    }
}
