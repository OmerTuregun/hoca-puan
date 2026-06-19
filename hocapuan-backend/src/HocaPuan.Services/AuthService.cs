using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HocaPuan.Core.DTOs.Auth;
using HocaPuan.Core.Entities;
using HocaPuan.Core.Interfaces.Services;
using HocaPuan.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace HocaPuan.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;
    private readonly IEmailService _emailService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        AppDbContext db,
        IConfiguration config,
        IEmailService emailService,
        ILogger<AuthService> logger)
    {
        _db = db;
        _config = config;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        var emailExists = await _db.Users.AnyAsync(u => u.Email == dto.Email.ToLower());
        if (emailExists)
            return new AuthResponseDto { Success = false, Message = "Bu e-posta adresi zaten kullanımda." };

        var usernameExists = await _db.Users.AnyAsync(u => u.Username == dto.Username);
        if (usernameExists)
            return new AuthResponseDto { Success = false, Message = "Bu kullanıcı adı zaten alınmış." };

        var verificationToken = Guid.NewGuid().ToString("N");
        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email.ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            UniversityName = dto.UniversityName,
            EmailVerificationToken = verificationToken,
            EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24),
            IsEmailVerified = false
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var frontendUrl = (_config["App:FrontendUrl"] ?? "http://localhost:5173").TrimEnd('/');
        var verificationLink = $"{frontendUrl}/verify-email?token={verificationToken}";

        try
        {
            await _emailService.SendVerificationEmailAsync(user.Email, verificationLink);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Doğrulama e-postası gönderilemedi: {Email}", user.Email);
            return new AuthResponseDto
            {
                Success = false,
                Message = "Kayıt oluşturuldu ancak doğrulama e-postası gönderilemedi. Lütfen daha sonra tekrar deneyin."
            };
        }

        return new AuthResponseDto
        {
            Success = true,
            Message = "E-postanıza doğrulama linki gönderildi."
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email.ToLower());
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return new AuthResponseDto { Success = false, Message = "E-posta veya şifre hatalı." };

        if (user.IsBanned)
            return new AuthResponseDto { Success = false, Message = "Hesabınız askıya alınmıştır." };

        if (!user.IsEmailVerified)
            return new AuthResponseDto { Success = false, Message = "Önce e-posta adresinizi doğrulayın." };

        var token = GenerateJwtToken(user);
        return new AuthResponseDto
        {
            Success = true,
            Token = token,
            User = MapUserInfo(user)
        };
    }

    public async Task<bool> VerifyEmailAsync(string token)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u =>
            u.EmailVerificationToken == token &&
            u.EmailVerificationTokenExpiry > DateTime.UtcNow);

        if (user == null) return false;

        user.IsEmailVerified = true;
        user.EmailVerificationToken = null;
        user.EmailVerificationTokenExpiry = null;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<ForgotPasswordResponseDto> ForgotPasswordAsync(string email)
    {
        var response = new ForgotPasswordResponseDto
        {
            Message = "Kayıtlı bir hesap varsa şifre sıfırlama bağlantısı e-posta adresinize gönderildi."
        };

        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail);
        if (user == null) return response;

        var token = Guid.NewGuid().ToString("N");
        user.PasswordResetToken = token;
        user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);
        await _db.SaveChangesAsync();

        var frontendUrl = (_config["App:FrontendUrl"] ?? "http://localhost:5173").TrimEnd('/');
        var resetLink = $"{frontendUrl}/reset-password?token={token}";

        try
        {
            await _emailService.SendPasswordResetEmailAsync(user.Email, resetLink);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Şifre sıfırlama e-postası gönderilemedi: {Email}", user.Email);
        }

        return response;
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordDto dto)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u =>
            u.PasswordResetToken == dto.Token &&
            u.PasswordResetTokenExpiry > DateTime.UtcNow);

        if (user == null) return false;

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiry = null;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<UserProfileDto?> GetProfileAsync(int userId)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null) return null;

        var totalReviews = await _db.Reviews.CountAsync(r => r.UserId == userId && !r.IsDeleted);

        return new UserProfileDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            UniversityName = user.UniversityName,
            CreatedAt = user.CreatedAt,
            TotalReviews = totalReviews
        };
    }

    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _config.GetSection("JwtSettings");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(double.Parse(jwtSettings["ExpirationHours"]!)),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static UserInfoDto MapUserInfo(User user) => new()
    {
        Id = user.Id,
        Username = user.Username,
        Email = user.Email,
        UniversityName = user.UniversityName,
        Role = user.Role.ToString(),
        IsEmailVerified = user.IsEmailVerified
    };
}
