using HocaPuan.Core.DTOs.University;
using HocaPuan.Core.Utils;
using HocaPuan.Core.Entities;
using HocaPuan.Core.Interfaces.Services;
using HocaPuan.Data;
using Microsoft.EntityFrameworkCore;

namespace HocaPuan.Services;

public class UniversityService : IUniversityService
{
    private readonly AppDbContext _db;
    public UniversityService(AppDbContext db) => _db = db;

    public async Task<UniversityDetailDto?> GetByIdAsync(int id)
    {
        var uni = await _db.Universities
            .Include(u => u.Faculties).ThenInclude(f => f.Departments)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (uni == null) return null;

        var professorCount = await _db.Professors
            .AsNoTracking()
            .CountAsync(p => p.UniversityId == id);

        return MapDetail(uni, professorCount);
    }

    public async Task<List<UniversityListItemDto>> GetAllAsync(string? search = null)
    {
        var query = _db.Universities.AsQueryable();

        var professorCounts = await _db.Professors
            .AsNoTracking()
            .GroupBy(p => p.UniversityId)
            .Select(g => new { UniversityId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.UniversityId, x => x.Count);

        var unis = await query
            .OrderByDescending(u => u.TotalReviews)
            .ThenBy(u => u.Name)
            .ToListAsync();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalized = TurkishSearchNormalizer.Fold(search);
            var compact = normalized.Replace(" ", "");
            unis = unis
                .Where(u =>
                    TurkishSearchNormalizer.Matches(u.Name, normalized) ||
                    TurkishSearchNormalizer.Matches(u.ShortName, normalized) ||
                    TurkishSearchNormalizer.Matches(u.City, normalized) ||
                    (!string.IsNullOrEmpty(compact) &&
                     TurkishSearchNormalizer.Fold(u.Name).Replace(" ", "").Contains(compact, StringComparison.Ordinal)))
                .ToList();
        }

        return unis.Select(u => MapListItem(u, professorCounts.GetValueOrDefault(u.Id, 0))).ToList();
    }

    public async Task<UniversityDetailDto> CreateAsync(CreateUniversityDto dto)
    {
        var uni = new University
        {
            Name = dto.Name,
            ShortName = dto.ShortName,
            City = dto.City,
            Type = dto.Type == "Vakif" ? UniversityType.Vakif : UniversityType.Devlet,
            Website = dto.Website,
            EmailDomain = dto.EmailDomain
        };

        _db.Universities.Add(uni);
        await _db.SaveChangesAsync();
        return (await GetByIdAsync(uni.Id))!;
    }

    public async Task<UniversityDetailDto> UpdateAsync(int id, CreateUniversityDto dto)
    {
        var uni = await _db.Universities.FindAsync(id)
            ?? throw new KeyNotFoundException("Üniversite bulunamadı.");

        uni.Name = dto.Name;
        uni.ShortName = dto.ShortName;
        uni.City = dto.City;
        uni.Type = dto.Type == "Vakif" ? UniversityType.Vakif : UniversityType.Devlet;
        uni.Website = dto.Website;
        uni.EmailDomain = dto.EmailDomain;
        uni.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return (await GetByIdAsync(id))!;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var uni = await _db.Universities.FindAsync(id);
        if (uni == null) return false;
        uni.IsDeleted = true;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<List<FacultyDto>> GetFacultiesAsync(int universityId)
    {
        var hostUniversity = await _db.Universities
            .AsNoTracking()
            .Where(u => u.Id == universityId)
            .Select(u => u.Name)
            .FirstOrDefaultAsync();

        if (hostUniversity == null) return [];

        var professorCountsByDepartment = await _db.Professors
            .AsNoTracking()
            .Where(p => p.UniversityId == universityId && !p.IsDeleted)
            .GroupBy(p => p.DepartmentId)
            .Select(g => new { DepartmentId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.DepartmentId, x => x.Count);

        var faculties = await _db.Faculties
            .Include(f => f.Departments)
            .Where(f => f.UniversityId == universityId && !f.IsDeleted)
            .ToListAsync();

        return faculties
            .Where(f => FacultyDepartmentNameValidator.IsDisplayableFacultyName(f.Name, hostUniversity))
            .Select(f =>
            {
                var departments = f.Departments
                    .Where(d => !d.IsDeleted && FacultyDepartmentNameValidator.IsDisplayableDepartmentName(d.Name))
                    .Select(d => new DepartmentDto { Id = d.Id, Name = d.Name, FacultyId = d.FacultyId })
                    .ToList();

                return new FacultyDto
                {
                    Id = f.Id,
                    Name = f.Name,
                    UniversityId = f.UniversityId,
                    TotalProfessors = departments.Sum(d => professorCountsByDepartment.GetValueOrDefault(d.Id, 0)),
                    Departments = departments,
                };
            })
            .OrderBy(f => f.Name)
            .ToList();
    }

    public async Task<List<UniversityDepartmentListItemDto>> GetUniversityDepartmentsAsync(int universityId)
    {
        var hostUniversity = await _db.Universities
            .AsNoTracking()
            .Where(u => u.Id == universityId)
            .Select(u => u.Name)
            .FirstOrDefaultAsync();

        if (hostUniversity == null) return [];

        var professorCountsByDepartment = await _db.Professors
            .AsNoTracking()
            .Where(p => p.UniversityId == universityId && !p.IsDeleted)
            .GroupBy(p => p.DepartmentId)
            .Select(g => new { DepartmentId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.DepartmentId, x => x.Count);

        var departments = await _db.Departments
            .AsNoTracking()
            .Include(d => d.Faculty)
            .Where(d => d.Faculty.UniversityId == universityId && !d.IsDeleted && !d.Faculty.IsDeleted)
            .ToListAsync();

        return departments
            .Where(d =>
                FacultyDepartmentNameValidator.IsDisplayableDepartmentName(d.Name) &&
                FacultyDepartmentNameValidator.IsDisplayableFacultyName(d.Faculty.Name, hostUniversity))
            .Select(d => new UniversityDepartmentListItemDto
            {
                Id = d.Id,
                Name = d.Name,
                FacultyId = d.FacultyId,
                FacultyName = d.Faculty.Name,
                TotalProfessors = professorCountsByDepartment.GetValueOrDefault(d.Id, 0),
            })
            .OrderBy(d => d.Name)
            .ToList();
    }

    public async Task<List<DepartmentDto>> GetDepartmentsAsync(int facultyId)
    {
        return await _db.Departments
            .Where(d => d.FacultyId == facultyId && !d.IsDeleted)
            .Select(d => new DepartmentDto { Id = d.Id, Name = d.Name, FacultyId = d.FacultyId })
            .ToListAsync();
    }

    public async Task<List<TopProfessorDto>> GetTopProfessorsAsync(int universityId, int limit = 10)
    {
        var take = Math.Clamp(limit, 1, 10);

        return await _db.Professors
            .AsNoTracking()
            .Include(p => p.Department).ThenInclude(d => d.Faculty)
            .Where(p => p.UniversityId == universityId && !p.IsDeleted && p.TotalReviews > 0)
            .OrderByDescending(p => p.TotalReviews)
            .ThenByDescending(p => p.AverageQuality)
            .Take(take)
            .Select(p => new TopProfessorDto
            {
                Id = p.Id,
                FullName = (p.Title + " " + p.FirstName + " " + p.LastName).Trim(),
                Title = p.Title,
                FacultyName = p.Department != null ? p.Department.Faculty.Name : "",
                DepartmentName = p.Department != null ? p.Department.Name : "",
                AverageQuality = p.AverageQuality,
                TotalReviews = p.TotalReviews,
            })
            .ToListAsync();
    }

    public async Task<DepartmentDetailDto?> GetDepartmentDetailAsync(int universityId, int departmentId)
    {
        var dept = await _db.Departments
            .AsNoTracking()
            .Include(d => d.Faculty).ThenInclude(f => f.University)
            .Include(d => d.Professors.Where(p => !p.IsDeleted))
            .FirstOrDefaultAsync(d =>
                d.Id == departmentId &&
                !d.IsDeleted &&
                d.Faculty.UniversityId == universityId);

        if (dept == null) return null;

        var profs = dept.Professors.ToList();
        var reviewedProfs = profs.Where(p => p.TotalReviews > 0).ToList();

        return new DepartmentDetailDto
        {
            Id = dept.Id,
            Name = dept.Name,
            FacultyId = dept.FacultyId,
            FacultyName = dept.Faculty.Name,
            UniversityId = universityId,
            UniversityName = dept.Faculty.University.Name,
            AvgQuality = reviewedProfs.Count > 0 ? reviewedProfs.Average(p => p.AverageQuality) : 0,
            AvgDifficulty = reviewedProfs.Count > 0 ? reviewedProfs.Average(p => p.AverageDifficulty) : 0,
            TotalProfessors = profs.Count,
            TotalReviews = profs.Sum(p => p.TotalReviews),
            Professors = profs
                .OrderByDescending(p => p.TotalReviews)
                .ThenByDescending(p => p.AverageQuality)
                .Select(p => new DepartmentProfessorDto
                {
                    Id = p.Id,
                    FullName = $"{p.Title} {p.FirstName} {p.LastName}".Trim(),
                    Title = p.Title,
                    AverageQuality = p.AverageQuality,
                    AverageDifficulty = p.AverageDifficulty,
                    WouldTakeAgainPercent = p.WouldTakeAgainPercent,
                    TotalReviews = p.TotalReviews,
                })
                .ToList()
        };
    }

    // ────────────────────────────────────────────────────────────
    private static UniversityListItemDto MapListItem(University u, int liveProfessorCount) => new()
    {
        Id = u.Id,
        Name = u.Name,
        ShortName = u.ShortName,
        City = u.City,
        Type = u.Type == UniversityType.Vakif ? "Vakıf" : "Devlet",
        AverageRating = u.AverageRating,
        TotalProfessors = liveProfessorCount,
        TotalReviews = u.TotalReviews,
        LogoUrl = u.LogoUrl
    };

    private static UniversityDetailDto MapDetail(University u, int liveProfessorCount) => new()
    {
        Id = u.Id,
        Name = u.Name,
        ShortName = u.ShortName,
        City = u.City,
        Type = u.Type == UniversityType.Vakif ? "Vakıf" : "Devlet",
        Website = u.Website,
        EmailDomain = u.EmailDomain,
        AverageRating = u.AverageRating,
        TotalProfessors = liveProfessorCount,
        TotalReviews = u.TotalReviews,
        LogoUrl = u.LogoUrl,
        Faculties = u.Faculties
            .Where(f => !f.IsDeleted && FacultyDepartmentNameValidator.IsDisplayableFacultyName(f.Name, u.Name))
            .Select(f => new FacultyDto
            {
                Id = f.Id,
                Name = f.Name,
                UniversityId = f.UniversityId,
                Departments = f.Departments
                    .Where(d => !d.IsDeleted && FacultyDepartmentNameValidator.IsDisplayableDepartmentName(d.Name))
                    .Select(d => new DepartmentDto { Id = d.Id, Name = d.Name, FacultyId = d.FacultyId })
                    .ToList()
            }).ToList()
    };
}
