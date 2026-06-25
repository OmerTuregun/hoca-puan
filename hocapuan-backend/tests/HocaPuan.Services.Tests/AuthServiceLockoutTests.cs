using HocaPuan.Core.DTOs.Auth;
using HocaPuan.Core.Entities;
using HocaPuan.Data;
using HocaPuan.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace HocaPuan.Services.Tests;

public class AuthServiceLockoutTests
{
    private const string ValidPassword = "CorrectPass1";

    private static (AppDbContext Db, AuthService Service, User User) CreateScenario()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var db = new AppDbContext(options);
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtSettings:SecretKey"] = "test-secret-key-min-32-characters-long!!",
                ["JwtSettings:Issuer"] = "Test",
                ["JwtSettings:Audience"] = "Test",
                ["JwtSettings:ExpirationHours"] = "1",
            })
            .Build();

        var service = new AuthService(db, config, new NoOpEmailService(), NullLogger<AuthService>.Instance);

        var user = new User
        {
            Username = "lockuser",
            Email = "lock@itu.edu.tr",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(ValidPassword),
            IsEmailVerified = true,
        };
        db.Users.Add(user);
        db.SaveChanges();

        return (db, service, user);
    }

    [Fact]
    public async Task LoginAsync_FourFailedAttempts_DoesNotLockAccount()
    {
        var (_, service, _) = CreateScenario();

        for (var i = 0; i < 4; i++)
        {
            var result = await service.LoginAsync(new LoginDto
            {
                Email = "lock@itu.edu.tr",
                Password = "wrong",
            });
            Assert.False(result.Success);
            Assert.False(result.IsLockedOut);
            Assert.Equal("E-posta veya şifre hatalı.", result.Message);
        }
    }

    [Fact]
    public async Task LoginAsync_FifthFailedAttempt_LocksAccount()
    {
        var (db, service, _) = CreateScenario();

        for (var i = 0; i < 4; i++)
            await service.LoginAsync(new LoginDto { Email = "lock@itu.edu.tr", Password = "wrong" });

        var fifth = await service.LoginAsync(new LoginDto { Email = "lock@itu.edu.tr", Password = "wrong" });

        Assert.False(fifth.Success);
        Assert.True(fifth.IsLockedOut);

        var user = await db.Users.FirstAsync(u => u.Email == "lock@itu.edu.tr");
        Assert.NotNull(user.LockoutEnd);
        Assert.True(user.LockoutEnd > DateTime.UtcNow);
        Assert.Equal(0, user.FailedLoginAttempts);
    }

    [Fact]
    public async Task LoginAsync_WhileLocked_CorrectPasswordStillRejected()
    {
        var (db, service, user) = CreateScenario();
        user.LockoutEnd = DateTime.UtcNow.AddMinutes(15);
        user.FailedLoginAttempts = 0;
        await db.SaveChangesAsync();

        var result = await service.LoginAsync(new LoginDto
        {
            Email = "lock@itu.edu.tr",
            Password = ValidPassword,
        });

        Assert.False(result.Success);
        Assert.True(result.IsLockedOut);
        Assert.Contains("kilitlendi", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoginAsync_AfterLockoutExpires_AllowsLogin()
    {
        var (db, service, user) = CreateScenario();
        user.LockoutEnd = DateTime.UtcNow.AddMinutes(-1);
        await db.SaveChangesAsync();

        var result = await service.LoginAsync(new LoginDto
        {
            Email = "lock@itu.edu.tr",
            Password = ValidPassword,
        });

        Assert.True(result.Success);

        user = await db.Users.FirstAsync(u => u.Email == "lock@itu.edu.tr");
        Assert.Null(user.LockoutEnd);
        Assert.Equal(0, user.FailedLoginAttempts);
    }

    [Fact]
    public async Task LoginAsync_SuccessfulLogin_ResetsFailedAttempts()
    {
        var (db, service, user) = CreateScenario();
        user.FailedLoginAttempts = 3;
        await db.SaveChangesAsync();

        var result = await service.LoginAsync(new LoginDto
        {
            Email = "lock@itu.edu.tr",
            Password = ValidPassword,
        });

        Assert.True(result.Success);

        user = await db.Users.FirstAsync(u => u.Email == "lock@itu.edu.tr");
        Assert.Equal(0, user.FailedLoginAttempts);
        Assert.Null(user.LockoutEnd);
    }

    [Fact]
    public async Task LoginAsync_UnknownEmail_ReturnsGenericMessage()
    {
        var (_, service, _) = CreateScenario();

        var result = await service.LoginAsync(new LoginDto
        {
            Email = "nobody@itu.edu.tr",
            Password = "wrong",
        });

        Assert.False(result.Success);
        Assert.False(result.IsLockedOut);
        Assert.Equal("E-posta veya şifre hatalı.", result.Message);
    }

    private sealed class NoOpEmailService : Core.Interfaces.Services.IEmailService
    {
        public Task SendVerificationEmailAsync(string email, string verificationLink) => Task.CompletedTask;
        public Task SendPasswordResetEmailAsync(string email, string resetLink) => Task.CompletedTask;
    }
}
