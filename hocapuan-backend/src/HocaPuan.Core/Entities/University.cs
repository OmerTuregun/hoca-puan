namespace HocaPuan.Core.Entities;

public class University : BaseEntity
{
    public string Name { get; set; } = string.Empty;           // İstanbul Teknik Üniversitesi
    public string ShortName { get; set; } = string.Empty;      // İTÜ
    public string City { get; set; } = string.Empty;
    public UniversityType Type { get; set; }                   // Devlet / Vakıf
    public string? Website { get; set; }
    public string? LogoUrl { get; set; }
    public string? EmailDomain { get; set; }                   // itu.edu.tr

    // Hesaplanan alanlar (denormalize — performans için)
    public double AverageRating { get; set; } = 0;
    public int TotalProfessors { get; set; } = 0;
    public int TotalReviews { get; set; } = 0;

    // Navigation
    public ICollection<Faculty> Faculties { get; set; } = new List<Faculty>();
    public ICollection<Professor> Professors { get; set; } = new List<Professor>();
}

public enum UniversityType
{
    Devlet = 0,
    Vakif = 1
}
