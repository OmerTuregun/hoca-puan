using System.Security.Claims;
using HocaPuan.API.Services;
using HocaPuan.Core.DTOs.Auth;
using HocaPuan.Core.Interfaces.Services;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using HocaPuan.API.Extensions;

namespace HocaPuan.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly AuthCookieService _authCookieService;

    public AuthController(IAuthService authService, AuthCookieService authCookieService)
    {
        _authService = authService;
        _authCookieService = authCookieService;
    }

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>CSRF token al (state-değiştiren isteklerde X-CSRF-TOKEN header'ı gerekir)</summary>
    [HttpGet("csrf-token")]
    public IActionResult GetCsrfToken([FromServices] IAntiforgery antiforgery)
    {
        var tokens = antiforgery.GetAndStoreTokens(HttpContext);
        return Ok(new { token = tokens.RequestToken });
    }

    /// <summary>Yeni kullanıcı kaydı</summary>
    [HttpPost("register")]
    [EnableRateLimiting(RateLimitingExtensions.AuthPolicy)]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var result = await _authService.RegisterAsync(dto);
        if (!result.Success) return BadRequest(result);
        return Ok(new { result.Success, result.Message });
    }

    /// <summary>Giriş yap — JWT httpOnly cookie olarak set edilir</summary>
    [HttpPost("login")]
    [EnableRateLimiting(RateLimitingExtensions.AuthPolicy)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var result = await _authService.LoginAsync(dto);
        if (!result.Success) return Unauthorized(new { result.Success, result.Message });

        _authCookieService.SetAccessTokenCookie(Response, result.Token!);
        return Ok(new { result.Success, result.User });
    }

    /// <summary>Çıkış yap — access token cookie'sini temizler</summary>
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        _authCookieService.DeleteAccessTokenCookie(Response);
        return Ok(new { message = "Çıkış yapıldı." });
    }

    /// <summary>E-posta doğrulama</summary>
    [HttpGet("verify-email/{token}")]
    [EnableRateLimiting(RateLimitingExtensions.AuthPolicy)]
    public async Task<IActionResult> VerifyEmail(string token)
    {
        var success = await _authService.VerifyEmailAsync(token);
        if (!success) return BadRequest(new { message = "Geçersiz veya süresi dolmuş token." });
        return Ok(new { message = "E-posta başarıyla doğrulandı." });
    }

    /// <summary>Şifremi unuttum</summary>
    [HttpPost("forgot-password")]
    [EnableRateLimiting(RateLimitingExtensions.AuthPolicy)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        var result = await _authService.ForgotPasswordAsync(dto.Email);
        return Ok(result);
    }

    /// <summary>Şifre sıfırla</summary>
    [HttpPost("reset-password")]
    [EnableRateLimiting(RateLimitingExtensions.AuthPolicy)]
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
