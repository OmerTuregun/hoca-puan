namespace HocaPuan.API.Configuration;

/// <summary>
/// JWT access token httpOnly cookie ayarları.
/// Secure ve SameSite: appsettings.{Environment}.json veya COOKIE_SECURE / USE_HTTPS ortam değişkenleri.
/// </summary>
public class AuthCookieSettings
{
    public const string SectionName = "AuthCookie";

    public string AccessTokenCookieName { get; set; } = "access_token";

    /// <summary>Production'da true; HTTP üzerindeki local dev için false.</summary>
    public bool Secure { get; set; } = true;

    /// <summary>Strict | Lax | None — cross-origin dev için None + Secure gerekir.</summary>
    public string SameSite { get; set; } = "Strict";
}
