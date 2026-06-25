using HocaPuan.Core.Entities;
using HocaPuan.Core.Utils;
using HocaPuan.Data;
using Microsoft.EntityFrameworkCore;

namespace HocaPuan.Services;

public class FacultyDepartmentCleanupResult
{
    public int FacultiesScanned { get; set; }
    public int FacultiesSalvaged { get; set; }
    public int FacultiesMovedToUnknown { get; set; }
    public int ProfessorsReassigned { get; set; }
    public int ProfessorsDeduplicated { get; set; }
    public int FacultiesSoftDeleted { get; set; }
    public int DepartmentsSoftDeleted { get; set; }
}

/// <summary>
/// Mevcut veritabanındaki hatalı fakülte/bölüm adlarını düzeltir veya Bilinmiyor'a taşır.
/// </summary>
public class FacultyDepartmentCleanupService
{
    private readonly AppDbContext _db;

    public FacultyDepartmentCleanupService(AppDbContext db) => _db = db;

    public async Task<FacultyDepartmentCleanupResult> CleanupAsync(CancellationToken ct = default)
    {
        var result = new FacultyDepartmentCleanupResult();

        var uniNames = await _db.Universities
            .AsNoTracking()
            .ToDictionaryAsync(u => u.Id, u => u.Name, ct);

        var junkFaculties = await _db.Faculties
            .AsNoTracking()
            .Where(f => !f.IsDeleted)
            .Select(f => new { f.Id, f.UniversityId, f.Name })
            .ToListAsync(ct);

        junkFaculties = junkFaculties
            .Where(f => !FacultyDepartmentNameValidator.IsDisplayableFacultyName(
                f.Name, uniNames.GetValueOrDefault(f.UniversityId)))
            .ToList();

        result.FacultiesScanned = junkFaculties.Count;
        if (junkFaculties.Count == 0) return result;

        var junkFacultyIds = junkFaculties.Select(f => f.Id).ToHashSet();

        var departments = await _db.Departments
            .Where(d => !d.IsDeleted && junkFacultyIds.Contains(d.FacultyId))
            .Select(d => new { d.Id, d.FacultyId })
            .ToListAsync(ct);

        var deptIds = departments.Select(d => d.Id).ToHashSet();

        var professors = await _db.Professors
            .Where(p => !p.IsDeleted && deptIds.Contains(p.DepartmentId))
            .ToListAsync(ct);

        var facultyCache = await _db.Faculties
            .Where(f => !f.IsDeleted)
            .ToDictionaryAsync(f => $"{f.UniversityId}\u001f{f.Name}", f => f.Id, ct);

        var deptCache = await _db.Departments
            .Where(d => !d.IsDeleted)
            .ToDictionaryAsync(d => $"{d.FacultyId}\u001f{d.Name}", d => d.Id, ct);

        var profKeysByDept = professors
            .GroupBy(p => p.DepartmentId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(p => $"{p.FirstName}\u001f{p.LastName}").ToHashSet(StringComparer.OrdinalIgnoreCase));

        int ResolveFacultyId(int universityId, string name)
        {
            var key = $"{universityId}\u001f{name}";
            if (facultyCache.TryGetValue(key, out var id)) return id;
            var faculty = new Faculty { UniversityId = universityId, Name = name };
            _db.Faculties.Add(faculty);
            _db.SaveChanges();
            facultyCache[key] = faculty.Id;
            return faculty.Id;
        }

        int ResolveDeptId(int facultyId, string name)
        {
            var key = $"{facultyId}\u001f{name}";
            if (deptCache.TryGetValue(key, out var id)) return id;
            var dept = new Department { FacultyId = facultyId, Name = name };
            _db.Departments.Add(dept);
            _db.SaveChanges();
            deptCache[key] = dept.Id;
            profKeysByDept[dept.Id] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            return dept.Id;
        }

        var facultyById = junkFaculties.ToDictionary(f => f.Id);

        foreach (var prof in professors)
        {
            var dept = departments.First(d => d.Id == prof.DepartmentId);
            var faculty = facultyById[dept.FacultyId];

            string targetFacultyName;
            string targetDeptName;
            if (FacultyDepartmentNameValidator.TrySalvage(faculty.Name, out var salvagedFaculty, out var salvagedDept))
            {
                targetFacultyName = salvagedFaculty;
                targetDeptName = salvagedDept != FacultyDepartmentNameValidator.Unknown
                    ? salvagedDept
                    : FacultyDepartmentNameValidator.Unknown;
            }
            else
            {
                targetFacultyName = FacultyDepartmentNameValidator.Unknown;
                targetDeptName = FacultyDepartmentNameValidator.Unknown;
            }

            var targetFacultyId = ResolveFacultyId(faculty.UniversityId, targetFacultyName);
            var targetDeptId = ResolveDeptId(targetFacultyId, targetDeptName);

            var profKey = $"{prof.FirstName}\u001f{prof.LastName}";
            if (!profKeysByDept.TryGetValue(targetDeptId, out var keys))
            {
                keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                profKeysByDept[targetDeptId] = keys;
            }

            if (keys.Contains(profKey))
            {
                prof.IsDeleted = true;
                result.ProfessorsDeduplicated++;
            }
            else
            {
                prof.DepartmentId = targetDeptId;
                keys.Add(profKey);
                result.ProfessorsReassigned++;
            }
        }

        result.FacultiesSalvaged = junkFaculties.Count(f =>
            FacultyDepartmentNameValidator.TrySalvage(f.Name, out var sf, out _)
            && sf != FacultyDepartmentNameValidator.Unknown);
        result.FacultiesMovedToUnknown = result.FacultiesScanned - result.FacultiesSalvaged;

        foreach (var deptId in deptIds)
        {
            if (professors.Any(p => !p.IsDeleted && p.DepartmentId == deptId)) continue;
            var entity = await _db.Departments.FindAsync([deptId], ct);
            if (entity != null && !entity.IsDeleted)
            {
                entity.IsDeleted = true;
                result.DepartmentsSoftDeleted++;
            }
        }

        foreach (var facultyId in junkFacultyIds)
        {
            var hasDept = await _db.Departments.AnyAsync(d => !d.IsDeleted && d.FacultyId == facultyId, ct);
            if (hasDept) continue;
            var entity = await _db.Faculties.FindAsync([facultyId], ct);
            if (entity != null && !entity.IsDeleted)
            {
                entity.IsDeleted = true;
                result.FacultiesSoftDeleted++;
            }
        }

        await _db.SaveChangesAsync(ct);
        return result;
    }
}
