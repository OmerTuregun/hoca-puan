namespace HocaPuan.Core.Entities;

public class Faculty : BaseEntity
{
    public string Name { get; set; } = string.Empty;    // Mühendislik Fakültesi
    public int UniversityId { get; set; }
    public University University { get; set; } = null!;

    // Navigation
    public ICollection<Department> Departments { get; set; } = new List<Department>();
}

public class Department : BaseEntity
{
    public string Name { get; set; } = string.Empty;   // Bilgisayar Mühendisliği
    public int FacultyId { get; set; }
    public Faculty Faculty { get; set; } = null!;

    // Navigation
    public ICollection<Professor> Professors { get; set; } = new List<Professor>();
}
