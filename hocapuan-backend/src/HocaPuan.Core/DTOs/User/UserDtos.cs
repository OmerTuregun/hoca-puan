using HocaPuan.Core.DTOs.Common;
using HocaPuan.Core.DTOs.Review;

namespace HocaPuan.Core.DTOs.User;

public class ContributionHistoryDto
{
    public int TotalReviews { get; set; }
    public int TotalHelpfulVotes { get; set; }
    public PagedResultDto<ReviewDto> Reviews { get; set; } = new();
}
