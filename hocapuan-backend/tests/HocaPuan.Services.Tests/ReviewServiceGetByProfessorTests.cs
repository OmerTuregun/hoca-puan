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

public class ReviewServiceGetByProfessorTests
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

    private static async Task SeedReviewGraphAsync(AppDbContext db, int voterUserId, bool isUpvote)
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

        var author = new User
        {
            Username = "yazar",
            Email = "yazar@itu.edu.tr",
            PasswordHash = "hash",
            IsEmailVerified = true,
        };
        var voter = new User
        {
            Username = "oyveren",
            Email = "oyveren@itu.edu.tr",
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

        var review = new Review
        {
            Professor = professor,
            User = author,
            Year = 2024,
            QualityRating = 4,
            DifficultyRating = 3,
            WouldTakeAgain = true,
            AttendanceMandatory = false,
            Comment = "Güzel bir ders, öğretim çok iyiydi.",
            Status = ReviewStatus.Approved,
            TagsJson = "[]",
        };

        db.AddRange(author, voter);
        db.Add(review);
        await db.SaveChangesAsync();

        db.ReviewVotes.Add(new ReviewVote
        {
            ReviewId = review.Id,
            UserId = voterUserId == 0 ? voter.Id : voterUserId,
            IsUpvote = isUpvote,
        });
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task GetByProfessorAsync_WithCurrentUserId_SetsCurrentUserVote()
    {
        var (db, service) = CreateService();
        await SeedReviewGraphAsync(db, voterUserId: 0, isUpvote: true);

        var voter = await db.Users.SingleAsync(u => u.Username == "oyveren");
        var professor = await db.Professors.SingleAsync();

        var result = await service.GetByProfessorAsync(professor.Id, page: 1, pageSize: 10, currentUserId: voter.Id);

        Assert.Single(result.Items);
        Assert.True(result.Items[0].CurrentUserVote);
    }

    [Fact]
    public async Task GetByProfessorAsync_WithNullCurrentUserId_LeavesCurrentUserVoteNull()
    {
        var (db, service) = CreateService();
        await SeedReviewGraphAsync(db, voterUserId: 0, isUpvote: false);

        var professor = await db.Professors.SingleAsync();

        var result = await service.GetByProfessorAsync(professor.Id, page: 1, pageSize: 10, currentUserId: null);

        Assert.Single(result.Items);
        Assert.Null(result.Items[0].CurrentUserVote);
    }

    [Fact]
    public async Task GetByProfessorAsync_WhenUserDidNotVote_CurrentUserVoteIsNull()
    {
        var (db, service) = CreateService();
        await SeedReviewGraphAsync(db, voterUserId: 0, isUpvote: true);

        var professor = await db.Professors.SingleAsync();
        var stranger = new User
        {
            Username = "baska",
            Email = "baska@itu.edu.tr",
            PasswordHash = "hash",
            IsEmailVerified = true,
        };
        db.Users.Add(stranger);
        await db.SaveChangesAsync();

        var result = await service.GetByProfessorAsync(professor.Id, page: 1, pageSize: 10, currentUserId: stranger.Id);

        Assert.Single(result.Items);
        Assert.Null(result.Items[0].CurrentUserVote);
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
