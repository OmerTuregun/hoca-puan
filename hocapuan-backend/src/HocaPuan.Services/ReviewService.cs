using System.Text.Json;
using HocaPuan.Core.DTOs.Common;
using HocaPuan.Core.DTOs.Review;
using HocaPuan.Core.Entities;
using HocaPuan.Core.Interfaces.Services;
using HocaPuan.Data;
using Microsoft.EntityFrameworkCore;

namespace HocaPuan.Services;

public class ReviewService : IReviewService
{
    private readonly AppDbContext _db;
    private readonly IProfessorService _professorService;

    public ReviewService(AppDbContext db, IProfessorService professorService)
    {
        _db = db;
        _professorService = professorService;
    }

    public async Task<ReviewDto?> GetByIdAsync(int id)
    {
        var review = await _db.Reviews
            .Include(r => r.User)
            .Include(r => r.Professor)
            .FirstOrDefaultAsync(r => r.Id == id);

        return review == null ? null : MapDto(review, null);
    }

    public async Task<PagedResultDto<ReviewDto>> GetByProfessorAsync(int professorId, int page, int pageSize)
    {
        var query = _db.Reviews
            .Include(r => r.User)
            .Include(r => r.Professor)
            .Where(r => r.ProfessorId == professorId && r.Status == ReviewStatus.Approved)
            .OrderByDescending(r => r.CreatedAt);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResultDto<ReviewDto>
        {
            Items = items.Select(r => MapDto(r, null)).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ReviewDto> CreateAsync(int userId, CreateReviewDto dto)
    {
        // Aynı hoca için zaten bekleyen/onaylı yorum var mı?
        var existingPending = await _db.Reviews.AnyAsync(r =>
            r.ProfessorId == dto.ProfessorId &&
            r.UserId == userId &&
            r.Status != ReviewStatus.Rejected &&
            !r.IsDeleted);

        if (existingPending)
            throw new InvalidOperationException("Bu hoca için zaten aktif bir yorumunuz var.");

        if (dto.QualityRating < 1 || dto.QualityRating > 5)
            throw new ArgumentException("Kalite puanı 1-5 arasında olmalıdır.");

        if (dto.DifficultyRating < 1 || dto.DifficultyRating > 5)
            throw new ArgumentException("Zorluk puanı 1-5 arasında olmalıdır.");

        var review = new Review
        {
            ProfessorId = dto.ProfessorId,
            UserId = userId,
            CourseCode = dto.CourseCode,
            Grade = dto.Grade,
            Year = dto.Year,
            QualityRating = dto.QualityRating,
            DifficultyRating = dto.DifficultyRating,
            WouldTakeAgain = dto.WouldTakeAgain,
            AttendanceMandatory = dto.AttendanceMandatory,
            Comment = dto.Comment,
            TagsJson = JsonSerializer.Serialize(dto.Tags),
            Status = ReviewStatus.Approved  // TODO: moderasyon için Pending yapın
        };

        _db.Reviews.Add(review);
        await _db.SaveChangesAsync();

        // Hoca istatistiklerini güncelle
        await _professorService.RecalculateStatsAsync(dto.ProfessorId);

        return (await GetByIdAsync(review.Id))!;
    }

    public async Task<bool> DeleteAsync(int reviewId, int requestingUserId, bool isAdmin)
    {
        var review = await _db.Reviews.FindAsync(reviewId);
        if (review == null) return false;

        if (!isAdmin && review.UserId != requestingUserId)
            throw new UnauthorizedAccessException("Bu yorumu silme yetkiniz yok.");

        review.IsDeleted = true;
        review.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        await _professorService.RecalculateStatsAsync(review.ProfessorId);
        return true;
    }

    public async Task<VoteResultDto> VoteAsync(int reviewId, int userId, bool isUpvote)
    {
        var review = await _db.Reviews.FindAsync(reviewId)
            ?? throw new KeyNotFoundException("Yorum bulunamadı.");

        var existingVote = await _db.ReviewVotes
            .FirstOrDefaultAsync(v => v.ReviewId == reviewId && v.UserId == userId);

        if (existingVote != null)
        {
            if (existingVote.IsUpvote == isUpvote)
            {
                // Aynı oy → geri al
                _db.ReviewVotes.Remove(existingVote);
                if (isUpvote) review.ThumbsUp--;
                else review.ThumbsDown--;
            }
            else
            {
                // Oy değiştir
                existingVote.IsUpvote = isUpvote;
                if (isUpvote) { review.ThumbsUp++; review.ThumbsDown--; }
                else { review.ThumbsDown++; review.ThumbsUp--; }
            }
        }
        else
        {
            _db.ReviewVotes.Add(new ReviewVote { ReviewId = reviewId, UserId = userId, IsUpvote = isUpvote });
            if (isUpvote) review.ThumbsUp++;
            else review.ThumbsDown++;
        }

        await _db.SaveChangesAsync();

        var userVote = await _db.ReviewVotes.FirstOrDefaultAsync(v => v.ReviewId == reviewId && v.UserId == userId);
        return new VoteResultDto
        {
            ThumbsUp = review.ThumbsUp,
            ThumbsDown = review.ThumbsDown,
            UserVote = userVote?.IsUpvote
        };
    }

    public async Task<ReviewDto> ModerateAsync(int reviewId, ModerateReviewDto dto)
    {
        var review = await _db.Reviews.FindAsync(reviewId)
            ?? throw new KeyNotFoundException("Yorum bulunamadı.");

        review.Status = dto.Approve ? ReviewStatus.Approved : ReviewStatus.Rejected;
        review.ModeratorNote = dto.ModeratorNote;
        review.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        await _professorService.RecalculateStatsAsync(review.ProfessorId);

        return (await GetByIdAsync(reviewId))!;
    }

    public async Task<PagedResultDto<ReviewDto>> GetPendingAsync(int page, int pageSize)
    {
        var query = _db.Reviews
            .Include(r => r.User)
            .Include(r => r.Professor)
            .Where(r => r.Status == ReviewStatus.Pending)
            .OrderBy(r => r.CreatedAt);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResultDto<ReviewDto>
        {
            Items = items.Select(r => MapDto(r, null)).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    // ────────────────────────────────────────────────────────────
    private static ReviewDto MapDto(Review r, bool? currentUserVote) => new()
    {
        Id = r.Id,
        ProfessorId = r.ProfessorId,
        ProfessorFullName = r.Professor?.FullName ?? "",
        Username = r.User?.Username ?? "Anonim",
        CourseCode = r.CourseCode,
        Grade = r.Grade,
        Year = r.Year,
        QualityRating = r.QualityRating,
        DifficultyRating = r.DifficultyRating,
        WouldTakeAgain = r.WouldTakeAgain,
        AttendanceMandatory = r.AttendanceMandatory,
        Comment = r.Comment,
        Tags = JsonSerializer.Deserialize<List<string>>(r.TagsJson) ?? new(),
        Status = r.Status.ToString(),
        ThumbsUp = r.ThumbsUp,
        ThumbsDown = r.ThumbsDown,
        CurrentUserVote = currentUserVote,
        CreatedAt = r.CreatedAt
    };
}
