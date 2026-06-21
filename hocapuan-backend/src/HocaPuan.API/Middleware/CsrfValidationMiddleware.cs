using Microsoft.AspNetCore.Antiforgery;

namespace HocaPuan.API.Middleware;

/// <summary>
/// POST/PUT/PATCH/DELETE isteklerinde antiforgery token doğrular. GET istekleri muaf.
/// </summary>
public class CsrfValidationMiddleware
{
    private static readonly HashSet<string> SafeMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        HttpMethods.Get,
        HttpMethods.Head,
        HttpMethods.Options,
        HttpMethods.Trace,
    };

    private readonly RequestDelegate _next;

    public CsrfValidationMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, IAntiforgery antiforgery)
    {
        if (SafeMethods.Contains(context.Request.Method))
        {
            await _next(context);
            return;
        }

        try
        {
            await antiforgery.ValidateRequestAsync(context);
        }
        catch (AntiforgeryValidationException)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new { message = "Geçersiz veya eksik CSRF token." });
            return;
        }

        await _next(context);
    }
}
