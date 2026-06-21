namespace HocaPuan.API.Configuration;

/// <summary>
/// JWT access token httpOnly cookie ayarları.
/// Production varsayılanları: Secure=true, SameSite=Strict (aynı origin proxy).
/// Development için appsettings.Development.json'da Secure=false kullanılabilir.
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
