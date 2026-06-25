using HocaPuan.Core.Constants;
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

public class ReviewServiceFreshnessVoteTests
{
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

    private record SeedResult(
        AppDbContext Db,
        ReviewService Service,
        Review Review,
        User Owner,
        User[] Voters
    );

    /// <summary>
    /// Seeds a review with configurable age.  Pass <paramref name="reviewAge"/> to
    /// control whether the review is old enough for freshness voting.
    /// </summary>
    private static async Task<SeedResult> SeedAsync(TimeSpan reviewAge, int extraVoterCount = 5)
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

        var owner = new User
        {
            Username = "yazar",
            Email = "yazar@edu.tr",
            PasswordHash = "hash",
            IsEmailVerified = true,
        };

        var voters = Enumerable.Range(1, extraVoterCount)
            .Select(i => new User
            {
                Username = $"voter{i}",
                Email = $"voter{i}@edu.tr",
                PasswordHash = "hash",
                IsEmailVerified = true,
            })
            .ToArray();

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
            Year = 2020,
            QualityRating = 4,
            DifficultyRating = 3,
            WouldTakeAgain = true,
            AttendanceMandatory = false,
            Comment = "Test yorumu.",
            Status = ReviewStatus.Approved,
            TagsJson = "[]",
            CreatedAt = DateTime.UtcNow - reviewAge,
        };

        db.Add(owner);
        db.AddRange(voters);
        db.Add(review);
        await db.SaveChangesAsync();

        return new SeedResult(db, service, review, owner, voters);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // 1. Reviews younger than 1 year must reject voting with ArgumentException
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task VoteFreshnessAsync_ReviewYoungerThan1Year_ThrowsArgumentException()
    {
        var seed = await SeedAsync(reviewAge: TimeSpan.FromDays(300)); // < 365 days

        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => seed.Service.VoteFreshnessAsync(seed.Review.Id, seed.Voters[0].Id, isStillValid: true));

        Assert.Contains("1 yıldan eski değil", ex.Message);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // 2. Owner cannot vote on their own review
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task VoteFreshnessAsync_OwnerVotesOnOwnReview_ThrowsArgumentException()
    {
        var seed = await SeedAsync(reviewAge: TimeSpan.FromDays(400));

        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => seed.Service.VoteFreshnessAsync(seed.Review.Id, seed.Owner.Id, isStillValid: true));

        Assert.Contains("Kendi yorumunuz", ex.Message);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // 3. Fewer than MinVotesForDisplay (3) votes → IsFlaggedAsOutdated is false
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task VoteFreshnessAsync_LessThan3Votes_IsFlaggedAsOutdatedFalse()
    {
        var seed = await SeedAsync(reviewAge: TimeSpan.FromDays(400), extraVoterCount: 2);

        // Both voters say "not valid"
        await seed.Service.VoteFreshnessAsync(seed.Review.Id, seed.Voters[0].Id, isStillValid: false);
        var result = await seed.Service.VoteFreshnessAsync(seed.Review.Id, seed.Voters[1].Id, isStillValid: false);

        // Only 2 votes — below MinVotesForDisplay=3, so flagging must not trigger
        Assert.False(result.IsFlaggedAsOutdated);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // 4. ≥3 votes with >50 % "not valid" → IsFlaggedAsOutdated becomes true
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task VoteFreshnessAsync_ThreeVotesMajorityNotValid_IsFlaggedAsOutdatedTrue()
    {
        var seed = await SeedAsync(reviewAge: TimeSpan.FromDays(400), extraVoterCount: 3);

        // 2 of 3 say "not valid" → 66.7 % → above 50 % threshold
        await seed.Service.VoteFreshnessAsync(seed.Review.Id, seed.Voters[0].Id, isStillValid: false);
        await seed.Service.VoteFreshnessAsync(seed.Review.Id, seed.Voters[1].Id, isStillValid: false);
        var result = await seed.Service.VoteFreshnessAsync(seed.Review.Id, seed.Voters[2].Id, isStillValid: true);

        Assert.True(result.IsFlaggedAsOutdated);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // 5. ≥3 votes but majority says "still valid" → IsFlaggedAsOutdated is false
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task VoteFreshnessAsync_ThreeVotesMajorityStillValid_IsFlaggedAsOutdatedFalse()
    {
        var seed = await SeedAsync(reviewAge: TimeSpan.FromDays(400), extraVoterCount: 3);

        // 2 of 3 say "still valid" → 33.3 % not valid → below 50 % threshold
        await seed.Service.VoteFreshnessAsync(seed.Review.Id, seed.Voters[0].Id, isStillValid: true);
        await seed.Service.VoteFreshnessAsync(seed.Review.Id, seed.Voters[1].Id, isStillValid: true);
        var result = await seed.Service.VoteFreshnessAsync(seed.Review.Id, seed.Voters[2].Id, isStillValid: false);

        Assert.False(result.IsFlaggedAsOutdated);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // 6. Upsert: same user voting again updates (does not duplicate) their vote
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task VoteFreshnessAsync_SameUserVotesTwice_UpdatesVoteInsteadOfDuplicating()
    {
        var seed = await SeedAsync(reviewAge: TimeSpan.FromDays(400), extraVoterCount: 3);

        // Voter0 first says "not valid", then changes to "still valid"
        await seed.Service.VoteFreshnessAsync(seed.Review.Id, seed.Voters[0].Id, isStillValid: false);
        await seed.Service.VoteFreshnessAsync(seed.Review.Id, seed.Voters[1].Id, isStillValid: false);
        await seed.Service.VoteFreshnessAsync(seed.Review.Id, seed.Voters[2].Id, isStillValid: false);

        // Now voter0 changes mind → should update, not add a 4th row
        var result = await seed.Service.VoteFreshnessAsync(seed.Review.Id, seed.Voters[0].Id, isStillValid: true);

        var totalVotes = await seed.Db.ReviewFreshnessVotes
            .Where(v => v.ReviewId == seed.Review.Id)
            .CountAsync();

        // Still exactly 3 rows in DB (upsert, not insert)
        Assert.Equal(3, totalVotes);

        // 2 "not valid" of 3 total → 66.7 % → still flagged
        Assert.True(result.IsFlaggedAsOutdated);

        // The stored vote for voter0 must now be "still valid"
        var storedVote = await seed.Db.ReviewFreshnessVotes
            .FirstAsync(v => v.ReviewId == seed.Review.Id && v.VoterUserId == seed.Voters[0].Id);
        Assert.True(storedVote.IsStillValid);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // 7. Result carries back the caller's vote value
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task VoteFreshnessAsync_SingleVote_ResultReflectsCallerVote()
    {
        var seed = await SeedAsync(reviewAge: TimeSpan.FromDays(400));

        var result = await seed.Service.VoteFreshnessAsync(seed.Review.Id, seed.Voters[0].Id, isStillValid: true);

        Assert.True(result.CurrentUserFreshnessVote);
        Assert.Equal("Oyunuz kaydedildi.", result.Message);
        // Single vote is below MinVotesForDisplay → percentage is null
        Assert.Null(result.FreshnessStillValidPercentage);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // 8. Exactly-at-threshold: 50 % not valid → NOT flagged (must be strictly >50 %)
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task VoteFreshnessAsync_ExactlyFiftyPercentNotValid_IsNotFlagged()
    {
        var seed = await SeedAsync(reviewAge: TimeSpan.FromDays(400), extraVoterCount: 4);

        // 2 valid, 2 not valid → exactly 50 % not valid → threshold is >50 %, so NOT flagged
        await seed.Service.VoteFreshnessAsync(seed.Review.Id, seed.Voters[0].Id, isStillValid: true);
        await seed.Service.VoteFreshnessAsync(seed.Review.Id, seed.Voters[1].Id, isStillValid: true);
        await seed.Service.VoteFreshnessAsync(seed.Review.Id, seed.Voters[2].Id, isStillValid: false);
        var result = await seed.Service.VoteFreshnessAsync(seed.Review.Id, seed.Voters[3].Id, isStillValid: false);

        Assert.False(result.IsFlaggedAsOutdated);
    }
}
