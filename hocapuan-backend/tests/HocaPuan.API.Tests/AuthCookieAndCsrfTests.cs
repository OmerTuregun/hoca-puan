using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using HocaPuan.Core.DTOs.Auth;
using HocaPuan.Core.Interfaces.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace HocaPuan.API.Tests;

public class AuthCookieAndCsrfTests : IClassFixture<CookieTestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthCookieAndCsrfTests(CookieTestWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = true,
            AllowAutoRedirect = false,
        });
    }

    [Fact]
    public async Task Login_SetsHttpOnlyAccessTokenCookie()
    {
        var csrf = await FetchCsrfTokenAsync();

        var response = await PostLoginAsync(csrf);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var setCookie = GetSetCookieHeader(response);
        Assert.Contains("access_token=", setCookie, StringComparison.Ordinal);
        Assert.Contains("httponly", setCookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=strict", setCookie, StringComparison.OrdinalIgnoreCase);

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("success", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("\"token\"", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task StateChangingRequest_WithoutCsrfToken_IsRejected()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginDto
        {
            Email = "cookie-test@itu.edu.tr",
            Password = "password",
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ApiMessageResponse>();
        Assert.NotNull(body);
        Assert.Contains("CSRF", body.Message, StringComparison.OrdinalIgnoreCase);
    }

    private async Task<string> FetchCsrfTokenAsync()
    {
        var response = await _client.GetAsync("/api/auth/csrf-token");
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<CsrfResponse>();
        return payload!.Token;
    }

    private async Task<HttpResponseMessage> PostLoginAsync(string csrfToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login")
        {
            Content = JsonContent.Create(new LoginDto
            {
                Email = "cookie-test@itu.edu.tr",
                Password = "password",
            }),
        };
        request.Headers.Add("X-CSRF-TOKEN", csrfToken);
        return await _client.SendAsync(request);
    }

    private static string GetSetCookieHeader(HttpResponseMessage response)
    {
        if (response.Headers.TryGetValues("Set-Cookie", out var values))
            return string.Join("; ", values);

        return string.Empty;
    }

    private sealed class CsrfResponse
    {
        public string Token { get; set; } = "";
    }

    private sealed class LoginResponse
    {
        public bool Success { get; set; }
        public UserInfoDto? User { get; set; }
    }

    private sealed class ApiMessageResponse
    {
        public string Message { get; set; } = "";
    }
}

internal sealed class CookieTestAuthService : IAuthService
{
  private static readonly string TestJwt = CreateTestJwt();

    public Task<AuthResponseDto> RegisterAsync(RegisterDto dto) =>
        Task.FromResult(new AuthResponseDto { Success = false, Message = "test" });

    public Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        if (dto.Email == "cookie-test@itu.edu.tr")
        {
            return Task.FromResult(new AuthResponseDto
            {
                Success = true,
                Token = TestJwt,
                User = new UserInfoDto
                {
                    Id = 1,
                    Username = "cookieuser",
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
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Email, "cookie-test@itu.edu.tr"),
            },
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public class CookieTestWebApplicationFactory : WebApplicationFactory<Program>
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
            services.AddScoped<IAuthService, CookieTestAuthService>();
        });
    }
}
