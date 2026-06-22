using System.Text.Json;
using HocaPuan.Core.DTOs.Common;
using HocaPuan.Core.DTOs.Review;
using HocaPuan.Core.Entities;
using HocaPuan.Core.Interfaces.Services;
using HocaPuan.Core.Moderation;
using HocaPuan.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HocaPuan.Services;

public class ReviewService : IReviewService
{
    public const int ReportsRequiredForAutoPending = 3;

    private const string PendingReasonPrefix = "İnceleme nedeni: ";
    private const string PendingInfoMessage =
        "Yorumunuz incelemeye alındı, onaylandığında yayınlanacaktır.";

    private readonly AppDbContext _db;
    private readonly IProfessorService _professorService;
    private readonly IContentModerationService _moderation;
    private readonly ILogger<ReviewService> _logger;

    public ReviewService(
        AppDbContext db,
        IProfessorService professorService,
        IContentModerationService moderation,
        ILogger<ReviewService> logger)
    {
        _db = db;
        _professorService = professorService;
        _moderation = moderation;
        _logger = logger;
    }

    public async Task<ReviewDto?> GetByIdAsync(int id)
    {
        var review = await _db.Reviews
            .Include(r => r.User)
            .Include(r => r.Professor).ThenInclude(p => p.University)
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

        return review == null ? null : MapDto(review, null);
    }

    public async Task<PagedResultDto<ReviewDto>> GetByProfessorAsync(
        int professorId,
        int page,
        int pageSize,
        int? currentUserId = null,
        string sortBy = "newest",
        string? tag = null)
    {
        var query = _db.Reviews
            .Include(r => r.User)
            .Include(r => r.Professor).ThenInclude(p => p.University)
            .Where(r => r.ProfessorId == professorId && r.Status == ReviewStatus.Approved && !r.IsDeleted);

        if (!string.IsNullOrWhiteSpace(tag))
        {
            var tagJson = JsonSerializer.Serialize(tag.Trim());
            query = query.Where(r => r.TagsJson.Contains(tagJson));
        }

        if (currentUserId.HasValue)
        {
            query = query.Include(r => r.Votes.Where(v => v.UserId == currentUserId.Value));
        }

        var totalCount = await query.CountAsync();
        var ordered = ApplyProfessorReviewSort(query, sortBy);
        var items = await ordered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResultDto<ReviewDto>
        {
            Items = items.Select(r => MapDto(r, currentUserId)).ToList(),
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

        var moderation = EvaluateComment(dto.Comment, userId);
        var status = moderation.RequiresManualReview ? ReviewStatus.Pending : ReviewStatus.Approved;

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
            Status = status,
            ModeratorNote = moderation.RequiresManualReview
                ? FormatPendingReasons(moderation.ManualReviewReasons)
                : null
        };

        _db.Reviews.Add(review);
        await _db.SaveChangesAsync();

        if (status == ReviewStatus.Approved)
            await _professorService.RecalculateStatsAsync(dto.ProfessorId);

        var result = (await GetByIdAsync(review.Id))!;
        if (status == ReviewStatus.Pending)
            result.InfoMessage = PendingInfoMessage;

        return result;
    }

    public async Task<ReviewDto?> UpdateAsync(int reviewId, int userId, UpdateReviewDto dto)
    {
        var review = await _db.Reviews.FirstOrDefaultAsync(r => r.Id == reviewId && !r.IsDeleted);
        if (review == null) return null;

        if (review.UserId != userId)
            throw new UnauthorizedAccessException("Bu yorumu düzenleme yetkiniz yok.");

        if (dto.QualityRating < 1 || dto.QualityRating > 5)
            throw new ArgumentException("Kalite puanı 1-5 arasında olmalıdır.");

        if (dto.DifficultyRating < 1 || dto.DifficultyRating > 5)
            throw new ArgumentException("Zorluk puanı 1-5 arasında olmalıdır.");

        var moderation = EvaluateComment(dto.Comment, userId);
        var previousStatus = review.Status;
        var newStatus = moderation.RequiresManualReview ? ReviewStatus.Pending : ReviewStatus.Approved;

        review.CourseCode = dto.CourseCode;
        review.Grade = dto.Grade;
        review.Year = dto.Year;
        review.QualityRating = dto.QualityRating;
        review.DifficultyRating = dto.DifficultyRating;
        review.WouldTakeAgain = dto.WouldTakeAgain;
        review.AttendanceMandatory = dto.AttendanceMandatory;
        review.Comment = dto.Comment;
        review.TagsJson = JsonSerializer.Serialize(dto.Tags);
        review.Status = newStatus;
        review.ModeratorNote = moderation.RequiresManualReview
            ? FormatPendingReasons(moderation.ManualReviewReasons)
            : null;
        review.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        if (previousStatus == ReviewStatus.Approved || newStatus == ReviewStatus.Approved)
            await _professorService.RecalculateStatsAsync(review.ProfessorId);

        var result = await GetByIdAsync(reviewId);
        if (result != null && newStatus == ReviewStatus.Pending)
            result.InfoMessage = PendingInfoMessage;

        return result;
    }

    public async Task<PagedResultDto<ReviewDto>> GetByUserAsync(int userId, int page, int pageSize)
    {
        var query = _db.Reviews
            .Include(r => r.User)
            .Include(r => r.Professor).ThenInclude(p => p.University)
            .Where(r => r.UserId == userId && !r.IsDeleted)
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

    public async Task<bool> DeleteAsync(int reviewId, int requestingUserId, bool isAdmin)
    {
        var review = await _db.Reviews.FindAsync(reviewId);
        if (review == null) return false;

        if (!isAdmin && review.UserId != requestingUserId)
            throw new UnauthorizedAccessException("Bu yorumu silme yetkiniz yok.");

        if (isAdmin && review.UserId != requestingUserId)
        {
            _logger.LogWarning(
                "Admin review deletion: AdminUserId={AdminUserId} deleted ReviewId={ReviewId} owned by UserId={OwnerUserId}",
                requestingUserId,
                reviewId,
                review.UserId);
        }

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
        review.ModeratorNote = dto.Approve ? null : dto.ModeratorNote;
        review.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        await _professorService.RecalculateStatsAsync(review.ProfessorId);

        return (await GetByIdAsync(reviewId))!;
    }

    public async Task<PagedResultDto<ReviewDto>> GetPendingAsync(int page, int pageSize)
    {
        var query = _db.Reviews
            .Include(r => r.User)
            .Include(r => r.Professor).ThenInclude(p => p.University)
            .Where(r => r.Status == ReviewStatus.Pending && !r.IsDeleted)
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

    public async Task<ReportReviewResultDto> ReportAsync(int reviewId, int reporterUserId)
    {
        var review = await _db.Reviews.FirstOrDefaultAsync(r => r.Id == reviewId && !r.IsDeleted)
            ?? throw new KeyNotFoundException("Yorum bulunamadı.");

        if (review.UserId == reporterUserId)
            throw new ArgumentException("Kendi yorumunuzu bildiremezsiniz.");

        if (review.Status != ReviewStatus.Approved)
            throw new ArgumentException("Sadece yayınlanmış yorumlar bildirilebilir.");

        var alreadyReported = await _db.ReviewReports.AnyAsync(r =>
            r.ReviewId == reviewId && r.ReporterUserId == reporterUserId);

        if (alreadyReported)
            throw new InvalidOperationException("Bu yorumu zaten bildirdiniz.");

        try
        {
            _db.ReviewReports.Add(new ReviewReport
            {
                ReviewId = reviewId,
                ReporterUserId = reporterUserId,
            });
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (IsDuplicateReport(ex))
        {
            throw new InvalidOperationException("Bu yorumu zaten bildirdiniz.");
        }

        var reportCount = await _db.ReviewReports.CountAsync(r => r.ReviewId == reviewId);

        if (reportCount >= ReportsRequiredForAutoPending && review.Status == ReviewStatus.Approved)
        {
            review.Status = ReviewStatus.Pending;
            review.ModeratorNote = FormatPendingReasons(
                [$"{reportCount} kullanıcı tarafından bildirildi"]);
            review.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            await _professorService.RecalculateStatsAsync(review.ProfessorId);
        }

        return new ReportReviewResultDto
        {
            Message = "Bildiriminiz alındı.",
            ReportCount = reportCount,
        };
    }

    private static IQueryable<Review> ApplyProfessorReviewSort(IQueryable<Review> query, string sortBy) =>
        sortBy.Trim().ToLowerInvariant() switch
        {
            "oldest" => query.OrderBy(r => r.CreatedAt),
            "mosthelpful" => query.OrderByDescending(r => r.ThumbsUp - r.ThumbsDown).ThenByDescending(r => r.CreatedAt),
            "highestrating" => query.OrderByDescending(r => r.QualityRating).ThenByDescending(r => r.CreatedAt),
            "lowestrating" => query.OrderBy(r => r.QualityRating).ThenByDescending(r => r.CreatedAt),
            _ => query.OrderByDescending(r => r.CreatedAt),
        };

    // ────────────────────────────────────────────────────────────
    private ModerationResult EvaluateComment(string comment, int userId)
    {
        var result = _moderation.Moderate(comment);
        if (result.IsAllowed) return result;

        _logger.LogWarning(
            "Yorum moderasyonu reddetti. UserId={UserId}, Categories={Categories}",
            userId,
            string.Join(", ", result.MatchedCategories));

        throw new ArgumentException(result.RejectionReason ?? "Yorumunuz reddedildi.");
    }

    private static string FormatPendingReasons(IEnumerable<string> reasons) =>
        PendingReasonPrefix + string.Join("; ", reasons);

    private static List<string> ParsePendingReasons(string? moderatorNote)
    {
        if (string.IsNullOrWhiteSpace(moderatorNote) ||
            !moderatorNote.StartsWith(PendingReasonPrefix, StringComparison.Ordinal))
        {
            return [];
        }

        return moderatorNote[PendingReasonPrefix.Length..]
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
    }

    private static bool IsDuplicateReport(DbUpdateException ex)
    {
        var message = ex.InnerException?.Message ?? ex.Message;
        return message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase) ||
               message.Contains("unique constraint", StringComparison.OrdinalIgnoreCase) ||
               message.Contains("duplicate", StringComparison.OrdinalIgnoreCase);
    }

    private static ReviewDto MapDto(Review r, int? currentUserId)
    {
        bool? currentUserVote = null;
        if (currentUserId.HasValue)
        {
            var vote = r.Votes.FirstOrDefault(v => v.UserId == currentUserId.Value);
            currentUserVote = vote?.IsUpvote;
        }

        return new ReviewDto
        {
            Id = r.Id,
        UserId = r.UserId,
        ProfessorId = r.ProfessorId,
        ProfessorFullName = r.Professor?.FullName ?? "",
        UniversityName = r.Professor?.University?.Name ?? "",
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
        ManualReviewReasons = r.Status == ReviewStatus.Pending
            ? ParsePendingReasons(r.ModeratorNote)
            : [],
        ThumbsUp = r.ThumbsUp,
        ThumbsDown = r.ThumbsDown,
        CurrentUserVote = currentUserVote,
        CreatedAt = r.CreatedAt
        };
    }
}
