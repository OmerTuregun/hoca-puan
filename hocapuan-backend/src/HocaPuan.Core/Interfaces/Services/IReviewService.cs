using HocaPuan.Core.DTOs.Common;
using HocaPuan.Core.DTOs.Review;
using HocaPuan.Core.DTOs.User;

namespace HocaPuan.Core.Interfaces.Services;

public interface IReviewService
{
    Task<ReviewDto?> GetByIdAsync(int id);
    Task<ContributionHistoryDto> GetContributionHistoryAsync(int userId, int page, int pageSize);
    Task<PagedResultDto<ReviewDto>> GetByProfessorAsync(
        int professorId,
        int page,
        int pageSize,
        int? currentUserId = null,
        string sortBy = "newest",
        string? tag = null);
    Task<ReviewDto> CreateAsync(int userId, CreateReviewDto dto);
    Task<ReviewDto?> UpdateAsync(int reviewId, int userId, UpdateReviewDto dto);
    Task<PagedResultDto<ReviewDto>> GetByUserAsync(int userId, int page, int pageSize);
    Task<bool> DeleteAsync(int reviewId, int requestingUserId, bool isAdmin);
    Task<VoteResultDto> VoteAsync(int reviewId, int userId, bool isUpvote);
    Task<ReviewDto> ModerateAsync(int reviewId, ModerateReviewDto dto);
    Task<PagedResultDto<ReviewDto>> GetPendingAsync(int page, int pageSize);
    Task<ReportReviewResultDto> ReportAsync(int reviewId, int reporterUserId);
    Task<FreshnessVoteResultDto> VoteFreshnessAsync(int reviewId, int voterUserId, bool isStillValid);
}
