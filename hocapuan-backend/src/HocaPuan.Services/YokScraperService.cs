using System.Net;
using System.Text.RegularExpressions;
using HocaPuan.Core.DTOs.Import;
using HocaPuan.Core.Entities;
using HocaPuan.Core.Interfaces.Services;
using HocaPuan.Data;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;

namespace HocaPuan.Services;

public class YokScraperService : IYokScraperService
{
    private const string BaseUrl = "https://akademik.yok.gov.tr";
    private const int MaxReturnedNotes = 50;
    private readonly HttpClient _httpClient;
    private readonly AppDbContext _dbContext;
    private static readonly Regex MultiSpaceRegex = new(@"\s+", RegexOptions.Compiled);
    private static readonly Regex PersonLikeRegex = new(
        @"^(Prof\.?\s*Dr\.?|Doç\.?\s*Dr\.?|Dr\.?\s*Öğr\.?\s*Üyesi|Öğr\.?\s*Gör\.?\s*Dr\.?|Öğr\.?\s*Gör\.?|Arş\.?\s*Gör\.?|Uzman)?\s*[A-ZÇĞİÖŞÜ][A-Za-zÇĞİÖŞÜçğıöşü\.\-']+\s+[A-ZÇĞİÖŞÜ][A-Za-zÇĞİÖŞÜçğıöşü\.\-']+",
        RegexOptions.Compiled);

    public YokScraperService(HttpClient httpClient, AppDbContext dbContext)
    {
        _httpClient = httpClient;
        _dbContext = dbContext;
    }

    public async Task<YokScrapeResponseDto> ScrapePreviewAsync(YokScrapeRequestDto request, CancellationToken cancellationToken = default)
    {
        var outcome = await ExecuteSearchAsync(request.FormData ?? new Dictionary<string, string>(), cancellationToken);
        return new YokScrapeResponseDto
        {
            PostStatusCode = outcome.PostStatusCode,
            RedirectLocation = outcome.RedirectLocation,
            FinalStatusCode = outcome.FinalStatusCode,
            FinalUrl = outcome.FinalUrl,
            HtmlLength = outcome.Html.Length,
            HtmlPreview = outcome.Html.Length <= 4000 ? outcome.Html : outcome.Html[..4000]
        };
    }

    public async Task<YokBulkImportResponseDto> ImportProfessorsAsync(YokBulkImportRequestDto request, CancellationToken cancellationToken = default)
    {
        var response = new YokBulkImportResponseDto();
        var targetUniversityIds = request.UniversityIds?.Distinct().ToList() ?? new List<int>();

        var universitiesQuery = _dbContext.Universities.AsNoTracking();
        if (targetUniversityIds.Count > 0)
        {
            universitiesQuery = universitiesQuery.Where(u => targetUniversityIds.Contains(u.Id));
        }

        var universities = await universitiesQuery.OrderBy(u => u.Name).ToListAsync(cancellationToken);
        if (universities.Count == 0)
        {
            AddNote(response, "Import için üniversite bulunamadı.");
            return response;
        }

        foreach (var university in universities)
        {
            cancellationToken.ThrowIfCancellationRequested();
            response.UniversitiesProcessed++;

            var form = BuildSearchForm(university.Name, request.FormDataTemplate);
            var outcome = await ExecuteSearchAsync(form, cancellationToken);
            var candidates = ParseCandidates(outcome.Html);
            if (candidates.Count == 0)
            {
                var htmlHint = ExtractHtmlHint(outcome.Html);
                AddNote(
                    response,
                    $"{university.Name}: aday bulunamadı. post={outcome.PostStatusCode}, final={outcome.FinalStatusCode}, html={outcome.Html.Length}, hint={htmlHint}");

                if (IsAccessBlocked(outcome.Html))
                {
                    AddNote(response, "YOK erisimi engellendi (WAF/IP block). Import erken durduruldu.");
                    break;
                }

                continue;
            }

            var limited = candidates.Take(Math.Max(1, request.MaxPerUniversity)).ToList();
            response.ParsedCandidates += limited.Count;

            var departmentId = await GetOrCreateUnknownDepartmentAsync(university.Id, cancellationToken);

            foreach (var item in limited)
            {
                var (title, firstName, lastName) = SplitName(item);
                if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
                {
                    continue;
                }

                var exists = await _dbContext.Professors.AnyAsync(
                    p => p.UniversityId == university.Id
                         && p.DepartmentId == departmentId
                         && p.FirstName == firstName
                         && p.LastName == lastName,
                    cancellationToken);
                if (exists)
                {
                    response.SkippedDuplicates++;
                    continue;
                }

                _dbContext.Professors.Add(new Professor
                {
                    Title = title,
                    FirstName = firstName,
                    LastName = lastName,
                    UniversityId = university.Id,
                    DepartmentId = departmentId
                });
                response.InsertedProfessors++;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            AddNote(response, $"{university.Name}: {limited.Count} aday işlendi.");
        }

        return response;
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

    private async Task<(int PostStatusCode, string? RedirectLocation, int FinalStatusCode, string FinalUrl, string Html)> ExecuteSearchAsync(
        Dictionary<string, string> formData,
        CancellationToken cancellationToken)
    {
        using var warmUpResponse = await _httpClient.GetAsync("/AkademikArama/", cancellationToken);
        warmUpResponse.EnsureSuccessStatusCode();

        using var postRequest = new HttpRequestMessage(HttpMethod.Post, "/AkademikArama/AkademisyenArama")
        {
            Content = new FormUrlEncodedContent(formData)
        };
        postRequest.Headers.Referrer = new Uri($"{BaseUrl}/AkademikArama/");
        postRequest.Headers.TryAddWithoutValidation("Origin", BaseUrl);
        postRequest.Headers.TryAddWithoutValidation("Cache-Control", "max-age=0");

        using var postResponse = await _httpClient.SendAsync(postRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        var postStatus = (int)postResponse.StatusCode;
        var redirectLocation = postResponse.Headers.Location?.ToString();

        if (postResponse.StatusCode == HttpStatusCode.Found && postResponse.Headers.Location != null)
        {
            var redirectUri = postResponse.Headers.Location.IsAbsoluteUri
                ? postResponse.Headers.Location
                : new Uri(new Uri(BaseUrl), postResponse.Headers.Location);

            using var finalResponse = await _httpClient.GetAsync(redirectUri, cancellationToken);
            var html = await finalResponse.Content.ReadAsStringAsync(cancellationToken);
            return (postStatus, redirectLocation, (int)finalResponse.StatusCode, redirectUri.ToString(), html);
        }

        var fallback = await postResponse.Content.ReadAsStringAsync(cancellationToken);
        return (postStatus, redirectLocation, postStatus, $"{BaseUrl}/AkademikArama/AkademisyenArama", fallback);
    }

    private static Dictionary<string, string> BuildSearchForm(string term, Dictionary<string, string>? template)
    {
        var form = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["aramaTerim"] = term
        };

        if (template != null)
        {
            foreach (var (key, value) in template)
            {
                if (!string.IsNullOrWhiteSpace(key))
                {
                    form[key] = value ?? string.Empty;
                }
            }
        }

        var defaults = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["islem"] = "1",
            ["yazarCheckbox"] = "on",
            ["kitapCheckbox"] = "on",
            ["PatentCheckbox"] = "on",
            ["projeCheckbox"] = "on",
            ["MakaleCheckbox"] = "on",
            ["MakaeleCheckbox"] = "on",
            ["BildiriCheckbox"] = "on",
            ["SanatsalCheckbox"] = "on",
            ["TezCheckbox"] = "on"
        };

        foreach (var (key, value) in defaults)
        {
            if (!form.ContainsKey(key))
            {
                form[key] = value;
            }
        }

        form["aramaTerim"] = term;
        return form;
    }

    private static List<string> ParseCandidates(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return new List<string>();
        }

        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        var texts = doc.DocumentNode
            .SelectNodes("//a|//td|//span|//div|//h3|//h4")
            ?.Select(n => NormalizeText(WebUtility.HtmlDecode(n.InnerText)))
            .Where(t => !string.IsNullOrWhiteSpace(t) && t.Length is > 6 and < 120)
            .Distinct()
            .Where(IsLikelyPersonText)
            .ToList()
            ?? new List<string>();

        return texts;
    }

    private static bool IsLikelyPersonText(string text)
    {
        if (text.Any(char.IsDigit))
        {
            return false;
        }

        if (text.Contains("Üniversite", StringComparison.OrdinalIgnoreCase)
            || text.Contains("Fakülte", StringComparison.OrdinalIgnoreCase)
            || text.Contains("Bölüm", StringComparison.OrdinalIgnoreCase)
            || text.Contains("Ara", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return PersonLikeRegex.IsMatch(text);
    }

    private static string NormalizeText(string value)
    {
        return MultiSpaceRegex.Replace(value ?? string.Empty, " ").Trim();
    }

    private static string ExtractHtmlHint(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return "empty";
        }

        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        var text = WebUtility.HtmlDecode(doc.DocumentNode.InnerText);
        var normalized = NormalizeText(string.IsNullOrWhiteSpace(text) ? html : text);
        if (normalized.Length <= 140)
        {
            return normalized;
        }

        return normalized[..140];
    }

    private static bool IsAccessBlocked(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return false;
        }

        return html.Contains("Your Access To This Page Has Been Blocked", StringComparison.OrdinalIgnoreCase)
            || html.Contains("Request ID:", StringComparison.OrdinalIgnoreCase)
            || html.Contains("IP Address:", StringComparison.OrdinalIgnoreCase);
    }

    private static (string title, string firstName, string lastName) SplitName(string fullName)
    {
        var cleaned = NormalizeText(fullName);
        var knownTitles = new[]
        {
            "Prof. Dr.", "Doç. Dr.", "Dr. Öğr. Üyesi", "Öğr. Gör. Dr.", "Öğr. Gör.", "Arş. Gör.", "Uzman"
        };

        var title = knownTitles.FirstOrDefault(t => cleaned.StartsWith(t, StringComparison.OrdinalIgnoreCase)) ?? "Öğr. Gör.";
        var namePart = title == "Öğr. Gör." && !cleaned.StartsWith("Öğr. Gör.", StringComparison.OrdinalIgnoreCase)
            ? cleaned
            : cleaned[title.Length..].Trim();

        var parts = namePart.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2)
        {
            return (title, string.Empty, string.Empty);
        }

        var firstName = parts[0];
        var lastName = string.Join(' ', parts.Skip(1));
        return (title, firstName, lastName);
    }

    private async Task<int> GetOrCreateUnknownDepartmentAsync(int universityId, CancellationToken cancellationToken)
    {
        var faculty = await _dbContext.Faculties
            .FirstOrDefaultAsync(f => f.UniversityId == universityId && f.Name == "Bilinmiyor", cancellationToken);
        if (faculty == null)
        {
            faculty = new Faculty
            {
                UniversityId = universityId,
                Name = "Bilinmiyor"
            };
            _dbContext.Faculties.Add(faculty);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        var department = await _dbContext.Departments
            .FirstOrDefaultAsync(d => d.FacultyId == faculty.Id && d.Name == "Bilinmiyor", cancellationToken);
        if (department == null)
        {
            department = new Department
            {
                FacultyId = faculty.Id,
                Name = "Bilinmiyor"
            };
            _dbContext.Departments.Add(department);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return department.Id;
    }
}
