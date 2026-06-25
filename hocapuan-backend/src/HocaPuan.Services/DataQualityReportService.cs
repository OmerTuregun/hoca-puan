using HocaPuan.Core.Utils;
using HocaPuan.Data;
using Microsoft.EntityFrameworkCore;

namespace HocaPuan.Services;

public class DataQualityIssueSample
{
    public int Id { get; set; }
    public int? UniversityId { get; set; }
    public string? UniversityName { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public class DataQualityUniversitySummary
{
    public int UniversityId { get; set; }
    public string UniversityName { get; set; } = string.Empty;
    public int BadFaculties { get; set; }
    public int BadDepartments { get; set; }
    public int BadProfessorTitles { get; set; }
    public int BadProfessorNames { get; set; }
    public int TotalIssues => BadFaculties + BadDepartments + BadProfessorTitles + BadProfessorNames;
}

public class DataQualityReport
{
    public DateTime ScannedAt { get; set; } = DateTime.UtcNow;
    public int TotalUniversities { get; set; }
    public int BadFaculties { get; set; }
    public int BadDepartments { get; set; }
    public int BadProfessorTitles { get; set; }
    public int BadProfessorNames { get; set; }
    public int TotalIssues { get; set; }
    public List<DataQualityUniversitySummary> ByUniversity { get; set; } = new();
    public List<DataQualityIssueSample> Samples { get; set; } = new();
}

public class DataQualityReportService
{
    private readonly AppDbContext _db;

    public DataQualityReportService(AppDbContext db) => _db = db;

    public async Task<DataQualityReport> ScanAsync(int samplePerCategory = 5, CancellationToken ct = default)
    {
        var report = new DataQualityReport();

        var universities = await _db.Universities
            .AsNoTracking()
            .Where(u => !u.IsDeleted)
            .Select(u => new { u.Id, u.Name })
            .ToListAsync(ct);

        report.TotalUniversities = universities.Count;

        var faculties = await _db.Faculties
            .AsNoTracking()
            .Where(f => !f.IsDeleted)
            .Select(f => new { f.Id, f.UniversityId, f.Name })
            .ToListAsync(ct);

        var departments = await _db.Departments
            .AsNoTracking()
            .Where(d => !d.IsDeleted)
            .Select(d => new { d.Id, d.FacultyId, d.Name, UniversityId = d.Faculty.UniversityId })
            .ToListAsync(ct);

        var professors = await _db.Professors
            .AsNoTracking()
            .Where(p => !p.IsDeleted)
            .Select(p => new { p.Id, p.UniversityId, p.Title, p.FirstName, p.LastName })
            .ToListAsync(ct);

        var uniNames = universities.ToDictionary(u => u.Id, u => u.Name);

        var badFaculties = faculties
            .Where(f => !FacultyDepartmentNameValidator.IsDisplayableFacultyName(
                f.Name, uniNames.GetValueOrDefault(f.UniversityId)))
            .ToList();
        var badDepartments = departments
            .Where(d => !FacultyDepartmentNameValidator.IsDisplayableDepartmentName(d.Name))
            .ToList();
        var badTitles = professors
            .Where(p => ProfessorNameValidator.HasProblematicTitle(p.Title))
            .ToList();
        var badNames = professors
            .Where(p => ProfessorNameValidator.HasProblematicNameParts(p.FirstName, p.LastName))
            .ToList();

        report.BadFaculties = badFaculties.Count;
        report.BadDepartments = badDepartments.Count;
        report.BadProfessorTitles = badTitles.Count;
        report.BadProfessorNames = badNames.Count;
        report.TotalIssues = report.BadFaculties + report.BadDepartments + report.BadProfessorTitles + report.BadProfessorNames;

        report.ByUniversity = universities
            .Select(u => new DataQualityUniversitySummary
            {
                UniversityId = u.Id,
                UniversityName = u.Name,
                BadFaculties = badFaculties.Count(f => f.UniversityId == u.Id),
                BadDepartments = badDepartments.Count(d => d.UniversityId == u.Id),
                BadProfessorTitles = badTitles.Count(p => p.UniversityId == u.Id),
                BadProfessorNames = badNames.Count(p => p.UniversityId == u.Id),
            })
            .Where(x => x.TotalIssues > 0)
            .OrderByDescending(x => x.TotalIssues)
            .ToList();

        void AddSamples(string category, IEnumerable<(int Id, int UniId, string Value)> items)
        {
            foreach (var item in items.Take(samplePerCategory))
            {
                report.Samples.Add(new DataQualityIssueSample
                {
                    Id = item.Id,
                    UniversityId = item.UniId,
                    UniversityName = uniNames.GetValueOrDefault(item.UniId),
                    Category = category,
                    Value = item.Value.Length > 160 ? item.Value[..160] + "…" : item.Value,
                });
            }
        }

        AddSamples("faculty", badFaculties.Select(f => (f.Id, f.UniversityId, f.Name)));
        AddSamples("department", badDepartments.Select(d => (d.Id, d.UniversityId, d.Name)));
        AddSamples("professor_title", badTitles.Select(p => (p.Id, p.UniversityId, p.Title)));
        AddSamples("professor_name", badNames.Select(p => (p.Id, p.UniversityId, $"{p.FirstName} | {p.LastName}")));

        return report;
    }
}
