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

public class ReviewServiceReportTests
{
    private sealed class TrackingProfessorService : IProfessorService
    {
        public List<int> RecalculatedProfessorIds { get; } = [];

        public Task<ProfessorDetailDto?> GetByIdAsync(int id) => Task.FromResult<ProfessorDetailDto?>(null);
        public Task<PagedResultDto<ProfessorListItemDto>> SearchAsync(ProfessorSearchDto searchDto) =>
            Task.FromResult(new PagedResultDto<ProfessorListItemDto>());
        public Task<ProfessorDetailDto> CreateAsync(CreateProfessorDto dto) =>
            throw new NotImplementedException();
        public Task<ProfessorDetailDto> UpdateAsync(int id, UpdateProfessorDto dto) =>
            throw new NotImplementedException();
        public Task<bool> DeleteAsync(int id) => Task.FromResult(false);
        public Task RecalculateStatsAsync(int professorId)
        {
            RecalculatedProfessorIds.Add(professorId);
            return Task.CompletedTask;
        }
    }

    private static async Task<(AppDbContext Db, ReviewService Service, TrackingProfessorService ProfessorService, Review Review, User Owner, User[] Reporters)>
        SeedApprovedReviewAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var db = new AppDbContext(options);
        var professorService = new TrackingProfessorService();
        var service = new ReviewService(
            db,
            professorService,
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

        var owner = new User
        {
            Username = "yazar",
            Email = "yazar@itu.edu.tr",
            PasswordHash = "hash",
            IsEmailVerified = true,
        };

        var reporters = new[]
        {
            new User { Username = "r1", Email = "r1@itu.edu.tr", PasswordHash = "hash", IsEmailVerified = true },
            new User { Username = "r2", Email = "r2@itu.edu.tr", PasswordHash = "hash", IsEmailVerified = true },
            new User { Username = "r3", Email = "r3@itu.edu.tr", PasswordHash = "hash", IsEmailVerified = true },
        };

        var professor = new Professor
        {
            FirstName = "Ali",
            LastName = "Veli",
            Title = "Dr.",
            University = university,
            Department = department,
        };

        var review = new Review
        {
            Professor = professor,
            User = owner,
            Year = 2024,
            QualityRating = 4,
            DifficultyRating = 3,
            WouldTakeAgain = true,
            AttendanceMandatory = false,
            Comment = "Normal bir yorum.",
            Status = ReviewStatus.Approved,
            TagsJson = "[]",
        };

        db.Add(owner);
        db.AddRange(reporters);
        db.Add(review);
        await db.SaveChangesAsync();

        return (db, service, professorService, review, owner, reporters);
    }

    [Fact]
    public async Task ReportAsync_OwnerCannotReport_ReturnsArgumentException()
    {
        var (_, service, _, review, owner, _) = await SeedApprovedReviewAsync();

        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => service.ReportAsync(review.Id, owner.Id));

        Assert.Contains("Kendi yorumunuzu", ex.Message);
    }

    [Fact]
    public async Task ReportAsync_DuplicateReport_ReturnsInvalidOperation()
    {
        var (_, service, _, review, _, reporters) = await SeedApprovedReviewAsync();

        await service.ReportAsync(review.Id, reporters[0].Id);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ReportAsync(review.Id, reporters[0].Id));

        Assert.Contains("zaten bildirdiniz", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ReportAsync_TwoReports_KeepsReviewApproved()
    {
        var (db, service, _, review, _, reporters) = await SeedApprovedReviewAsync();

        await service.ReportAsync(review.Id, reporters[0].Id);
        await service.ReportAsync(review.Id, reporters[1].Id);

        var updated = await db.Reviews.FindAsync(review.Id);
        Assert.Equal(ReviewStatus.Approved, updated!.Status);
    }

    [Fact]
    public async Task ReportAsync_ThreeReports_SetsReviewPendingWithNote()
    {
        var (db, service, professorService, review, _, reporters) = await SeedApprovedReviewAsync();

        await service.ReportAsync(review.Id, reporters[0].Id);
        await service.ReportAsync(review.Id, reporters[1].Id);
        await service.ReportAsync(review.Id, reporters[2].Id);

        var updated = await db.Reviews.FindAsync(review.Id);
        Assert.Equal(ReviewStatus.Pending, updated!.Status);
        Assert.Contains("3 kullanıcı tarafından bildirildi", updated.ModeratorNote);
        Assert.Single(professorService.RecalculatedProfessorIds);
    }

    [Fact]
    public async Task GetByProfessorAsync_PendingReview_IsNotListed()
    {
        var (db, service, _, review, _, reporters) = await SeedApprovedReviewAsync();

        foreach (var reporter in reporters)
            await service.ReportAsync(review.Id, reporter.Id);

        var professorId = (await db.Reviews.FindAsync(review.Id))!.ProfessorId;
        var result = await service.GetByProfessorAsync(professorId, page: 1, pageSize: 10);

        Assert.Empty(result.Items);
    }

    private sealed class NoOpModerationService : IContentModerationService
    {
        public ModerationResult Moderate(string text) =>
            new() { IsAllowed = true, RequiresManualReview = false };
    }
}
