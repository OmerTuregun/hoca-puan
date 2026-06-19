namespace HocaPuan.API.Configuration;

public class RateLimitingSettings
{
    public const string SectionName = "RateLimiting";

    public RateLimitPolicySettings Auth { get; set; } = new();
    public RateLimitPolicySettings CommentWrite { get; set; } = new();
    public RateLimitPolicySettings Global { get; set; } = new();
}

public class RateLimitPolicySettings
{
    public int PermitLimit { get; set; }
    public int WindowMinutes { get; set; }
}
