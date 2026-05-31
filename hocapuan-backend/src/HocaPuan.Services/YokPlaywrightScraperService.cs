using System.Text.RegularExpressions;
using HocaPuan.Core.DTOs.Import;
using HocaPuan.Core.Entities;
using HocaPuan.Core.Interfaces.Services;
using HocaPuan.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

namespace HocaPuan.Services;

public class YokPlaywrightScraperService : IYokPlaywrightScraperService
{
    private const string BaseUrl = "https://akademik.yok.gov.tr";
    private const string UniversityListPath = "/AkademikArama/view/universityListview.jsp";
    private const int MaxReturnedNotes = 50;
    private const bool DebugFacultyDepartmentParsing = true;

    private readonly AppDbContext _dbContext;
    private readonly ILogger<YokPlaywrightScraperService> _logger;

    private static readonly Regex MultiSpaceRegex = new(@"\s+", RegexOptions.Compiled);

    public YokPlaywrightScraperService(
        AppDbContext dbContext,
        ILogger<YokPlaywrightScraperService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public async Task<YokBrowserDebugDto> DebugUniversityListPageAsync(
        CancellationToken cancellationToken = default)
    {
        var result = new YokBrowserDebugDto();

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await LaunchBrowserAsync(playwright);
        await using var context = await CreateContextAsync(browser);
        var page = await context.NewPageAsync();

        try
        {
            var response = await page.GotoAsync(BaseUrl + UniversityListPath, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.DOMContentLoaded,
                Timeout = 30_000
            });

            result.FinalUrl = page.Url;
            var content = await page.ContentAsync();
            result.HtmlLength = content.Length;
            result.IsBlocked = IsAccessBlocked(content);
            result.HtmlPreview = content.Length > 2000 ? content[..2000] : content;

            // Count AkademisyenArama links (university/faculty nav links)
            var hrefLinks = await page.QuerySelectorAllAsync("a[href*='AkademisyenArama']");
            result.HrefLinksFound = hrefLinks.Count;

            // Count onclick links as fallback indicator
            var onclickLinks = await page.QuerySelectorAllAsync("a[onclick]");
            result.OnclickLinksFound = onclickLinks.Count;

            // Sample up to 5 link texts + hrefs for inspection
            foreach (var link in hrefLinks.Take(5))
            {
                result.SampleLinkTexts.Add(Normalize1(await link.InnerTextAsync()));
                result.SampleHrefs.Add(await link.GetAttributeAsync("href") ?? "");
            }

            if (result.SampleLinkTexts.Count == 0)
            {
                // Fall back: show first 5 <a> tags whatever they are
                var anyLinks = await page.QuerySelectorAllAsync("a");
                foreach (var link in anyLinks.Take(5))
                {
                    var text = Normalize1(await link.InnerTextAsync());
                    var href = await link.GetAttributeAsync("href") ?? "";
                    var onclick = await link.GetAttributeAsync("onclick") ?? "";
                    result.SampleLinkTexts.Add($"text={text} | href={href} | onclick={onclick[..Math.Min(80, onclick.Length)]}");
                }
            }
        }
        catch (Exception ex)
        {
            result.HtmlPreview = $"EXCEPTION: {ex.Message}";
        }

        return result;
    }

    public async Task<List<YokUniversityPreviewDto>> PreviewUniversityListAsync(
        CancellationToken cancellationToken = default)
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await LaunchBrowserAsync(playwright);
        await using var context = await CreateContextAsync(browser);
        var page = await context.NewPageAsync();

        var map = await FetchUniversityBirimMapAsync(page);
        return map
            .Select(kv => new YokUniversityPreviewDto { YokName = kv.Key, BirimToken = kv.Value })
            .OrderBy(x => x.YokName)
            .ToList();
    }

    public async Task<YokBrowserDebugDto> DebugProfessorDetailAsync(
        string detailUrl,
        CancellationToken cancellationToken = default)
    {
        var result = new YokBrowserDebugDto();

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await LaunchBrowserAsync(playwright);
        await using var context = await CreateContextAsync(browser);
        var page = await context.NewPageAsync();

        try
        {
            var response = await page.GotoAsync(detailUrl, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.DOMContentLoaded,
                Timeout = 30_000
            });

            result.FinalUrl = page.Url;
            var content = await page.ContentAsync();
            result.HtmlLength = content.Length;
            result.IsBlocked = IsAccessBlocked(content);
            result.HtmlPreview = content.Length > 4000 ? content[..4000] : content;

            var bodyText = Normalize1(await page.InnerTextAsync("body"));
            if (!string.IsNullOrWhiteSpace(bodyText))
                result.HtmlPreview += "\n\n--- BODY_TEXT_PREVIEW ---\n" + (bodyText.Length > 1200 ? bodyText[..1200] : bodyText);
        }
        catch (Exception ex)
        {
            result.HtmlPreview = $"EXCEPTION: {ex.Message}";
        }

        return result;
    }

    public async Task<YokBulkImportResponseDto> ImportProfessorsAsync(
        YokBulkImportRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var response = new YokBulkImportResponseDto();

        var targetIds = request.UniversityIds?.Distinct().ToList() ?? new List<int>();
        var query = _dbContext.Universities.AsNoTracking();
        if (targetIds.Count > 0)
            query = query.Where(u => targetIds.Contains(u.Id));

        var universities = await query.OrderBy(u => u.Name).ToListAsync(cancellationToken);
        if (universities.Count == 0)
        {
            AddNote(response, "Import için üniversite bulunamadı.");
            return response;
        }

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await LaunchBrowserAsync(playwright);
        await using var browserContext = await CreateContextAsync(browser);

        // Step 1 — Build the university→birim map from YÖK's university list page
        AddNote(response, "YÖK üniversite listesi çekiliyor...");
        var listPage = await browserContext.NewPageAsync();
        var birimMap = await FetchUniversityBirimMapAsync(listPage);
        await listPage.CloseAsync();

        if (birimMap.Count == 0)
        {
            AddNote(response, "YÖK üniversite listesi alınamadı — IP engeli olabilir ya da site yapısı değişmiş.");
            return response;
        }

        AddNote(response, $"YÖK'ten {birimMap.Count} üniversite birim token'ı alındı.");

        // Step 2 — Process each target university
        bool blocked = false;
        foreach (var university in universities)
        {
            if (blocked) break;
            cancellationToken.ThrowIfCancellationRequested();

            response.UniversitiesProcessed++;

            var birimToken = FindBestMatch(university.Name, birimMap);
            if (birimToken == null)
            {
                AddNote(response, $"{university.Name}: YÖK listesinde eşleşme bulunamadı.");
                continue;
            }

            var page = await browserContext.NewPageAsync();
            try
            {
                var (professors, wasBlocked) = await ScrapeProfessorsForUniversityAsync(
                    page, birimToken, request.MaxPerUniversity, university.Name, response, cancellationToken);

                if (wasBlocked)
                {
                    AddNote(response, "YÖK erişimi engellendi (WAF/IP block). Import durduruldu.");
                    blocked = true;
                    continue;
                }

                if (professors.Count == 0)
                {
                    AddNote(response, $"{university.Name}: aday bulunamadı.");
                    continue;
                }

                response.ParsedCandidates += professors.Count;
                await SaveProfessorsAsync(university, professors, request.MaxPerUniversity, response, cancellationToken);
                AddNote(response, $"{university.Name}: {professors.Count} aday işlendi.");
            }
            finally
            {
                await page.CloseAsync();
            }
        }

        return response;
    }

    // ── YÖK Navigation ───────────────────────────────────────────────────────

    private async Task<Dictionary<string, string>> FetchUniversityBirimMapAsync(IPage page)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        try
        {
            await page.GotoAsync(BaseUrl + UniversityListPath, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.DOMContentLoaded,
                Timeout = 30_000
            });

            var content = await page.ContentAsync();
            if (IsAccessBlocked(content))
                return map;

            // Pattern A: href-based links pointing to AkademisyenArama
            // URL format: /AkademikArama/AkademisyenArama?islem=TOKEN  (or ?birim=TOKEN on sub-pages)
            var hrefLinks = await page.QuerySelectorAllAsync("a[href*='AkademisyenArama']");
            foreach (var link in hrefLinks)
            {
                var href = await link.GetAttributeAsync("href") ?? string.Empty;
                var name = Normalize1(await link.InnerTextAsync());
                if (string.IsNullOrWhiteSpace(name) || href == "#") continue;
                // Store the full relative URL as the navigation key
                map[name] = href;
            }

            // Pattern B: onclick-based links  <a onclick="someFunc('TOKEN')">Name</a>
            if (map.Count == 0)
            {
                var tokenRegex = new Regex(@"['""]([A-Za-z0-9+/=:\-_]{20,})['""]");
                var onclickLinks = await page.QuerySelectorAllAsync("a[onclick*='AkademisyenArama'], a[onclick*='islem'], a[onclick*='birim']");
                foreach (var link in onclickLinks)
                {
                    var onclick = await link.GetAttributeAsync("onclick") ?? string.Empty;
                    var name = Normalize1(await link.InnerTextAsync());
                    if (string.IsNullOrWhiteSpace(name)) continue;
                    var m = tokenRegex.Match(onclick);
                    if (m.Success) map[name] = m.Groups[1].Value;
                }
            }
        }
        catch (Exception ex)
        {
            // Non-fatal; caller checks map.Count
            _ = ex;
        }

        return map;
    }

    private async Task<(List<YokProfessorCandidate> Professors, bool WasBlocked)> ScrapeProfessorsForUniversityAsync(
        IPage page,
        string navHref,
        int maxCount,
        string universityName,
        YokBulkImportResponseDto response,
        CancellationToken ct)
    {
        var professors = new List<YokProfessorCandidate>();
        var professorKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        async Task<List<string>> CollectFacultyRelativeHrefsAsync()
        {
            var hrefs = new List<string>();
            var nodes = await page.QuerySelectorAllAsync(
                "a[href*='AkademisyenArama'][href*='birim=']");
            foreach (var link in nodes)
            {
                var href = await link.GetAttributeAsync("href") ?? string.Empty;
                if (string.IsNullOrWhiteSpace(href) || href == "#") continue;
                hrefs.Add(href);
            }

            return hrefs.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        var seedUrl = ToAbsoluteUrl(navHref);

        try
        {
            await page.GotoAsync(seedUrl, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.DOMContentLoaded,
                Timeout = 30_000
            });

            var bootstrapContent = await page.ContentAsync();
            if (IsAccessBlocked(bootstrapContent))
                return (professors, true);

            var facultyRelHrefs = await CollectFacultyRelativeHrefsAsync();
            List<string> facultyUrls;
            if (facultyRelHrefs.Count > 0)
            {
                facultyUrls = facultyRelHrefs
                    .Select(ToAbsoluteUrl)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
                _logger.LogInformation(
                    "Üniversite: {UniName} → {FacCount} fakülte bulundu",
                    universityName, facultyUrls.Count);
            }
            else
            {
                facultyUrls = new List<string> { seedUrl };
            }

            for (var i = 0; i < facultyUrls.Count; i++)
            {
                var facultyUrl = facultyUrls[i];
                ct.ThrowIfCancellationRequested();
                if (professors.Count >= maxCount) break;

                await page.GotoAsync(facultyUrl, new PageGotoOptions
                {
                    WaitUntil = WaitUntilState.DOMContentLoaded,
                    Timeout = 30_000
                });

                var content = await page.ContentAsync();
                if (IsAccessBlocked(content))
                    return (professors, true);

                var candidates = await ExtractProfessorCandidatesFromTableAsync(page, universityName, response);
                await EnrichFacultyDepartmentFromDetailPagesAsync(page.Context, universityName, candidates, response, ct);
                foreach (var c in candidates)
                {
                    if (professors.Count >= maxCount) break;
                    if (professorKeys.Add(c.UniqueKey))
                        professors.Add(c);
                }

                var visitedPageNumbers = new HashSet<int> { 1 };
                string? lastNextHref = null;
                var stableIterationsWithoutNewPages = 0;

                while (professors.Count < maxCount)
                {
                    ct.ThrowIfCancellationRequested();

                    var pageHrefMap = await GetPaginationPageHrefMapAsync(page);
                    var pageNums = pageHrefMap.Keys.OrderBy(x => x).ToList();

                    var visitedBefore = visitedPageNumbers.Count;
                    foreach (var pageNum in pageNums)
                    {
                        if (pageNum <= 1) continue;
                        if (professors.Count >= maxCount) break;
                        if (!visitedPageNumbers.Add(pageNum)) continue;

                        var href = pageHrefMap[pageNum];
                        if (string.IsNullOrWhiteSpace(href)) continue;

                        await page.GotoAsync(ToAbsoluteUrl(href), new PageGotoOptions
                        {
                            WaitUntil = WaitUntilState.DOMContentLoaded,
                            Timeout = 30_000
                        });

                        content = await page.ContentAsync();
                        if (IsAccessBlocked(content))
                            return (professors, true);

                        candidates = await ExtractProfessorCandidatesFromTableAsync(page, universityName, response);
                        await EnrichFacultyDepartmentFromDetailPagesAsync(page.Context, universityName, candidates, response, ct);
                        foreach (var c in candidates)
                        {
                            if (professors.Count >= maxCount) break;
                            if (professorKeys.Add(c.UniqueKey))
                                professors.Add(c);
                        }

                        _logger.LogInformation(
                            "[{UniName}] Fakülte {FacIdx}/{FacTotal} — sayfa {PageNum} ziyaret edildi, toplam aday: {Total}",
                            universityName, i + 1, facultyUrls.Count, pageNum, professors.Count);

                        await Task.Delay(300, ct);
                    }

                    // If we didn't discover any new page numbers from the current pagination block,
                    // try moving to the next pagination block (»). If that doesn't change anything,
                    // stop to avoid loops caused by rotating tokens.
                    if (visitedPageNumbers.Count == visitedBefore)
                        stableIterationsWithoutNewPages++;
                    else
                        stableIterationsWithoutNewPages = 0;

                    if (stableIterationsWithoutNewPages >= 2)
                        break;

                    var nextHref = await GetPaginationNextHrefAsync(page);
                    if (string.IsNullOrWhiteSpace(nextHref))
                        break;

                    // Defensive: prevent endless bouncing on the same next link with new tokens.
                    if (lastNextHref != null && Normalize1(nextHref) == Normalize1(lastNextHref))
                        break;
                    lastNextHref = nextHref;

                    await page.GotoAsync(ToAbsoluteUrl(nextHref), new PageGotoOptions
                    {
                        WaitUntil = WaitUntilState.DOMContentLoaded,
                        Timeout = 30_000
                    });

                    content = await page.ContentAsync();
                    if (IsAccessBlocked(content))
                        return (professors, true);

                    await Task.Delay(300, ct);
                }

                _logger.LogInformation(
                    "[{UniName}] Fakülte {FacIdx}/{FacTotal} tamamlandı — toplam aday: {Total}",
                    universityName, i + 1, facultyUrls.Count, professors.Count);

                if (professors.Count >= maxCount)
                    break;
                await Task.Delay(300, ct);
            }
        }
        catch (OperationCanceledException) { throw; }
        catch { }

        return (professors.Take(maxCount).ToList(), false);
    }

    private sealed record YokProfessorCandidate(
        string Title,
        string FirstName,
        string LastName,
        string FacultyName,
        string DepartmentName,
        string? Email,
        string DetailUrl)
    {
        public string FullName => $"{Title} {FirstName} {LastName}".Trim();
        public string UniqueKey => $"{NormalizeForMatch(FirstName)}\u001f{NormalizeForMatch(LastName)}\u001f{NormalizeForMatch(FacultyName)}\u001f{NormalizeForMatch(DepartmentName)}";
    }

    // Extracts professor row details from YÖK table rows:
    // - Title in first <h6>
    // - Name in <h4><a href*='AkademisyenGorevOgrenimBilgileri'>
    // - University/Faculty/Department path in the next <h6> after h4
    // - Email in mailto link if present
    private async Task<List<YokProfessorCandidate>> ExtractProfessorCandidatesFromTableAsync(
        IPage page,
        string universityName,
        YokBulkImportResponseDto response)
    {
        var list = new List<YokProfessorCandidate>();
        try
        {
            var debugLogged = 0;
            var rows = await page.QuerySelectorAllAsync("tr[id^='authorInfo_']");
            foreach (var row in rows)
            {
                // YÖK rows vary:
                // - Sometimes name is a link (<h4><a ...>NAME</a></h4>)
                // - Sometimes name is plain text (<h4>NAME</h4>) as in viewAuthor.jsp
                IElementHandle? nameEl = await row.QuerySelectorAsync("h4 a");
                var detailHref = string.Empty;
                if (nameEl != null)
                {
                    detailHref = await nameEl.GetAttributeAsync("href") ?? string.Empty;
                }
                else
                {
                    nameEl = await row.QuerySelectorAsync("h4");
                }

                if (nameEl == null) continue;
                var rawName = Normalize1(await nameEl.InnerTextAsync());
                if (string.IsNullOrWhiteSpace(rawName)) continue;

                var detailUrl = string.IsNullOrWhiteSpace(detailHref) ? string.Empty : ToAbsoluteUrl(detailHref);

                // Main content is in td[2] (nth-child(3)) on YÖK rows.
                var tdMain = await row.QuerySelectorAsync("td:nth-child(3)");

                // Title + Path are in <h6> elements:
                // - h6[0]: title
                // - h6[1]: university/faculty/department path
                // h6[0] = unvan, h6[1] = fakülte/bölüm path
                var h6Elements = await row.QuerySelectorAllAsync("h6");
                var titleEl = h6Elements.Count > 0 ? h6Elements[0] : null;

                var rawTitle = titleEl == null ? string.Empty : Normalize1(await titleEl.InnerTextAsync());

                // Path: usually h6[1]; sometimes split across h6[1], h6[2], ...
                var rawPath = string.Empty;
                if (h6Elements.Count >= 2)
                {
                    var segments = new List<string>();
                    for (var hi = 1; hi < h6Elements.Count; hi++)
                    {
                        var t = Normalize1(await h6Elements[hi].InnerTextAsync());
                        if (!string.IsNullOrWhiteSpace(t)) segments.Add(t);
                    }

                    rawPath = string.Join("/", segments);
                }
                else if (h6Elements.Count == 1)
                {
                    var h6Text = Normalize1(await h6Elements[0].InnerTextAsync());
                    if (h6Text.Contains('/') || LooksLikeFacultyUnit(h6Text) || LooksLikeDepartmentUnit(h6Text))
                        rawPath = h6Text;
                }

                if (string.IsNullOrWhiteSpace(rawPath) && tdMain != null)
                {
                    var tdText = Normalize1(await tdMain.InnerTextAsync());
                    if (!string.IsNullOrWhiteSpace(tdText))
                        rawPath = tdText;
                }

                if (rawTitle.Contains('/'))
                    rawTitle = string.Empty;

                // Email
                var emailEl = await row.QuerySelectorAsync("a[href^='mailto:']");
                var emailHref = emailEl == null ? null : await emailEl.GetAttributeAsync("href");
                var email = string.IsNullOrWhiteSpace(emailHref)
                    ? null
                    : emailHref!.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase)
                        ? emailHref[7..]
                        : null;

                // Parse name into parts (YÖK name is ALL-CAPS)
                var (titleFallback, firstName, lastName) = SplitName(rawName);
                var title = !string.IsNullOrWhiteSpace(rawTitle) ? ToDisplayTitle(rawTitle) : titleFallback;

                // Parse faculty & department from path
                var (facultyName, departmentName) = ParseFacultyDepartmentFromPath(rawPath, universityName);

                if (DebugFacultyDepartmentParsing && debugLogged < 6)
                {
                    debugLogged++;
                    AddNote(
                        response,
                        $"PARSEDBG [{universityName}] name='{rawName}' rawTitle='{rawTitle}' rawPath='{rawPath}' faculty='{facultyName}' dept='{departmentName}'");
                }

                if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
                    continue;

                list.Add(new YokProfessorCandidate(
                    Title: title,
                    FirstName: firstName,
                    LastName: lastName,
                    FacultyName: facultyName,
                    DepartmentName: departmentName,
                    Email: email,
                    DetailUrl: detailUrl));
            }
        }
        catch { /* Non-fatal */ }

        return list;
    }

    private async Task EnrichFacultyDepartmentFromDetailPagesAsync(
        IBrowserContext context,
        string universityName,
        List<YokProfessorCandidate> candidates,
        YokBulkImportResponseDto response,
        CancellationToken ct)
    {
        // Only enrich those missing faculty/dept to keep runtime bounded.
        var targets = candidates
            .Select((c, idx) => (c, idx))
            .Where(x => x.c.FacultyName == "Bilinmiyor" || x.c.DepartmentName == "Bilinmiyor")
            .Take(40)
            .ToList();

        if (targets.Count == 0) return;

        var page = await context.NewPageAsync();
        try
        {
            var debugPreviewLogged = false;
            foreach (var (c, idx) in targets)
            {
                ct.ThrowIfCancellationRequested();
                if (string.IsNullOrWhiteSpace(c.DetailUrl))
                    continue;

                try
                {
                    await page.GotoAsync(c.DetailUrl, new PageGotoOptions
                    {
                        WaitUntil = WaitUntilState.DOMContentLoaded,
                        Timeout = 30_000
                    });

                    var bodyText = Normalize1(await page.InnerTextAsync("body"));
                    if (string.IsNullOrWhiteSpace(bodyText))
                        continue;

                    if (!debugPreviewLogged)
                    {
                        debugPreviewLogged = true;
                        var preview = bodyText.Length > 600 ? bodyText[..600] : bodyText;
                        AddNote(response, $"DETAILPAGE [{universityName}] url='{c.DetailUrl}' bodyPreview='{preview}'");
                    }

                    // Liste satırıyla aynı anahtar kelime çıkarımı (detay metninde etiket varyasyonları daha fazla).
                    TryParseFacultyDepartmentFromKeywords(bodyText, out var faculty, out var dept);

                    if (faculty != "Bilinmiyor" || dept != "Bilinmiyor")
                    {
                        candidates[idx] = candidates[idx] with
                        {
                            FacultyName = faculty,
                            DepartmentName = dept
                        };

                        AddNote(
                            response,
                            $"DETAILDBG [{universityName}] name='{c.FullName}' faculty='{faculty}' dept='{dept}'");
                    }
                }
                catch
                {
                    // ignore per-candidate failures
                }

                await Task.Delay(150, ct);
            }
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    private static (string FacultyName, string DepartmentName) ParseFacultyDepartmentFromPath(string rawPath, string universityName)
    {
        // Example:
        // KOCAELİ ÜNİVERSİTESİ/MÜHENDİSLİK FAKÜLTESİ/BİLGİSAYAR MÜHENDİSLİĞİ BÖLÜMÜ/BİLGİSAYAR BİLİMLERİ ANABİLİM DALI/
        // Also: KOÜ / MÜHENDİSLİK FAKÜLTESİ / ... (üniversite adı DB ile eşleşmeyebilir)
        // Also: MÜHENDİSLİK FAKÜLTESİ / BİLGİSAYAR BÖLÜMÜ (iki parça — önceden Bilinmiyor dönüyordu)
        if (string.IsNullOrWhiteSpace(rawPath))
            return ("Bilinmiyor", "Bilinmiyor");

        var cleaned = NormalizePathForFacultyDepartment(rawPath);
        var parts = cleaned.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

        string faculty;
        string dept;

        if (parts.Count >= 3)
        {
            var uniIdx = parts.FindIndex(p => NormalizeForMatch(p).Contains(NormalizeForMatch(universityName)));
            if (uniIdx >= 0 && parts.Count >= uniIdx + 3)
            {
                faculty = ToTitleCaseTr(parts[uniIdx + 1]);
                dept = ToTitleCaseTr(parts[uniIdx + 2]);
                if (IsUsableFacultyDept(faculty, dept))
                    return (faculty, dept);
            }

            // Kısaltma veya farklı üniversite etiketi: ... / FAKÜLTE / BÖLÜM
            if (LooksLikeFacultyUnit(parts[1]) && LooksLikeDepartmentUnit(parts[2]))
            {
                faculty = ToTitleCaseTr(parts[1]);
                dept = ToTitleCaseTr(parts[2]);
                return (faculty, dept);
            }

            // FAKÜLTE / BÖLÜM / ANABİLİM DALI / ...
            if (LooksLikeFacultyUnit(parts[0]) && LooksLikeDepartmentUnit(parts[1]))
            {
                faculty = ToTitleCaseTr(parts[0]);
                dept = ToTitleCaseTr(parts[1]);
                return (faculty, dept);
            }

            var faculty2 = ToTitleCaseTr(parts[0]);
            var dept2 = ToTitleCaseTr(parts[1]);
            return (string.IsNullOrWhiteSpace(faculty2) ? "Bilinmiyor" : faculty2,
                string.IsNullOrWhiteSpace(dept2) ? "Bilinmiyor" : dept2);
        }

        if (parts.Count == 2)
        {
            if (LooksLikeFacultyUnit(parts[0]) && LooksLikeDepartmentUnit(parts[1]))
            {
                faculty = ToTitleCaseTr(parts[0]);
                dept = ToTitleCaseTr(parts[1]);
                return (faculty, dept);
            }

            if (TryParseFacultyDepartmentFromKeywords(cleaned, out faculty, out dept))
                return (faculty, dept);

            return ("Bilinmiyor", "Bilinmiyor");
        }

        if (parts.Count == 1 && TryParseFacultyDepartmentFromKeywords(parts[0], out faculty, out dept))
            return (faculty, dept);

        if (TryParseFacultyDepartmentFromKeywords(cleaned, out faculty, out dept))
            return (faculty, dept);

        return ("Bilinmiyor", "Bilinmiyor");
    }

    private static string NormalizePathForFacultyDepartment(string rawPath)
    {
        var cleaned = Normalize1(rawPath).Trim();
        cleaned = cleaned
            .Replace("\r\n", "/")
            .Replace('\r', '/')
            .Replace('\n', '/')
            .Replace("|", "/")
            .Replace(">", "/")
            .Replace("»", "/")
            .Replace("›", "/")
            .Replace("→", "/")
            .Replace("–", "/")
            .Replace("—", "/")
            .Replace("·", "/")
            .Replace("•", "/")
            .Replace("\\", "/");
        cleaned = MultiSpaceRegex.Replace(cleaned, " ").Trim().TrimEnd('/');
        return cleaned;
    }

    private static bool LooksLikeFacultyUnit(string p)
    {
        if (string.IsNullOrWhiteSpace(p)) return false;
        var n = Normalize1(p).ToUpper(new System.Globalization.CultureInfo("tr-TR"));
        if (n.Contains("FAKÜLT", StringComparison.Ordinal)) return true;
        if (n.Contains("ENSTİTÜ", StringComparison.Ordinal) || n.Contains("ENSTITU", StringComparison.OrdinalIgnoreCase))
            return true;
        if (n.Contains("YÜKSEKOKUL", StringComparison.Ordinal) || n.Contains("YUKSEKOKUL", StringComparison.OrdinalIgnoreCase))
            return true;
        if (n.Contains("MESLEK YÜKSEK", StringComparison.OrdinalIgnoreCase)) return true;
        var t = n.Trim();
        if (t.EndsWith(" MYO", StringComparison.OrdinalIgnoreCase) || t == "MYO") return true;
        return false;
    }

    private static bool LooksLikeDepartmentUnit(string p)
    {
        if (string.IsNullOrWhiteSpace(p)) return false;
        var n = Normalize1(p).ToUpper(new System.Globalization.CultureInfo("tr-TR"));
        if (n.Contains("BÖLÜM", StringComparison.Ordinal) || n.Contains("BOLUM", StringComparison.OrdinalIgnoreCase))
            return true;
        if (n.Contains("ANABİLİM", StringComparison.Ordinal) || n.Contains("ANABILIM", StringComparison.OrdinalIgnoreCase))
            return true;
        if (n.Contains("PROGRAMI", StringComparison.OrdinalIgnoreCase)) return true;
        return false;
    }

    private static bool IsUsableFacultyDept(string faculty, string dept) =>
        faculty != "Bilinmiyor" && dept != "Bilinmiyor"
        && !string.IsNullOrWhiteSpace(faculty) && !string.IsNullOrWhiteSpace(dept);

    /// <summary>
    /// YÖK bazen tek satırda veya slash olmadan "… FAKÜLTESİ … BÖLÜMÜ" verir; slash parse yetmezse kullanılır.
    /// </summary>
    private static bool TryParseFacultyDepartmentFromKeywords(string text, out string faculty, out string department)
    {
        faculty = "Bilinmiyor";
        department = "Bilinmiyor";
        if (string.IsNullOrWhiteSpace(text)) return false;

        var t = Normalize1(text);
        var facultyMatch = Regex.Match(
            t,
            @"(?i)((?:[^\n\r/|]{2,140}?(?:FAKÜLTESİ|FAKULTESI|FAKÜLTESI|ENSTİTÜSÜ|ENSTITUSU|ENSTİTÜSU|YÜKSEKOKULU|YUKSEKOKULU|MESLEK\s+YÜKSEKOKULU|MESLEK\s+YUKSEKOKULU|UYGULAMALI\s+BİLİMLER\s+YÜKSEKOKULU|UYGULAMALI\s+BILIMLER\s+YUKSEKOKULU))|(?:[^\n\r/|]{2,120}?\sMYO\b))");

        if (!facultyMatch.Success)
            return false;

        faculty = ToTitleCaseTr(facultyMatch.Groups[1].Value);
        var startAt = Math.Min(t.Length, facultyMatch.Index + facultyMatch.Length);
        var tail = t[startAt..];
        var deptMatch = Regex.Match(
            tail,
            @"(?i)([^\n\r/|]{2,180}?(BÖLÜMÜ|BOLUMU|BÖLÜMU|ANABİLİM\s+DALI|ANABILIM\s+DALI|ANA\s+BİLİM\s+DALI|PROGRAMI))\b");

        if (deptMatch.Success)
            department = ToTitleCaseTr(deptMatch.Groups[1].Value);

        return faculty != "Bilinmiyor" || department != "Bilinmiyor";
    }

    private static string ToDisplayTitle(string rawTitle)
    {
        // Keep YÖK titles readable
        var t = Normalize1(rawTitle);
        if (string.IsNullOrWhiteSpace(t)) return "Öğr. Gör.";
        // Convert common all-caps titles to canonical display
        return t switch
        {
            "DOKTOR ÖĞRETİM ÜYESİ" => "Dr. Öğr. Üyesi",
            "DOÇENT" => "Doç. Dr.",
            "PROFESÖR" => "Prof. Dr.",
            "ÖĞRETİM GÖREVLİSİ" => "Öğr. Gör.",
            "ARAŞTIRMA GÖREVLİSİ" => "Arş. Gör.",
            _ => ToTitleCaseTr(t)
        };
    }

    private static string ToTitleCaseTr(string s)
    {
        s = Normalize1(s);
        if (string.IsNullOrWhiteSpace(s)) return string.Empty;
        var culture = new System.Globalization.CultureInfo("tr-TR");
        return culture.TextInfo.ToTitleCase(s.ToLower(culture));
    }

    /// <summary>
    /// Mevcut görünümdeki sayfa numarası → href map'i.
    /// </summary>
    private async Task<Dictionary<int, string>> GetPaginationPageHrefMapAsync(IPage page)
    {
        var map = new Dictionary<int, string>();
        try
        {
            var links = await page.QuerySelectorAllAsync(
                "ul.pagination li a[href*='AramaFiltrele']");
            foreach (var link in links)
            {
                var text = Normalize1(await link.InnerTextAsync());
                if (!int.TryParse(text, System.Globalization.NumberStyles.Integer,
                        System.Globalization.CultureInfo.InvariantCulture, out var n))
                    continue;

                var href = await link.GetAttributeAsync("href") ?? string.Empty;
                if (string.IsNullOrWhiteSpace(href)) continue;
                map[n] = href;
            }
        }
        catch { /* Non-fatal */ }

        return map;
    }

    /// <summary>
    /// Pagination'daki sonraki blok linki (genelde '»'). Yoksa null.
    /// </summary>
    private async Task<string?> GetPaginationNextHrefAsync(IPage page)
    {
        try
        {
            var links = await page.QuerySelectorAllAsync("ul.pagination li a[href*='AramaFiltrele']");
            foreach (var link in links)
            {
                var text = Normalize1(await link.InnerTextAsync());
                if (text != "»" && text != ">" && text != "›" && text != "→"
                    && !text.Equals("Next", StringComparison.OrdinalIgnoreCase))
                    continue;

                var href = await link.GetAttributeAsync("href");
                if (!string.IsNullOrWhiteSpace(href))
                    return href;
            }
        }
        catch { /* Non-fatal */ }

        return null;
    }

    private string ToAbsoluteUrl(string href) =>
        href.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? href : BaseUrl + href;

    // ── Database ──────────────────────────────────────────────────────────────

    private async Task SaveProfessorsAsync(
        University university,
        List<YokProfessorCandidate> professors,
        int maxPerUniversity,
        YokBulkImportResponseDto response,
        CancellationToken ct)
    {
        var toProcess = professors.Take(maxPerUniversity).ToList();

        if (DebugFacultyDepartmentParsing && toProcess.Count > 0)
        {
            var facSample = string.Join(" | ", toProcess.Select(p => Normalize1(p.FacultyName)).Distinct().Take(5));
            var deptSample = string.Join(" | ", toProcess.Select(p => Normalize1(p.DepartmentName)).Distinct().Take(5));
            AddNote(response, $"SAVEDBG [{university.Name}] facSample='{facSample}' deptSample='{deptSample}'");
        }

        var existing = await _dbContext.Professors.AsNoTracking()
            .Where(p => p.UniversityId == university.Id)
            .Select(p => new { p.FirstName, p.LastName, p.DepartmentId })
            .ToListAsync(ct);

        var existingSet = existing
            .Select(e => $"{NameKey(e.FirstName, e.LastName)}\u001f{e.DepartmentId}")
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var facultyCache = await _dbContext.Faculties
            .Where(f => f.UniversityId == university.Id)
            .ToListAsync(ct);
        var facultyByName = facultyCache.ToDictionary(f => f.Name, StringComparer.OrdinalIgnoreCase);

        var deptCache = await _dbContext.Departments
            .Where(d => d.Faculty.UniversityId == university.Id)
            .Include(d => d.Faculty)
            .ToListAsync(ct);
        var deptByFacultyAndName = deptCache.ToDictionary(
            d => $"{d.FacultyId}\u001f{d.Name}",
            d => d,
            StringComparer.OrdinalIgnoreCase);

        async Task<int> GetOrCreateFacultyIdAsync(string facultyName)
        {
            var name = string.IsNullOrWhiteSpace(facultyName) ? "Bilinmiyor" : facultyName.Trim();
            if (facultyByName.TryGetValue(name, out var existingFaculty))
                return existingFaculty.Id;

            var faculty = new Faculty { UniversityId = university.Id, Name = name };
            _dbContext.Faculties.Add(faculty);
            await _dbContext.SaveChangesAsync(ct);
            facultyByName[name] = faculty;
            return faculty.Id;
        }

        async Task<int> GetOrCreateDepartmentIdAsync(int facultyId, string departmentName)
        {
            var name = string.IsNullOrWhiteSpace(departmentName) ? "Bilinmiyor" : departmentName.Trim();
            var key = $"{facultyId}\u001f{name}";
            if (deptByFacultyAndName.TryGetValue(key, out var existingDept))
                return existingDept.Id;

            var dept = new Department { FacultyId = facultyId, Name = name };
            _dbContext.Departments.Add(dept);
            await _dbContext.SaveChangesAsync(ct);
            deptByFacultyAndName[key] = dept;
            return dept.Id;
        }

        foreach (var prof in toProcess)
        {
            if (string.IsNullOrWhiteSpace(prof.FirstName) || string.IsNullOrWhiteSpace(prof.LastName))
                continue;

            var facultyId = await GetOrCreateFacultyIdAsync(prof.FacultyName);
            var departmentId = await GetOrCreateDepartmentIdAsync(facultyId, prof.DepartmentName);

            var key = $"{NameKey(prof.FirstName, prof.LastName)}\u001f{departmentId}";
            if (existingSet.Contains(key))
            {
                response.SkippedDuplicates++;
                continue;
            }
            existingSet.Add(key);

            _dbContext.Professors.Add(new Professor
            {
                Title = prof.Title,
                FirstName = prof.FirstName,
                LastName = prof.LastName,
                Email = prof.Email,
                UniversityId = university.Id,
                DepartmentId = departmentId
            });
            response.InsertedProfessors++;
        }

        await _dbContext.SaveChangesAsync(ct);
    }

    private static string NameKey(string firstName, string lastName) =>
        $"{firstName}\u001f{lastName}";

    private async Task<int> GetOrCreateUnknownDepartmentAsync(int universityId, CancellationToken ct)
    {
        var faculty = await _dbContext.Faculties
            .FirstOrDefaultAsync(f => f.UniversityId == universityId && f.Name == "Bilinmiyor", ct);

        if (faculty == null)
        {
            faculty = new Faculty { UniversityId = universityId, Name = "Bilinmiyor" };
            _dbContext.Faculties.Add(faculty);
            await _dbContext.SaveChangesAsync(ct);
        }

        var department = await _dbContext.Departments
            .FirstOrDefaultAsync(d => d.FacultyId == faculty.Id && d.Name == "Bilinmiyor", ct);

        if (department == null)
        {
            department = new Department { FacultyId = faculty.Id, Name = "Bilinmiyor" };
            _dbContext.Departments.Add(department);
            await _dbContext.SaveChangesAsync(ct);
        }

        return department.Id;
    }

    // ── Playwright Helpers ────────────────────────────────────────────────────

    private static async Task<IBrowser> LaunchBrowserAsync(IPlaywright playwright)
    {
        return await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true,
            Args = new[]
            {
                "--no-sandbox",
                "--disable-setuid-sandbox",
                "--disable-dev-shm-usage",
                "--disable-gpu"
            }
        });
    }

    private static async Task<IBrowserContext> CreateContextAsync(IBrowser browser)
    {
        return await browser.NewContextAsync(new BrowserNewContextOptions
        {
            UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/136.0.0.0 Safari/537.36",
            Locale = "tr-TR",
            AcceptDownloads = false
        });
    }

    // ── Text / Matching Helpers ───────────────────────────────────────────────

    private static string? FindBestMatch(string dbName, Dictionary<string, string> birimMap)
    {
        if (birimMap.TryGetValue(dbName, out var exact)) return exact;

        var normDb = NormalizeForMatch(dbName);
        foreach (var (yokName, token) in birimMap)
        {
            if (NormalizeForMatch(yokName) == normDb)
                return token;
        }

        foreach (var (yokName, token) in birimMap)
        {
            var nYok = NormalizeForMatch(yokName);
            if (normDb.Contains(nYok) || nYok.Contains(normDb))
                return token;
        }

        return null;
    }

    private static string NormalizeForMatch(string s)
    {
        return s
            .Replace('İ', 'I')
            .ToLowerInvariant()
            .Replace('ü', 'u').Replace('ö', 'o').Replace('ı', 'i')
            .Replace('ğ', 'g').Replace('ş', 's').Replace('ç', 'c')
            .Replace('â', 'a').Replace('î', 'i').Replace('û', 'u')  // şapkalı harfler
            .Replace('é', 'e').Replace('è', 'e').Replace('ê', 'e')
            .Replace("universitesi", string.Empty)
            .Replace("universiteti", string.Empty)
            .Trim();
    }

    private static string? ExtractQueryParam(string url, string param)
    {
        try
        {
            var key = $"{param}=";
            var idx = url.IndexOf(key, StringComparison.OrdinalIgnoreCase);
            if (idx < 0) return null;
            var start = idx + key.Length;
            var end = url.IndexOf('&', start);
            var raw = end < 0 ? url[start..] : url[start..end];
            return Uri.UnescapeDataString(raw);
        }
        catch { return null; }
    }

    private static bool IsAccessBlocked(string html) =>
        html.Contains("Your Access To This Page Has Been Blocked", StringComparison.OrdinalIgnoreCase)
        || html.Contains("Request ID:", StringComparison.OrdinalIgnoreCase);

    private static string Normalize1(string raw) =>
        MultiSpaceRegex.Replace(raw ?? string.Empty, " ").Trim();

    // YÖK returns names in ALL-CAPS (e.g. "BURAK ASILİSKENDER") without any title prefix.
    // We store them in Title Case, and default the academic title to "Öğr. Gör." since
    // the professor listing page does not expose the title in the link text.
    private static (string title, string firstName, string lastName) SplitName(string fullName)
    {
        var cleaned = Normalize1(fullName);

        // Strip known academic title prefixes (present when scraped from other sources)
        var knownTitles = new[]
        {
            "Prof. Dr.", "Doç. Dr.", "Dr. Öğr. Üyesi",
            "Öğr. Gör. Dr.", "Öğr. Gör.", "Arş. Gör.", "Uzman"
        };

        var matchedTitle = knownTitles.FirstOrDefault(
            t => cleaned.StartsWith(t, StringComparison.OrdinalIgnoreCase));

        var namePart = matchedTitle != null
            ? cleaned[matchedTitle.Length..].Trim()
            : cleaned;

        var parts = namePart.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2) return ("Öğr. Gör.", string.Empty, string.Empty);

        // Convert ALL-CAPS YÖK names to Title Case (e.g. "BURAK" → "Burak")
        var firstName = ToTitleCaseTr(parts[0]);
        var lastName = string.Join(' ', parts.Skip(1).Select(p => ToTitleCaseTr(p)));
        var title = matchedTitle ?? "Öğr. Gör.";

        return (title, firstName, lastName);
    }

    private static void AddNote(YokBulkImportResponseDto response, string note)
    {
        response.TotalNotes++;
        if (response.Notes.Count < MaxReturnedNotes)
        {
            response.Notes.Add(note);
            return;
        }
        response.NotesTruncated = true;
    }
}
