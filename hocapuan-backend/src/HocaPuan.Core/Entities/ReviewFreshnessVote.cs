namespace HocaPuan.Core.Entities;

public class ReviewFreshnessVote : BaseEntity
{
    public int ReviewId { get; set; }
    public Review Review { get; set; } = null!;

    public int VoterUserId { get; set; }
    public User Voter { get; set; } = null!;

    /// <summary>true = kullanıcı bilginin hâlâ geçerli olduğunu düşünüyor</summary>
    public bool IsStillValid { get; set; }
}
