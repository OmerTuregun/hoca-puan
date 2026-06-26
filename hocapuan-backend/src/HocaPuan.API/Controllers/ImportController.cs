using HocaPuan.Core.DTOs.Import;
using HocaPuan.Core.Interfaces.Services;
using HocaPuan.Data;
using HocaPuan.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HocaPuan.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImportController : ControllerBase
{
    private readonly IYokScraperService _yokScraperService;
    private readonly IYokPlaywrightScraperService _yokPlaywrightScraperService;
    private readonly ImportJobStore _jobStore;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly AppDbContext _db;

    public ImportController(
        IYokScraperService yokScraperService,
        IYokPlaywrightScraperService yokPlaywrightScraperService,
        ImportJobStore jobStore,
        IServiceScopeFactory scopeFactory,
        AppDbContext db)
    {
        _yokScraperService = yokScraperService;
        _yokPlaywrightScraperService = yokPlaywrightScraperService;
        _jobStore = jobStore;
        _scopeFactory = scopeFactory;
        _db = db;
    }

    /// <summary>
    /// First step endpoint: validates YOK session + POST + redirect flow.
    /// Send form fields captured from browser DevTools.
    /// </summary>
    [HttpPost("yok/preview")]
    [Authorize(Roles = "Admin,Moderator")]
    public async Task<IActionResult> YokPreview([FromBody] YokScrapeRequestDto request, CancellationToken cancellationToken)
    {
        if (request.FormData == null || request.FormData.Count == 0)
        {
            return BadRequest(new { message = "FormData boş olamaz. YÖK form alanlarını key/value olarak gönder." });
        }

        var result = await _yokScraperService.ScrapePreviewAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Bulk import step: searches universities on YOK and stores parsed professors.
    /// </summary>
    [HttpPost("yok/import-professors")]
    [Authorize(Roles = "Admin,Moderator")]
    public async Task<IActionResult> ImportProfessors([FromBody] YokBulkImportRequestDto request, CancellationToken cancellationToken)
    {
        request.MaxPerUniversity = Math.Clamp(request.MaxPerUniversity, 10, 250);
        var result = await _yokScraperService.ImportProfessorsAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Diagnoses the YÖK university list page: returns raw HTML preview, link counts,
    /// and sample hrefs so you can see exactly what Playwright finds.
    /// </summary>
    [HttpGet("yok/browser-debug")]
    [Authorize(Roles = "Admin,Moderator")]
    public async Task<IActionResult> BrowserDebug(CancellationToken cancellationToken)
    {
        var result = await _yokPlaywrightScraperService.DebugUniversityListPageAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Lists all universities found on YÖK with their birim tokens.
    /// Useful for verifying name matching before running browser-import.
    /// </summary>
    [HttpGet("yok/browser-university-list")]
    [Authorize(Roles = "Admin,Moderator")]
    public async Task<IActionResult> BrowserUniversityList(CancellationToken cancellationToken)
    {
        var result = await _yokPlaywrightScraperService.PreviewUniversityListAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("yok/browser-debug-professor-detail")]
    [Authorize(Roles = "Admin,Moderator")]
    public async Task<IActionResult> BrowserDebugProfessorDetail(
        [FromQuery] string url,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(url))
            return BadRequest(new { message = "url is required" });

        var result = await _yokPlaywrightScraperService.DebugProfessorDetailAsync(url, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Browser-based bulk import: navigates YÖK like a real browser via Playwright
    /// instead of sending raw HTTP POST requests (which are blocked by WAF).
    /// Returns 202 with jobId; poll GET api/import/job/{id} for status and result.
    /// </summary>
    [HttpPost("yok/browser-import-professors")]
    [Authorize(Roles = "Admin,Moderator")]
    public IActionResult BrowserImportProfessors([FromBody] YokBulkImportRequestDto request)
    {
        var job = _jobStore.Create();
        var jobId = job.Id;

        var universityIds = request.UniversityIds?.ToList() ?? new List<int>();
        var maxPerUniversity = request.MaxPerUniversity;
        var formTemplate = request.FormDataTemplate is null
            ? null
            : new Dictionary<string, string>(request.FormDataTemplate);

        _ = Task.Run(async () =>
        {
            try
            {
                var dto = new YokBulkImportRequestDto
                {
                    UniversityIds = universityIds,
                    FormDataTemplate = formTemplate
                };

                if (maxPerUniversity <= 0)
                    dto.MaxPerUniversity = int.MaxValue;
                else
                    dto.MaxPerUniversity = Math.Clamp(maxPerUniversity, 10, 10_000);

                await using var scope = _scopeFactory.CreateAsyncScope();
                var scraper = scope.ServiceProvider.GetRequiredService<IYokPlaywrightScraperService>();
                var result = await scraper.ImportProfessorsAsync(dto, CancellationToken.None);
                _jobStore.Complete(jobId, result);
            }
            catch (Exception ex)
            {
                _jobStore.Fail(jobId, ex.Message);
            }
        });

        return Accepted(new
        {
            jobId,
            message = $"Import başladı. Durumu GET /api/import/job/{jobId} ile takip et."
        });
    }

    /// <summary>Playwright browser import job durumu ve sonuç.</summary>
    [HttpGet("job/{id}")]
    [Authorize(Roles = "Admin,Moderator")]
    public IActionResult GetJobStatus(string id)
    {
        var job = _jobStore.Get(id);
        if (job == null) return NotFound();
        return Ok(job);
    }

    /// <summary>Hatalı fakülte/bölüm adlarını düzeltir veya Bilinmiyor'a taşır.</summary>
    [HttpPost("cleanup-faculty-names")]
    [Authorize(Roles = "Admin,Moderator")]
    public async Task<IActionResult> CleanupFacultyNames(CancellationToken cancellationToken)
    {
        var service = new FacultyDepartmentCleanupService(_db);
        var result = await service.CleanupAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>CV/öğrenim çöpü içeren bölüm kayıtlarını düzeltir veya Bilinmiyor'a taşır.</summary>
    [HttpPost("cleanup-department-names")]
    [Authorize(Roles = "Admin,Moderator")]
    public async Task<IActionResult> CleanupDepartmentNames(CancellationToken cancellationToken)
    {
        var service = new FacultyDepartmentCleanupService(_db);
        var result = await service.CleanupDepartmentsAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>Hatalı hoca ünvanlarını kısaltır (Unvan:Doçent → Doç. Dr. vb.).</summary>
    [HttpPost("cleanup-professor-names")]
    [Authorize(Roles = "Admin,Moderator")]
    public async Task<IActionResult> CleanupProfessorNames(CancellationToken cancellationToken)
    {
        var service = new ProfessorNameCleanupService(_db);
        var result = await service.CleanupAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>Tüm üniversitelerde sorunlu fakülte, bölüm ve hoca kayıtlarını tarar.</summary>
    [HttpGet("data-quality-report")]
    [Authorize(Roles = "Admin,Moderator")]
    public async Task<IActionResult> DataQualityReport(CancellationToken cancellationToken)
    {
        var service = new DataQualityReportService(_db);
        var report = await service.ScanAsync(samplePerCategory: 8, cancellationToken);
        return Ok(report);
    }
}
