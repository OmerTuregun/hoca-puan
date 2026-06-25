using HocaPuan.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace HocaPuan.API.Controllers;

[ApiController]
public class SitemapController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public SitemapController(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    /// <summary>
    /// Turkish-aware slug: replaces Turkish chars, lowercases, hyphenates.
    /// Mirrors the TypeScript slugify.ts in the frontend.
    /// </summary>
    private static string Slugify(string text)
    {
        var trMap = new Dictionary<char, char>
        {
            { 'ş', 's' }, { 'Ş', 'S' },
            { 'ı', 'i' }, { 'İ', 'I' },
            { 'ğ', 'g' }, { 'Ğ', 'G' },
            { 'ü', 'u' }, { 'Ü', 'U' },
            { 'ö', 'o' }, { 'Ö', 'O' },
            { 'ç', 'c' }, { 'Ç', 'C' },
        };

        // Replace Turkish-specific chars first
        var sb = new StringBuilder(text.Length);
        foreach (var c in text)
            sb.Append(trMap.TryGetValue(c, out var mapped) ? mapped : c);

        // NFD-normalize and strip combining diacritical marks
        var normalized = sb.ToString().Normalize(NormalizationForm.FormD);
        var stripped = new StringBuilder(normalized.Length);
        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                stripped.Append(c);
        }

        var slug = stripped.ToString().ToLowerInvariant();
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "").Trim();
        slug = Regex.Replace(slug, @"[\s_]+", "-");
        slug = Regex.Replace(slug, @"-+", "-");
        return slug;
    }

    /// <summary>XML sitemap - tüm üniversite ve bölüm sayfaları</summary>
    [HttpGet("/sitemap.xml")]
    [ResponseCache(Duration = 3600)]
    public async Task<IActionResult> Sitemap()
    {
        var baseUrl = (_config["App__FrontendUrl"] ?? "https://hocaniyorumla.com.tr").TrimEnd('/');

        var universities = await _db.Universities
            .AsNoTracking()
            .Where(u => !u.IsDeleted)
            .Select(u => new { u.Id, u.Name, u.UpdatedAt })
            .ToListAsync();

        var departments = await _db.Departments
            .AsNoTracking()
            .Where(d => !d.IsDeleted && !d.Faculty.IsDeleted && !d.Faculty.University.IsDeleted)
            .Select(d => new
            {
                d.Id,
                d.Name,
                d.Faculty.UniversityId,
                UniversityName = d.Faculty.University.Name,
                d.UpdatedAt,
            })
            .ToListAsync();

        using var ms = new MemoryStream();
        var settings = new XmlWriterSettings { Encoding = new UTF8Encoding(false), Indent = true };
        using (var writer = XmlWriter.Create(ms, settings))
        {
            writer.WriteStartDocument();
            writer.WriteStartElement("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");

            void WriteUrl(string loc, DateTime? lastmod = null)
            {
                writer.WriteStartElement("url");
                writer.WriteElementString("loc", loc);
                if (lastmod.HasValue)
                    writer.WriteElementString("lastmod", lastmod.Value.ToString("yyyy-MM-dd"));
                writer.WriteEndElement();
            }

            WriteUrl($"{baseUrl}/");
            WriteUrl($"{baseUrl}/universities");

            foreach (var u in universities)
                WriteUrl($"{baseUrl}/universities/{u.Id}", u.UpdatedAt);

            foreach (var d in departments)
                WriteUrl(
                    $"{baseUrl}/universite/{Slugify(d.UniversityName)}/bolum/{Slugify(d.Name)}",
                    d.UpdatedAt
                );

            writer.WriteEndElement();
            writer.WriteEndDocument();
        }

        return File(ms.ToArray(), "application/xml");
    }

    /// <summary>robots.txt</summary>
    [HttpGet("/robots.txt")]
    [ResponseCache(Duration = 86400)]
    public IActionResult RobotsTxt()
    {
        var baseUrl = (_config["App__FrontendUrl"] ?? "https://hocaniyorumla.com.tr").TrimEnd('/');
        var content = $"User-agent: *\nAllow: /\nSitemap: {baseUrl}/sitemap.xml\n";
        return Content(content, "text/plain", Encoding.UTF8);
    }
}
