using HocaPuan.Core.DTOs.University;
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

        return uni == null ? null : MapDetail(uni);
    }

    public async Task<List<UniversityListItemDto>> GetAllAsync(string? search = null)
    {
        var query = _db.Universities.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(u =>
                u.Name.ToLower().Contains(s) ||
                u.ShortName.ToLower().Contains(s) ||
                u.City.ToLower().Contains(s));
        }

        var unis = await query
            .OrderByDescending(u => u.TotalReviews)
            .ThenBy(u => u.Name)
            .ToListAsync();

        return unis.Select(MapListItem).ToList();
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
        var faculties = await _db.Faculties
            .Include(f => f.Departments)
            .Where(f => f.UniversityId == universityId && !f.IsDeleted)
            .ToListAsync();

        return faculties.Select(f => new FacultyDto
        {
            Id = f.Id,
            Name = f.Name,
            UniversityId = f.UniversityId,
            Departments = f.Departments
                .Where(d => !d.IsDeleted)
                .Select(d => new DepartmentDto { Id = d.Id, Name = d.Name, FacultyId = d.FacultyId })
                .ToList()
        }).ToList();
    }

    public async Task<List<DepartmentDto>> GetDepartmentsAsync(int facultyId)
    {
        return await _db.Departments
            .Where(d => d.FacultyId == facultyId && !d.IsDeleted)
            .Select(d => new DepartmentDto { Id = d.Id, Name = d.Name, FacultyId = d.FacultyId })
            .ToListAsync();
    }

    // ────────────────────────────────────────────────────────────
    private static UniversityListItemDto MapListItem(University u) => new()
    {
        Id = u.Id,
        Name = u.Name,
        ShortName = u.ShortName,
        City = u.City,
        Type = u.Type == UniversityType.Vakif ? "Vakıf" : "Devlet",
        AverageRating = u.AverageRating,
        TotalProfessors = u.TotalProfessors,
        TotalReviews = u.TotalReviews,
        LogoUrl = u.LogoUrl
    };

    private static UniversityDetailDto MapDetail(University u) => new()
    {
        Id = u.Id,
        Name = u.Name,
        ShortName = u.ShortName,
        City = u.City,
        Type = u.Type == UniversityType.Vakif ? "Vakıf" : "Devlet",
        Website = u.Website,
        EmailDomain = u.EmailDomain,
        AverageRating = u.AverageRating,
        TotalProfessors = u.TotalProfessors,
        TotalReviews = u.TotalReviews,
        LogoUrl = u.LogoUrl,
        Faculties = u.Faculties
            .Where(f => !f.IsDeleted)
            .Select(f => new FacultyDto
            {
                Id = f.Id,
                Name = f.Name,
                UniversityId = f.UniversityId,
                Departments = f.Departments
                    .Where(d => !d.IsDeleted)
                    .Select(d => new DepartmentDto { Id = d.Id, Name = d.Name, FacultyId = d.FacultyId })
                    .ToList()
            }).ToList()
    };
}
