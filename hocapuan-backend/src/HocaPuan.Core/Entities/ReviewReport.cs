namespace HocaPuan.Core.Entities;

public class ReviewReport : BaseEntity
{
    public int ReviewId { get; set; }
    public Review Review { get; set; } = null!;

    public int ReporterUserId { get; set; }
    public User Reporter { get; set; } = null!;
}
