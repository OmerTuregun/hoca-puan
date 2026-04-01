namespace HocaPuan.Core.Entities;

public class Course : BaseEntity
{
    public string Code { get; set; } = string.Empty;   // BLM101
    public string Name { get; set; } = string.Empty;   // Programlamaya Giriş

    public int ProfessorId { get; set; }
    public Professor Professor { get; set; } = null!;

    public int UniversityId { get; set; }
    public University University { get; set; } = null!;
}
