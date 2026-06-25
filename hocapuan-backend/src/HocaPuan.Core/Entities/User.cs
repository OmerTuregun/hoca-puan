namespace HocaPuan.Core.Entities;

public class User : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;        // .edu.tr zorunlu
    public string PasswordHash { get; set; } = string.Empty;
    public string? UniversityName { get; set; }
    public bool IsEmailVerified { get; set; } = false;
    public string? EmailVerificationToken { get; set; }
    public DateTime? EmailVerificationTokenExpiry { get; set; }
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiry { get; set; }
    public UserRole Role { get; set; } = UserRole.Student;
    public bool IsBanned { get; set; } = false;
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockoutEnd { get; set; }

    // Navigation
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<ReviewVote> ReviewVotes { get; set; } = new List<ReviewVote>();
}

public enum UserRole
{
    Student = 0,
    Moderator = 1,
    Admin = 2
}
