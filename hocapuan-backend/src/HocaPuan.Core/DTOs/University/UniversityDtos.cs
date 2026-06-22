namespace HocaPuan.Core.DTOs.University;

public class UniversityListItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ShortName { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;   // Devlet / Vakıf
    public double AverageRating { get; set; }
    public int TotalProfessors { get; set; }
    public int TotalReviews { get; set; }
    public string? LogoUrl { get; set; }
}

public class UniversityDetailDto : UniversityListItemDto
{
    public string? Website { get; set; }
    public string? EmailDomain { get; set; }
    public List<FacultyDto> Faculties { get; set; } = new();
}

public class FacultyDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int UniversityId { get; set; }
    public List<DepartmentDto> Departments { get; set; } = new();
}

public class DepartmentDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int FacultyId { get; set; }
}

public class CreateUniversityDto
{
    public string Name { get; set; } = string.Empty;
    public string ShortName { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Type { get; set; } = "Devlet";
    public string? Website { get; set; }
    public string? EmailDomain { get; set; }
}

public class TopProfessorDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string FacultyName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public double AverageQuality { get; set; }
    public int TotalReviews { get; set; }
}
