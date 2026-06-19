namespace HocaPuan.Core.Moderation;

public class ModerationResult
{
    public bool IsAllowed { get; set; }
    public bool RequiresManualReview { get; set; }
    public string? RejectionReason { get; set; }
    public List<string> MatchedCategories { get; set; } = new();
    public List<string> ManualReviewReasons { get; set; } = new();
}
