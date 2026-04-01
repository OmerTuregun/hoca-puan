using HocaPuan.Core.DTOs.Common;
using HocaPuan.Core.DTOs.Review;

namespace HocaPuan.Core.Interfaces.Services;

public interface IReviewService
{
    Task<ReviewDto?> GetByIdAsync(int id);
    Task<PagedResultDto<ReviewDto>> GetByProfessorAsync(int professorId, int page, int pageSize);
    Task<ReviewDto> CreateAsync(int userId, CreateReviewDto dto);
    Task<bool> DeleteAsync(int reviewId, int requestingUserId, bool isAdmin);
    Task<VoteResultDto> VoteAsync(int reviewId, int userId, bool isUpvote);
    Task<ReviewDto> ModerateAsync(int reviewId, ModerateReviewDto dto);
    Task<PagedResultDto<ReviewDto>> GetPendingAsync(int page, int pageSize);
}
