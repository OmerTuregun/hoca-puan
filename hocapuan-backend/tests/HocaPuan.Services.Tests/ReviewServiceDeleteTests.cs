using HocaPuan.Core.Entities;
using HocaPuan.Core.Interfaces.Services;
using HocaPuan.Core.Moderation;
using HocaPuan.Data;
using HocaPuan.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace HocaPuan.Services.Tests;

public class ReviewServiceDeleteTests
{
    private static async Task<(AppDbContext Db, ReviewService Service, Review Review, User Owner, User Other, User Admin)>
        CreateScenarioAsync()
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
            Username = "owner",
            Email = "owner@itu.edu.tr",
            PasswordHash = "hash",
            IsEmailVerified = true,
        };
        var other = new User
        {
            Username = "other",
            Email = "other@itu.edu.tr",
            PasswordHash = "hash",
            IsEmailVerified = true,
        };
        var admin = new User
        {
            Username = "admin",
            Email = "admin@itu.edu.tr",
            PasswordHash = "hash",
            IsEmailVerified = true,
            Role = UserRole.Admin,
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
            Comment = "Güzel bir ders, öğretim çok iyiydi.",
            Status = ReviewStatus.Approved,
            TagsJson = "[]",
        };

        db.AddRange(owner, other, admin);
        db.Add(review);
        await db.SaveChangesAsync();

        return (db, service, review, owner, other, admin);
    }

    [Fact]
    public async Task DeleteAsync_ByOwner_SoftDeletesReview()
    {
        var (_, service, review, owner, _, _) = await CreateScenarioAsync();

        var success = await service.DeleteAsync(review.Id, owner.Id, isAdmin: false);

        Assert.True(success);
    }

    [Fact]
    public async Task DeleteAsync_ByAdmin_NotOwner_SoftDeletesReview()
    {
        var (db, service, review, _, _, admin) = await CreateScenarioAsync();

        var success = await service.DeleteAsync(review.Id, admin.Id, isAdmin: true);

        Assert.True(success);
        var deleted = await db.Reviews.IgnoreQueryFilters().FirstAsync(r => r.Id == review.Id);
        Assert.True(deleted.IsDeleted);
    }

    [Fact]
    public async Task DeleteAsync_ByNonOwnerNonAdmin_ThrowsUnauthorized()
    {
        var (_, service, review, _, other, _) = await CreateScenarioAsync();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.DeleteAsync(review.Id, other.Id, isAdmin: false));
    }

    [Fact]
    public async Task DeleteAsync_UnknownReview_ReturnsFalse()
    {
        var (_, service, _, owner, _, _) = await CreateScenarioAsync();

        var success = await service.DeleteAsync(99999, owner.Id, isAdmin: false);

        Assert.False(success);
    }

    private sealed class NoOpProfessorService : IProfessorService
    {
        public Task<Core.DTOs.Professor.ProfessorDetailDto?> GetByIdAsync(int id) =>
            Task.FromResult<Core.DTOs.Professor.ProfessorDetailDto?>(null);
        public Task<Core.DTOs.Common.PagedResultDto<Core.DTOs.Professor.ProfessorListItemDto>> SearchAsync(
            Core.DTOs.Professor.ProfessorSearchDto searchDto) =>
            Task.FromResult(new Core.DTOs.Common.PagedResultDto<Core.DTOs.Professor.ProfessorListItemDto>());
        public Task<Core.DTOs.Professor.ProfessorDetailDto> CreateAsync(Core.DTOs.Professor.CreateProfessorDto dto) =>
            throw new NotImplementedException();
        public Task<Core.DTOs.Professor.ProfessorDetailDto> UpdateAsync(int id, Core.DTOs.Professor.UpdateProfessorDto dto) =>
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
