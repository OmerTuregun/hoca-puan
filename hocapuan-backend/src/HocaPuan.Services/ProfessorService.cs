using HocaPuan.Core.DTOs.Common;
using HocaPuan.Core.DTOs.Professor;
using HocaPuan.Core.Entities;
using HocaPuan.Core.Interfaces.Services;
using HocaPuan.Data;
using Microsoft.EntityFrameworkCore;

namespace HocaPuan.Services;

public class ProfessorService : IProfessorService
{
    private readonly AppDbContext _db;

    public ProfessorService(AppDbContext db) => _db = db;

    public async Task<ProfessorDetailDto?> GetByIdAsync(int id)
    {
        var p = await _db.Professors
            .Include(x => x.University)
            .Include(x => x.Department).ThenInclude(d => d.Faculty)
            .FirstOrDefaultAsync(x => x.Id == id);

        return p == null ? null : MapDetail(p);
    }

    public async Task<PagedResultDto<ProfessorListItemDto>> SearchAsync(ProfessorSearchDto dto)
    {
        var query = _db.Professors
            .Include(p => p.University)
            .Include(p => p.Department)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(dto.Query))
        {
            var q = dto.Query.Trim();
            query = query.Where(p =>
                EF.Functions.ILike(p.FirstName + " " + p.LastName, $"%{q}%"));
        }

        if (dto.UniversityId.HasValue)
            query = query.Where(p => p.UniversityId == dto.UniversityId.Value);

        if (dto.FacultyId.HasValue)
            query = query.Where(p => p.Department != null && p.Department.FacultyId == dto.FacultyId.Value);

        if (dto.DepartmentId.HasValue)
            query = query.Where(p => p.DepartmentId == dto.DepartmentId.Value);

        query = dto.SortBy switch
        {
            "difficulty"  => query.OrderByDescending(p => p.AverageDifficulty),
            "reviews"     => query.OrderByDescending(p => p.TotalReviews),
            _             => query.OrderByDescending(p => p.AverageQuality)
        };

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((dto.Page - 1) * dto.PageSize)
            .Take(dto.PageSize)
            .ToListAsync();

        return new PagedResultDto<ProfessorListItemDto>
        {
            Items = items.Select(MapListItem).ToList(),
            TotalCount = totalCount,
            Page = dto.Page,
            PageSize = dto.PageSize
        };
    }

    public async Task<ProfessorDetailDto> CreateAsync(CreateProfessorDto dto)
    {
        var professor = new Professor
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Title = dto.Title,
            Email = dto.Email,
            UniversityId = dto.UniversityId,
            DepartmentId = dto.DepartmentId,
            PersonalWebsite = dto.PersonalWebsite
        };

        _db.Professors.Add(professor);
        await _db.SaveChangesAsync();

        // Üniversite istatistiklerini güncelle
        await UpdateUniversityStatsAsync(dto.UniversityId);

        return (await GetByIdAsync(professor.Id))!;
    }

    public async Task<ProfessorDetailDto> UpdateAsync(int id, UpdateProfessorDto dto)
    {
        var professor = await _db.Professors.FindAsync(id)
            ?? throw new KeyNotFoundException("Hoca bulunamadı.");

        professor.FirstName = dto.FirstName;
        professor.LastName = dto.LastName;
        professor.Title = dto.Title;
        professor.Email = dto.Email;
        professor.UniversityId = dto.UniversityId;
        professor.DepartmentId = dto.DepartmentId;
        professor.PersonalWebsite = dto.PersonalWebsite;
        professor.PhotoUrl = dto.PhotoUrl;
        professor.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return (await GetByIdAsync(id))!;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var professor = await _db.Professors.FindAsync(id);
        if (professor == null) return false;

        professor.IsDeleted = true;
        professor.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task RecalculateStatsAsync(int professorId)
    {
        var reviews = await _db.Reviews
            .Where(r => r.ProfessorId == professorId && r.Status == ReviewStatus.Approved)
            .ToListAsync();

        var professor = await _db.Professors.FindAsync(professorId);
        if (professor == null) return;

        if (reviews.Count == 0)
        {
            professor.AverageQuality = 0;
            professor.AverageDifficulty = 0;
            professor.WouldTakeAgainPercent = 0;
            professor.TotalReviews = 0;
        }
        else
        {
            professor.AverageQuality = Math.Round(reviews.Average(r => r.QualityRating), 1);
            professor.AverageDifficulty = Math.Round(reviews.Average(r => r.DifficultyRating), 1);
            professor.WouldTakeAgainPercent = (int)Math.Round(reviews.Count(r => r.WouldTakeAgain) * 100.0 / reviews.Count);
            professor.TotalReviews = reviews.Count;
        }

        professor.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        await UpdateUniversityStatsAsync(professor.UniversityId);
    }

    // ────────────────────────────────────────────────────────────
    private async Task UpdateUniversityStatsAsync(int universityId)
    {
        var uni = await _db.Universities.FindAsync(universityId);
        if (uni == null) return;

        var profs = await _db.Professors
            .Where(p => p.UniversityId == universityId && p.TotalReviews > 0)
            .ToListAsync();

        uni.TotalProfessors = await _db.Professors.CountAsync(p => p.UniversityId == universityId);
        uni.TotalReviews = profs.Sum(p => p.TotalReviews);
        uni.AverageRating = profs.Count > 0 ? Math.Round(profs.Average(p => p.AverageQuality), 1) : 0;
        await _db.SaveChangesAsync();
    }

    private static ProfessorListItemDto MapListItem(Professor p) => new()
    {
        Id = p.Id,
        FullName = p.FullName,
        Title = p.Title,
        UniversityName = p.University?.Name ?? "",
        DepartmentName = p.Department?.Name ?? "",
        AverageQuality = p.AverageQuality,
        AverageDifficulty = p.AverageDifficulty,
        WouldTakeAgainPercent = p.WouldTakeAgainPercent,
        TotalReviews = p.TotalReviews,
        PhotoUrl = p.PhotoUrl
    };

    private static ProfessorDetailDto MapDetail(Professor p) => new()
    {
        Id = p.Id,
        FirstName = p.FirstName,
        LastName = p.LastName,
        FullName = p.FullName,
        Title = p.Title,
        Email = p.Email,
        PersonalWebsite = p.PersonalWebsite,
        UniversityId = p.UniversityId,
        UniversityName = p.University?.Name ?? "",
        DepartmentId = p.DepartmentId,
        DepartmentName = p.Department?.Name ?? "",
        FacultyName = p.Department?.Faculty?.Name ?? "",
        AverageQuality = p.AverageQuality,
        AverageDifficulty = p.AverageDifficulty,
        WouldTakeAgainPercent = p.WouldTakeAgainPercent,
        TotalReviews = p.TotalReviews,
        PhotoUrl = p.PhotoUrl
    };
}
