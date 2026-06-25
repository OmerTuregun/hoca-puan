using HocaPuan.Core.DTOs.Common;
using HocaPuan.Core.DTOs.Professor;
using HocaPuan.Core.Entities;
using HocaPuan.Core.Interfaces.Services;
using HocaPuan.Core.Moderation;
using HocaPuan.Data;
using HocaPuan.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace HocaPuan.Services.Tests;

public class ReviewServiceContributionHistoryTests
{
    private static (AppDbContext Db, ReviewService Service) CreateService()
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

        return (db, service);
    }

    private static async Task<(User User, Professor Professor)> SeedBaseAsync(AppDbContext db)
    {
        var university = new University
        {
            Name = "Test Üniversitesi",
            ShortName = "TU",
            City = "İstanbul",
            Type = UniversityType.Devlet,
        };
        var faculty = new Faculty { Name = "Mühendislik", University = university };
        var department = new Department { Name = "Bilgisayar", Faculty = faculty };

        var user = new User
        {
            Username = "kullanici",
            Email = "kullanici@itu.edu.tr",
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

        db.AddRange(user, professor);
        await db.SaveChangesAsync();
        return (user, professor);
    }

    private static Review BuildReview(User user, Professor professor, ReviewStatus status, int thumbsUp = 0) =>
        new()
        {
            User = user,
            Professor = professor,
            Year = 2024,
            QualityRating = 4,
            DifficultyRating = 3,
            WouldTakeAgain = true,
            AttendanceMandatory = false,
            Comment = "Güzel bir ders.",
            Status = status,
            TagsJson = "[]",
            ThumbsUp = thumbsUp,
        };

    [Fact]
    public async Task GetContributionHistoryAsync_NoReviews_ReturnsZeroStats()
    {
        var (db, service) = CreateService();
        var (user, _) = await SeedBaseAsync(db);

        var result = await service.GetContributionHistoryAsync(user.Id, page: 1, pageSize: 10);

        Assert.Equal(0, result.TotalReviews);
        Assert.Equal(0, result.TotalHelpfulVotes);
        Assert.Empty(result.Reviews.Items);
    }

    [Fact]
    public async Task GetContributionHistoryAsync_CountsAllStatuses()
    {
        var (db, service) = CreateService();
        var (user, professor) = await SeedBaseAsync(db);

        db.Reviews.AddRange(
            BuildReview(user, professor, ReviewStatus.Approved),
            BuildReview(user, professor, ReviewStatus.Pending),
            BuildReview(user, professor, ReviewStatus.Rejected));
        await db.SaveChangesAsync();

        var result = await service.GetContributionHistoryAsync(user.Id, page: 1, pageSize: 10);

        Assert.Equal(3, result.TotalReviews);
        Assert.Equal(3, result.Reviews.TotalCount);
    }

    [Fact]
    public async Task GetContributionHistoryAsync_SumsTotalHelpfulVotes()
    {
        var (db, service) = CreateService();
        var (user, professor) = await SeedBaseAsync(db);

        db.Reviews.AddRange(
            BuildReview(user, professor, ReviewStatus.Approved, thumbsUp: 5),
            BuildReview(user, professor, ReviewStatus.Approved, thumbsUp: 3),
            BuildReview(user, professor, ReviewStatus.Pending, thumbsUp: 2));
        await db.SaveChangesAsync();

        var result = await service.GetContributionHistoryAsync(user.Id, page: 1, pageSize: 10);

        Assert.Equal(10, result.TotalHelpfulVotes);
    }

    [Fact]
    public async Task GetContributionHistoryAsync_ExcludesDeletedReviews()
    {
        var (db, service) = CreateService();
        var (user, professor) = await SeedBaseAsync(db);

        var deleted = BuildReview(user, professor, ReviewStatus.Approved, thumbsUp: 10);
        deleted.IsDeleted = true;

        db.Reviews.AddRange(
            BuildReview(user, professor, ReviewStatus.Approved, thumbsUp: 4),
            deleted);
        await db.SaveChangesAsync();

        var result = await service.GetContributionHistoryAsync(user.Id, page: 1, pageSize: 10);

        Assert.Equal(1, result.TotalReviews);
        Assert.Equal(4, result.TotalHelpfulVotes);
    }

    [Fact]
    public async Task GetContributionHistoryAsync_DoesNotIncludeOtherUsersReviews()
    {
        var (db, service) = CreateService();
        var (user, professor) = await SeedBaseAsync(db);

        var otherUser = new User
        {
            Username = "baska",
            Email = "baska@itu.edu.tr",
            PasswordHash = "hash",
            IsEmailVerified = true,
        };
        db.Users.Add(otherUser);
        await db.SaveChangesAsync();

        db.Reviews.AddRange(
            BuildReview(user, professor, ReviewStatus.Approved, thumbsUp: 3),
            BuildReview(otherUser, professor, ReviewStatus.Approved, thumbsUp: 99));
        await db.SaveChangesAsync();

        var result = await service.GetContributionHistoryAsync(user.Id, page: 1, pageSize: 10);

        Assert.Equal(1, result.TotalReviews);
        Assert.Equal(3, result.TotalHelpfulVotes);
        Assert.Single(result.Reviews.Items);
    }

    [Fact]
    public async Task GetContributionHistoryAsync_PaginationWorks()
    {
        var (db, service) = CreateService();
        var (user, professor) = await SeedBaseAsync(db);

        for (var i = 0; i < 5; i++)
            db.Reviews.Add(BuildReview(user, professor, ReviewStatus.Approved));
        await db.SaveChangesAsync();

        var page1 = await service.GetContributionHistoryAsync(user.Id, page: 1, pageSize: 3);
        var page2 = await service.GetContributionHistoryAsync(user.Id, page: 2, pageSize: 3);

        Assert.Equal(5, page1.TotalReviews);
        Assert.Equal(3, page1.Reviews.Items.Count);
        Assert.Equal(2, page2.Reviews.Items.Count);
        Assert.Equal(2, page1.Reviews.TotalPages);
        Assert.True(page1.Reviews.HasNextPage);
        Assert.False(page1.Reviews.HasPreviousPage);
        Assert.False(page2.Reviews.HasNextPage);
        Assert.True(page2.Reviews.HasPreviousPage);
    }

    [Fact]
    public async Task GetContributionHistoryAsync_ReviewsOrderedByNewestFirst()
    {
        var (db, service) = CreateService();
        var (user, professor) = await SeedBaseAsync(db);

        var older = BuildReview(user, professor, ReviewStatus.Approved);
        older.CreatedAt = DateTime.UtcNow.AddDays(-5);
        var newer = BuildReview(user, professor, ReviewStatus.Approved);
        newer.CreatedAt = DateTime.UtcNow;

        db.Reviews.AddRange(older, newer);
        await db.SaveChangesAsync();

        var result = await service.GetContributionHistoryAsync(user.Id, page: 1, pageSize: 10);

        Assert.Equal(2, result.Reviews.Items.Count);
        Assert.True(result.Reviews.Items[0].CreatedAt >= result.Reviews.Items[1].CreatedAt);
    }

    [Fact]
    public async Task GetContributionHistoryAsync_ReviewDtoContainsProfessorName()
    {
        var (db, service) = CreateService();
        var (user, professor) = await SeedBaseAsync(db);

        db.Reviews.Add(BuildReview(user, professor, ReviewStatus.Approved));
        await db.SaveChangesAsync();

        var result = await service.GetContributionHistoryAsync(user.Id, page: 1, pageSize: 10);

        var dto = Assert.Single(result.Reviews.Items);
        Assert.Equal("Dr. Ali Veli", dto.ProfessorFullName);
        Assert.Equal("Approved", dto.Status);
    }

    private sealed class NoOpProfessorService : IProfessorService
    {
        public Task<ProfessorDetailDto?> GetByIdAsync(int id) => Task.FromResult<ProfessorDetailDto?>(null);
        public Task<PagedResultDto<ProfessorListItemDto>> SearchAsync(ProfessorSearchDto searchDto) =>
            Task.FromResult(new PagedResultDto<ProfessorListItemDto>());
        public Task<ProfessorDetailDto> CreateAsync(CreateProfessorDto dto) =>
            throw new NotImplementedException();
        public Task<ProfessorDetailDto> UpdateAsync(int id, UpdateProfessorDto dto) =>
            throw new NotImplementedException();
        public Task<bool> DeleteAsync(int id) => Task.FromResult(false);
        public Task RecalculateStatsAsync(int professorId) => Task.CompletedTask;
    }

    private sealed class NoOpModerationService : IContentModerationService
    {
        public ModerationResult Moderate(string text) =>
            new() { IsAllowed = true, RequiresManualReview = false };
    }
}
