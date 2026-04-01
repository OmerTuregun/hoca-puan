using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HocaPuan.Core.DTOs.Auth;
using HocaPuan.Core.Entities;
using HocaPuan.Core.Interfaces.Services;
using HocaPuan.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace HocaPuan.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public AuthService(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        // .edu.tr zorunluluğu — production'da açın, dev'de kapatılabilir
        // if (!dto.Email.EndsWith(".edu.tr", StringComparison.OrdinalIgnoreCase))
        //     return new AuthResponseDto { Success = false, Message = "Sadece .edu.tr uzantılı e-posta adresleri kabul edilmektedir." };

        var emailExists = await _db.Users.AnyAsync(u => u.Email == dto.Email.ToLower());
        if (emailExists)
            return new AuthResponseDto { Success = false, Message = "Bu e-posta adresi zaten kullanımda." };

        var usernameExists = await _db.Users.AnyAsync(u => u.Username == dto.Username);
        if (usernameExists)
            return new AuthResponseDto { Success = false, Message = "Bu kullanıcı adı zaten alınmış." };

        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email.ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            UniversityName = dto.UniversityName,
            EmailVerificationToken = Guid.NewGuid().ToString(),
            EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24),
            IsEmailVerified = true // TODO: E-posta doğrulamasını aktif etmek için false yapın
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        // TODO: Doğrulama e-postası gönder

        var token = GenerateJwtToken(user);
        return new AuthResponseDto
        {
            Success = true,
            Token = token,
            Message = "Kayıt başarılı.",
            User = MapUserInfo(user)
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email.ToLower());
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return new AuthResponseDto { Success = false, Message = "E-posta veya şifre hatalı." };

        if (user.IsBanned)
            return new AuthResponseDto { Success = false, Message = "Hesabınız askıya alınmıştır." };

        // if (!user.IsEmailVerified)
        //     return new AuthResponseDto { Success = false, Message = "Lütfen önce e-posta adresinizi doğrulayın." };

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

    public async Task<bool> ForgotPasswordAsync(string email)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email.ToLower());
        if (user == null) return false;

        user.PasswordResetToken = Guid.NewGuid().ToString();
        user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);
        await _db.SaveChangesAsync();

        // TODO: Şifre sıfırlama e-postası gönder
        return true;
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

    // ────────────────────────────────────────────────────────────
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
