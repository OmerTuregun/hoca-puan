namespace HocaPuan.Core.Entities;

public class Professor : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;      // Prof. Dr. / Doç. Dr. / Dr. Öğr. Üyesi / Arş. Gör.
    public string? Email { get; set; }
    public string? PhotoUrl { get; set; }
    public string? PersonalWebsite { get; set; }

    public int UniversityId { get; set; }
    public University University { get; set; } = null!;

    public int DepartmentId { get; set; }
    public Department Department { get; set; } = null!;

    // Denormalize — her yorumdan sonra güncellenir
    public double AverageQuality { get; set; } = 0;
    public double AverageDifficulty { get; set; } = 0;
    public int WouldTakeAgainPercent { get; set; } = 0;  // %
    public int TotalReviews { get; set; } = 0;

    // Navigation
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<Course> Courses { get; set; } = new List<Course>();

    // Computed
    public string FullName => $"{Title} {FirstName} {LastName}".Trim();
}
