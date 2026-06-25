using HocaPuan.Core.Utils;
using HocaPuan.Data;
using Microsoft.EntityFrameworkCore;

namespace HocaPuan.Services;

public class ProfessorNameCleanupResult
{
    public int ProfessorsScanned { get; set; }
    public int TitlesNormalized { get; set; }
    public int NamesFlagged { get; set; }
}

public class ProfessorNameCleanupService
{
    private readonly AppDbContext _db;

    public ProfessorNameCleanupService(AppDbContext db) => _db = db;

    public async Task<ProfessorNameCleanupResult> CleanupAsync(CancellationToken ct = default)
    {
        var result = new ProfessorNameCleanupResult();
        var professors = await _db.Professors
            .Where(p => !p.IsDeleted)
            .ToListAsync(ct);

        result.ProfessorsScanned = professors.Count;

        foreach (var p in professors)
        {
            if (ProfessorNameValidator.HasProblematicTitle(p.Title))
            {
                p.Title = ProfessorNameValidator.NormalizeTitle(p.Title);
                result.TitlesNormalized++;
            }

            if (ProfessorNameValidator.HasProblematicNameParts(p.FirstName, p.LastName))
                result.NamesFlagged++;
        }

        await _db.SaveChangesAsync(ct);
        return result;
    }
}
