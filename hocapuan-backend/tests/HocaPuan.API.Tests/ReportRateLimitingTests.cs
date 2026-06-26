using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using HocaPuan.Core.DTOs.Auth;
using HocaPuan.Core.DTOs.Common;
using HocaPuan.Core.DTOs.Review;
using HocaPuan.Core.DTOs.User;
using HocaPuan.Core.Interfaces.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace HocaPuan.API.Tests;

public class ReportRateLimitingTests : IClassFixture<ReportRateLimitWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ReportRateLimitingTests(ReportRateLimitWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = true,
        });
    }

    [Fact]
    public async Task ReportEndpoint_ExceedsLimit_Returns429WithTurkishMessage()
    {
        var csrf = await FetchCsrfTokenAsync();
        await LoginAsync(csrf);
        HttpResponseMessage? lastResponse = null;

        for (var i = 0; i < 6; i++)
        {
            csrf = await FetchCsrfTokenAsync();
            lastResponse = await PostReportAsync(1, csrf);
            if (lastResponse.StatusCode == HttpStatusCode.TooManyRequests)
                break;
            Assert.Equal(HttpStatusCode.OK, lastResponse.StatusCode);
        }

        Assert.NotNull(lastResponse);
        Assert.Equal(HttpStatusCode.TooManyRequests, lastResponse.StatusCode);

        var body = await lastResponse.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.NotNull(body);
        Assert.Equal(429, body.Status);
        Assert.Contains("Çok hızlı şikayet", body.Message, StringComparison.Ordinal);
    }

    private async Task LoginAsync(string csrfToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login")
        {
            Content = JsonContent.Create(new LoginDto
            {
                Email = "report-rate-test@itu.edu.tr",
                Password = "password",
            }),
        };
        request.Headers.Add("X-CSRF-TOKEN", csrfToken);
        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    private async Task<string> FetchCsrfTokenAsync()
    {
        var response = await _client.GetAsync("/api/auth/csrf-token");
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<CsrfResponse>();
        return payload!.Token;
    }

    private Task<HttpResponseMessage> PostReportAsync(int reviewId, string csrfToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/reviews/{reviewId}/report");
        request.Headers.Add("X-CSRF-TOKEN", csrfToken);
        return _client.SendAsync(request);
    }

    private sealed class CsrfResponse
    {
        public string Token { get; set; } = "";
    }

    private sealed class ApiErrorResponse
    {
        public int Status { get; set; }
        public string Message { get; set; } = "";
    }
}

public class ReportRateLimitWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.Testing.json"), optional: false);
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IAuthService>();
            services.RemoveAll<IReviewService>();
            services.AddScoped<IAuthService, ReportRateLimitAuthService>();
            services.AddScoped<IReviewService, FakeReportReviewService>();
        });
    }
}

internal sealed class ReportRateLimitAuthService : IAuthService
{
    private const string TestEmail = "report-rate-test@itu.edu.tr";
    private static readonly string TestJwt = CreateTestJwt();

    public Task<AuthResponseDto> RegisterAsync(RegisterDto dto) =>
        Task.FromResult(new AuthResponseDto { Success = false, Message = "test" });

    public Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        if (dto.Email == TestEmail)
        {
            return Task.FromResult(new AuthResponseDto
            {
                Success = true,
                Token = TestJwt,
                User = new UserInfoDto
                {
                    Id = 42,
                    Username = "reportuser",
                    Email = dto.Email,
                    Role = "Student",
                    IsEmailVerified = true,
                },
            });
        }

        return Task.FromResult(new AuthResponseDto { Success = false, Message = "test" });
    }

    public Task<bool> VerifyEmailAsync(string token) => Task.FromResult(false);

    public Task<ForgotPasswordResponseDto> ForgotPasswordAsync(string email) =>
        Task.FromResult(new ForgotPasswordResponseDto { Message = "test" });

    public Task<bool> ResetPasswordAsync(ResetPasswordDto dto) => Task.FromResult(false);

    public Task<UserProfileDto?> GetProfileAsync(int userId) => Task.FromResult<UserProfileDto?>(null);

    private static string CreateTestJwt()
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("integration-test-secret-key-min-32-chars!!"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: "HocaPuanAPI",
            audience: "HocaPuanClient",
            claims: new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "42"),
                new Claim(ClaimTypes.Email, TestEmail),
            },
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

internal sealed class FakeReportReviewService : IReviewService
{
    public Task<ReportReviewResultDto> ReportAsync(int reviewId, int reporterUserId) =>
        Task.FromResult(new ReportReviewResultDto
        {
            ReportCount = 1,
            Message = "Bildiriminiz alındı.",
        });

    public Task<ReviewDto?> GetByIdAsync(int id) => throw new NotImplementedException();
    public Task<ContributionHistoryDto> GetContributionHistoryAsync(int userId, int page, int pageSize) =>
        throw new NotImplementedException();
    public Task<PagedResultDto<ReviewDto>> GetByProfessorAsync(int professorId, int page, int pageSize, int? currentUserId = null, string sortBy = "newest", string? tag = null) =>
        throw new NotImplementedException();
    public Task<ReviewDto> CreateAsync(int userId, CreateReviewDto dto) => throw new NotImplementedException();
    public Task<ReviewDto?> UpdateAsync(int reviewId, int userId, UpdateReviewDto dto) => throw new NotImplementedException();
    public Task<PagedResultDto<ReviewDto>> GetByUserAsync(int userId, int page, int pageSize) =>
        throw new NotImplementedException();
    public Task<bool> DeleteAsync(int reviewId, int requestingUserId, bool isAdmin) => throw new NotImplementedException();
    public Task<VoteResultDto> VoteAsync(int reviewId, int userId, bool isUpvote) => throw new NotImplementedException();
    public Task<ReviewDto> ModerateAsync(int reviewId, ModerateReviewDto dto) => throw new NotImplementedException();
    public Task<PagedResultDto<ReviewDto>> GetPendingAsync(int page, int pageSize) => throw new NotImplementedException();
    public Task<FreshnessVoteResultDto> VoteFreshnessAsync(int reviewId, int voterUserId, bool isStillValid) =>
        throw new NotImplementedException();
}
