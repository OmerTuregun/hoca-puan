using System.Security.Claims;
using HocaPuan.Core.DTOs.Auth;
using HocaPuan.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HocaPuan.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    public AuthController(IAuthService authService) => _authService = authService;

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Yeni kullanıcı kaydı</summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var result = await _authService.RegisterAsync(dto);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>Giriş yap, JWT token al</summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var result = await _authService.LoginAsync(dto);
        if (!result.Success) return Unauthorized(result);
        return Ok(result);
    }

    /// <summary>E-posta doğrulama</summary>
    [HttpGet("verify-email/{token}")]
    public async Task<IActionResult> VerifyEmail(string token)
    {
        var success = await _authService.VerifyEmailAsync(token);
        if (!success) return BadRequest(new { message = "Geçersiz veya süresi dolmuş token." });
        return Ok(new { message = "E-posta başarıyla doğrulandı." });
    }

    /// <summary>Şifremi unuttum</summary>
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        var result = await _authService.ForgotPasswordAsync(dto.Email);
        return Ok(result);
    }

    /// <summary>Şifre sıfırla</summary>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        var success = await _authService.ResetPasswordAsync(dto);
        if (!success) return BadRequest(new { message = "Geçersiz veya süresi dolmuş token." });
        return Ok(new { message = "Şifreniz başarıyla güncellendi." });
    }

    /// <summary>Giriş yapmış kullanıcının profili</summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMe()
    {
        var profile = await _authService.GetProfileAsync(CurrentUserId);
        if (profile == null) return NotFound();
        return Ok(profile);
    }
}
