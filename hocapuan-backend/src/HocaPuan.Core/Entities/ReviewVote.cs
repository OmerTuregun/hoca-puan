namespace HocaPuan.Core.Entities;

public class ReviewVote : BaseEntity
{
    public int ReviewId { get; set; }
    public Review Review { get; set; } = null!;

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public bool IsUpvote { get; set; }   // true = 👍, false = 👎
}
