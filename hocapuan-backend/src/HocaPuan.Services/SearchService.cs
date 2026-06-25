using System.Globalization;
using HocaPuan.Core.DTOs.Search;
using HocaPuan.Core.Interfaces.Services;
using HocaPuan.Core.Utils;
using HocaPuan.Data;
using Microsoft.EntityFrameworkCore;

namespace HocaPuan.Services;

public class SearchService : ISearchService
{
    private const int MaxUniversities = 5;
    private const int MaxProfessors = 5;
    private const int MaxDepartments = 8;
    private const int MaxTotal = 18;
    private const int UniversityPrefetch = 40;
    private const int ProfessorPrefetch = 60;
    private const int DepartmentPrefetch = 150;

    private static readonly StringComparer NameComparer =
        StringComparer.Create(CultureInfo.GetCultureInfo("tr-TR"), true);

    private readonly AppDbContext _db;

    public SearchService(AppDbContext db) => _db = db;

    public async Task<List<SearchSuggestionDto>> GetSuggestionsAsync(string? query)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Trim().Length < 2)
            return [];

        var q = query.Trim();
        var normalized = TurkishSearchNormalizer.Fold(q);
        var patterns = TurkishSearchNormalizer.GetIlikePatterns(q);
        var p0 = patterns[0];
        var p1 = patterns.Count > 1 ? patterns[1] : null;
        var p2 = patterns.Count > 2 ? patterns[2] : null;

        var universities = await SearchUniversitiesAsync(p0, p1, p2, normalized);
        var professors = await SearchProfessorsAsync(p0, p1, p2, normalized);
        var departments = await SearchDepartmentsAsync(p0, p1, p2, normalized);

        var results = new List<SearchSuggestionDto>(MaxTotal);
        results.AddRange(universities);
        results.AddRange(professors);
        results.AddRange(departments);
        return results.Take(MaxTotal).ToList();
    }

    private async Task<List<SearchSuggestionDto>> SearchUniversitiesAsync(
        string p0, string? p1, string? p2,
        string normalizedQuery)
    {
        var candidates = await _db.Universities
            .AsNoTracking()
            .Where(u =>
                EF.Functions.ILike(u.Name, p0)
                || (p1 != null && EF.Functions.ILike(u.Name, p1))
                || (p2 != null && EF.Functions.ILike(u.Name, p2)))
            .OrderBy(u => u.Name)
            .Select(u => new { u.Id, u.Name })
            .Take(UniversityPrefetch)
            .ToListAsync();

        return RankAndMap(
            candidates,
            x => x.Name,
            normalizedQuery,
            _ => 0,
            MaxUniversities,
            x => new SearchSuggestionDto
            {
                Id = x.Id,
                Name = x.Name,
                Type = "university",
            });
    }

    private async Task<List<SearchSuggestionDto>> SearchProfessorsAsync(
        string p0, string? p1, string? p2,
        string normalizedQuery)
    {
        var candidates = await _db.Professors
            .AsNoTracking()
            .Where(p => !p.IsDeleted
                && (
                    EF.Functions.ILike(p.FirstName + " " + p.LastName, p0)
                    || EF.Functions.ILike(p.FirstName, p0)
                    || EF.Functions.ILike(p.LastName, p0)
                    || (p1 != null && (
                        EF.Functions.ILike(p.FirstName + " " + p.LastName, p1)
                        || EF.Functions.ILike(p.FirstName, p1)
                        || EF.Functions.ILike(p.LastName, p1)))
                    || (p2 != null && (
                        EF.Functions.ILike(p.FirstName + " " + p.LastName, p2)
                        || EF.Functions.ILike(p.FirstName, p2)
                        || EF.Functions.ILike(p.LastName, p2)))))
            .OrderBy(p => p.LastName).ThenBy(p => p.FirstName)
            .Select(p => new
            {
                p.Id,
                p.FirstName,
                p.LastName,
                p.Title,
                UniversityName = p.University.Name,
            })
            .Take(ProfessorPrefetch)
            .ToListAsync();

        var filtered = candidates
            .Where(p => !ProfessorNameValidator.HasProblematicNameParts(p.FirstName, p.LastName))
            .Select(p => new
            {
                p.Id,
                Name = BuildProfessorDisplayName(p.Title, p.FirstName, p.LastName),
                SearchName = $"{p.FirstName} {p.LastName}".Trim(),
                p.UniversityName,
            })
            .ToList();

        return RankAndMap(
            filtered,
            x => x.SearchName,
            normalizedQuery,
            _ => 0,
            MaxProfessors,
            x => new SearchSuggestionDto
            {
                Id = x.Id,
                Name = x.Name,
                Type = "professor",
                Context = x.UniversityName,
            });
    }

    private async Task<List<SearchSuggestionDto>> SearchDepartmentsAsync(
        string p0, string? p1, string? p2,
        string normalizedQuery)
    {
        var candidates = await _db.Departments
            .AsNoTracking()
            .Where(d => !d.IsDeleted
                && !d.Faculty.IsDeleted
                && (
                    EF.Functions.ILike(d.Name, p0)
                    || (p1 != null && EF.Functions.ILike(d.Name, p1))
                    || (p2 != null && EF.Functions.ILike(d.Name, p2))))
            .OrderBy(d => d.Name)
            .Select(d => new
            {
                d.Id,
                d.Name,
                UniversityId = d.Faculty.UniversityId,
                UniversityName = d.Faculty.University.Name,
                FacultyName = d.Faculty.Name,
            })
            .Take(DepartmentPrefetch)
            .ToListAsync();

        var filtered = candidates
            .Where(d => FacultyDepartmentNameValidator.IsDisplayableDepartmentName(d.Name))
            .Select(d => (d.Id, d.Name, d.UniversityId, d.UniversityName, d.FacultyName))
            .ToList();

        return RankDepartmentsWithDiversity(filtered, normalizedQuery, MaxDepartments);
    }

    private List<SearchSuggestionDto> RankDepartmentsWithDiversity(
        List<(int Id, string Name, int UniversityId, string UniversityName, string FacultyName)> items,
        string normalizedQuery,
        int take)
    {
        var ranked = items
            .Select(item =>
            {
                var score = SearchRelevance.Score(item.Name, normalizedQuery)
                    + SearchRelevance.DepartmentBonus(item.Name, normalizedQuery);
                return new
                {
                    item,
                    score,
                    item.Name,
                    item.UniversityId,
                };
            })
            .Where(x => x.score > 0)
            .OrderByDescending(x => x.score)
            .ThenBy(x => x.Name, NameComparer)
            .ToList();

        var picked = new List<SearchSuggestionDto>();
        var seenUniversities = new HashSet<int>();

        foreach (var x in ranked)
        {
            if (picked.Count >= take) break;
            if (!seenUniversities.Add(x.UniversityId)) continue;
            picked.Add(MapDepartment(x.item));
        }

        foreach (var x in ranked)
        {
            if (picked.Count >= take) break;
            if (picked.Any(p => p.Id == x.item.Id)) continue;
            picked.Add(MapDepartment(x.item));
        }

        return picked;
    }

    private static SearchSuggestionDto MapDepartment(
        (int Id, string Name, int UniversityId, string UniversityName, string FacultyName) x) =>
        new()
        {
            Id = x.Id,
            Name = x.Name,
            Type = "department",
            UniversityId = x.UniversityId,
            Context = string.IsNullOrWhiteSpace(x.FacultyName)
                ? x.UniversityName
                : $"{x.UniversityName} · {x.FacultyName}",
        };

    private static List<SearchSuggestionDto> RankAndMap<T>(
        IEnumerable<T> items,
        Func<T, string> getSearchName,
        string normalizedQuery,
        Func<T, int> extraBonus,
        int maxCount,
        Func<T, SearchSuggestionDto> map)
    {
        return items
            .Select(item => new
            {
                item,
                searchName = getSearchName(item),
                score = SearchRelevance.Score(getSearchName(item), normalizedQuery) + extraBonus(item),
            })
            .Where(x => x.score > 0)
            .OrderByDescending(x => x.score)
            .ThenBy(x => x.searchName, NameComparer)
            .Take(maxCount)
            .Select(x => map(x.item))
            .ToList();
    }

    private static string BuildProfessorDisplayName(string? title, string firstName, string lastName)
    {
        var shortTitle = ProfessorNameValidator.HasProblematicTitle(title)
            ? string.Empty
            : ProfessorNameValidator.NormalizeTitle(title);
        var name = $"{firstName} {lastName}".Trim();
        return string.IsNullOrEmpty(shortTitle) || shortTitle == "Öğr. Gör."
            ? name
            : $"{shortTitle} {name}".Trim();
    }
}
