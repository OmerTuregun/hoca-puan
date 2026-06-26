using Microsoft.AspNetCore.Mvc;

namespace HocaPuan.API.Controllers;

/// <summary>
/// Sentry entegrasyonunu doğrulamak için yalnızca Development ortamında kullanılır.
/// Production'da endpoint kayıtlı değildir.
/// </summary>
[ApiController]
[Route("api/test")]
public class SentryTestController : ControllerBase
{
    private readonly IWebHostEnvironment _env;

    public SentryTestController(IWebHostEnvironment env) => _env = env;

    /// <summary>Kasıtlı hata fırlatarak Sentry'ye gönderimi test eder (Development only).</summary>
    [HttpGet("sentry-test")]
    public IActionResult SentryTest()
    {
        if (!_env.IsDevelopment())
            return NotFound();

        throw new Exception("Sentry test");
    }
}
