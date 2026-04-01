using HocaPuan.Core.DTOs.Common;

namespace HocaPuan.Core.DTOs.Professor;

public class ProfessorListItemDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string UniversityName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public double AverageQuality { get; set; }
    public double AverageDifficulty { get; set; }
    public int WouldTakeAgainPercent { get; set; }
    public int TotalReviews { get; set; }
    public string? PhotoUrl { get; set; }
}

public class ProfessorDetailDto : ProfessorListItemDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PersonalWebsite { get; set; }
    public int UniversityId { get; set; }
    public int DepartmentId { get; set; }
    public string FacultyName { get; set; } = string.Empty;
}

public class CreateProfessorDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Email { get; set; }
    public int UniversityId { get; set; }
    public int DepartmentId { get; set; }
    public string? PersonalWebsite { get; set; }
}

public class UpdateProfessorDto : CreateProfessorDto
{
    public string? PhotoUrl { get; set; }
}

public class ProfessorSearchDto
{
    public string? Query { get; set; }
    public int? UniversityId { get; set; }
    public int? DepartmentId { get; set; }
    public string? SortBy { get; set; } = "quality";   // quality | difficulty | reviews
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
