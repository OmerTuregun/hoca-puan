using HocaPuan.Data;
using HocaPuan.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace HocaPuan.Services.Tests;

/// <summary>RUN_DATA_CLEANUP=1 ile docker'dan bir kerelik DB temizliği.</summary>
public class DataCleanupRunnerTests
{
    [Fact]
    public async Task RunFacultyAndProfessorCleanup_WhenEnvSet()
    {
        if (Environment.GetEnvironmentVariable("RUN_DATA_CLEANUP") != "1")
            return;

        var conn = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? throw new InvalidOperationException("Connection string missing");

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(conn)
            .Options;

        await using var db = new AppDbContext(options);

        var facultyResult = await new FacultyDepartmentCleanupService(db).CleanupAsync();
        var professorResult = await new ProfessorNameCleanupService(db).CleanupAsync();

        Assert.True(facultyResult.FacultiesScanned >= 0);
        Assert.True(professorResult.ProfessorsScanned > 0);
    }

    [Fact]
    public async Task RunDepartmentCleanup_WhenEnvSet()
    {
        if (Environment.GetEnvironmentVariable("RUN_DEPARTMENT_CLEANUP") != "1")
            return;

        var conn = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? throw new InvalidOperationException("Connection string missing");

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(conn)
            .Options;

        await using var db = new AppDbContext(options);

        var result = await new FacultyDepartmentCleanupService(db).CleanupDepartmentsAsync();
        Console.WriteLine(
            $"DEPARTMENT_CLEANUP_RESULT scanned={result.DepartmentsScanned} salvaged={result.DepartmentsSalvaged} " +
            $"unknown={result.DepartmentsMovedToUnknown} reassigned={result.ProfessorsReassigned} " +
            $"deduped={result.ProfessorsDeduplicated} softDeleted={result.DepartmentsSoftDeleted}");
        Assert.True(result.DepartmentsScanned >= 0);
    }
}
