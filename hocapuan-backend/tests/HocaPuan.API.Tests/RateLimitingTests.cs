using System.Net;
using System.Net.Http.Json;
using HocaPuan.Core.Interfaces.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace HocaPuan.API.Tests;

public class HocaPuanWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.Testing.json"), optional: false);
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["RateLimiting:Auth:PermitLimit"] = "2",
                ["RateLimiting:Auth:WindowMinutes"] = "5",
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IAuthService>();
            services.AddScoped<IAuthService, FakeAuthService>();
        });
    }
}

public class RateLimitingTests : IClassFixture<HocaPuanWebApplicationFactory>
{
    private readonly HttpClient _client;

    public RateLimitingTests(HocaPuanWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task AuthEndpoint_ExceedsLimit_Returns429WithTurkishMessage()
    {
        HttpResponseMessage? lastResponse = null;

        for (var i = 0; i < 3; i++)
        {
            lastResponse = await _client.GetAsync("/api/auth/verify-email/invalid-test-token");
            if (lastResponse.StatusCode == HttpStatusCode.TooManyRequests)
                break;
        }

        Assert.NotNull(lastResponse);
        Assert.Equal(HttpStatusCode.TooManyRequests, lastResponse.StatusCode);

        var body = await lastResponse.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.NotNull(body);
        Assert.Equal(429, body.Status);
        Assert.Contains("Çok fazla deneme", body.Message, StringComparison.Ordinal);
    }

    private sealed class ApiErrorResponse
    {
        public int Status { get; set; }
        public string Message { get; set; } = "";
    }
}
