using System.Security.Claims;
using System.Text.Json;
using System.Threading.RateLimiting;
using HocaPuan.API.Configuration;
using HocaPuan.API.Http;
using Microsoft.AspNetCore.RateLimiting;

namespace HocaPuan.API.Extensions;

public static class RateLimitingExtensions
{
    public const string AuthPolicy = "auth";
    public const string CommentWritePolicy = "comment-write";
    public const string VotePolicy = "vote";
    public const string ReportWritePolicy = "report-write";
    public const string GlobalPolicy = "global";

    public static IServiceCollection AddHocaPuanRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<RateLimitingSettings>()
            .Bind(configuration.GetSection(RateLimitingSettings.SectionName));

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = (context, _) => OnRejectedAsync(context);

            options.AddPolicy(AuthPolicy, httpContext =>
            {
                var settings = ResolveSettings(httpContext);
                var ip = ClientIpResolver.Resolve(httpContext);
                return RateLimitPartition.GetFixedWindowLimiter(
                    $"auth:{ip}",
                    _ => CreateWindowOptions(settings.Auth));
            });

            options.AddPolicy(CommentWritePolicy, httpContext =>
            {
                var settings = ResolveSettings(httpContext);
                var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                var partition = string.IsNullOrEmpty(userId)
                    ? $"comment-write:anon:{ClientIpResolver.Resolve(httpContext)}"
                    : $"comment-write:user:{userId}";

                return RateLimitPartition.GetFixedWindowLimiter(
                    partition,
                    _ => CreateWindowOptions(settings.CommentWrite));
            });

            options.AddPolicy(VotePolicy, httpContext =>
            {
                var settings = ResolveSettings(httpContext);
                var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                var partition = string.IsNullOrEmpty(userId)
                    ? $"vote:anon:{ClientIpResolver.Resolve(httpContext)}"
                    : $"vote:user:{userId}";

                return RateLimitPartition.GetFixedWindowLimiter(
                    partition,
                    _ => CreateWindowOptions(settings.Vote));
            });

            options.AddPolicy(ReportWritePolicy, httpContext =>
            {
                var settings = ResolveSettings(httpContext);
                var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                var partition = string.IsNullOrEmpty(userId)
                    ? $"report-write:anon:{ClientIpResolver.Resolve(httpContext)}"
                    : $"report-write:user:{userId}";

                return RateLimitPartition.GetFixedWindowLimiter(
                    partition,
                    _ => CreateWindowOptions(settings.ReportWrite));
            });

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            {
                var settings = ResolveSettings(httpContext);
                var ip = ClientIpResolver.Resolve(httpContext);
                return RateLimitPartition.GetFixedWindowLimiter(
                    $"global:{ip}",
                    _ => CreateWindowOptions(settings.Global));
            });
        });

        return services;
    }

    private static RateLimitingSettings ResolveSettings(HttpContext httpContext) =>
        httpContext.RequestServices
            .GetRequiredService<IConfiguration>()
            .GetSection(RateLimitingSettings.SectionName)
            .Get<RateLimitingSettings>() ?? new RateLimitingSettings();

    private static FixedWindowRateLimiterOptions CreateWindowOptions(RateLimitPolicySettings policy) =>
        new()
        {
            PermitLimit = policy.PermitLimit,
            Window = TimeSpan.FromMinutes(policy.WindowMinutes),
            QueueLimit = 0,
            AutoReplenishment = true,
        };

    private static async ValueTask OnRejectedAsync(OnRejectedContext context)
    {
        var httpContext = context.HttpContext;
        var policyName = ResolvePolicyName(httpContext);
        var partitionKey = ResolvePartitionKey(httpContext, policyName);
        var maskedPartition = policyName is CommentWritePolicy or VotePolicy or ReportWritePolicy && partitionKey.StartsWith("user:")
            ? partitionKey
            : ClientIpResolver.MaskForLog(partitionKey);

        var logger = httpContext.RequestServices
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("RateLimiting");

        logger.LogWarning(
            "Rate limit aşıldı. Policy={Policy} Partition={Partition} Path={Path}",
            policyName,
            maskedPartition,
            httpContext.Request.Path);

        var message = policyName switch
        {
            AuthPolicy => "Çok fazla deneme yaptınız, lütfen birkaç dakika sonra tekrar deneyin.",
            CommentWritePolicy => "Çok hızlı yorum gönderiyorsunuz, lütfen biraz bekleyin.",
            VotePolicy => "Çok hızlı oy veriyorsunuz, lütfen biraz bekleyin.",
            ReportWritePolicy => "Çok hızlı şikayet gönderiyorsunuz, lütfen biraz bekleyin.",
            _ => "Çok fazla istek gönderdiniz, lütfen kısa bir süre sonra tekrar deneyin.",
        };

        if (httpContext.Response.HasStarted)
            return;

        httpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        httpContext.Response.ContentType = "application/json";

        var body = new
        {
            status = StatusCodes.Status429TooManyRequests,
            message,
            timestamp = DateTime.UtcNow,
        };

        await httpContext.Response.WriteAsync(JsonSerializer.Serialize(body));
    }

    private static string ResolvePolicyName(HttpContext httpContext)
    {
        var endpoint = httpContext.GetEndpoint();
        var attr = endpoint?.Metadata.GetOrderedMetadata<EnableRateLimitingAttribute>().FirstOrDefault();
        return attr?.PolicyName ?? GlobalPolicy;
    }

    private static string ResolvePartitionKey(HttpContext httpContext, string policyName)
    {
        if (policyName is CommentWritePolicy or VotePolicy or ReportWritePolicy)
        {
            var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
                return $"user:{userId}";
        }

        return ClientIpResolver.Resolve(httpContext);
    }
}
