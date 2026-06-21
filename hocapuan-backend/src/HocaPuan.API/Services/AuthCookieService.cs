using System.IdentityModel.Tokens.Jwt;
using HocaPuan.API.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace HocaPuan.API.Services;

public class AuthCookieService
{
    private readonly AuthCookieSettings _settings;

    public AuthCookieService(IOptions<AuthCookieSettings> settings) =>
        _settings = settings.Value;

    public string CookieName => _settings.AccessTokenCookieName;

    public void SetAccessTokenCookie(HttpResponse response, string jwtToken)
    {
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(jwtToken);
        response.Cookies.Append(_settings.AccessTokenCookieName, jwtToken, BuildOptions(jwt.ValidTo));
    }

    public void DeleteAccessTokenCookie(HttpResponse response) =>
        response.Cookies.Delete(_settings.AccessTokenCookieName, new CookieOptions
        {
            HttpOnly = true,
            Secure = _settings.Secure,
            SameSite = ParseSameSite(_settings.SameSite),
            Path = "/",
        });

    private CookieOptions BuildOptions(DateTime expiresUtc) => new()
    {
        HttpOnly = true,
        Secure = _settings.Secure,
        SameSite = ParseSameSite(_settings.SameSite),
        Path = "/",
        Expires = new DateTimeOffset(expiresUtc, TimeSpan.Zero),
    };

    private static SameSiteMode ParseSameSite(string value) =>
        value.Trim().ToLowerInvariant() switch
        {
            "none" => SameSiteMode.None,
            "lax" => SameSiteMode.Lax,
            _ => SameSiteMode.Strict,
        };
}
