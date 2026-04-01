namespace HocaPuan.Core.Entities;

public class Review : BaseEntity
{
    public int ProfessorId { get; set; }
    public Professor Professor { get; set; } = null!;

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    // Ders bilgisi
    public string? CourseCode { get; set; }        // BLM101 (isteğe bağlı)
    public string? Grade { get; set; }             // AA, BA, BB... veya "Geçtim", "Kaldım"
    public int Year { get; set; }                  // Hangi yıl aldı (2023 gibi)

    // Puanlar
    public int QualityRating { get; set; }         // 1-5 (Kalite)
    public int DifficultyRating { get; set; }      // 1-5 (Zorluk — 5 en zor)

    // Evet/Hayır sorular
    public bool WouldTakeAgain { get; set; }
    public bool AttendanceMandatory { get; set; }

    // Yorum
    public string Comment { get; set; } = string.Empty;

    // Etiketler (Türkçe) — JSON array olarak saklanır
    public string TagsJson { get; set; } = "[]";

    // Moderasyon
    public ReviewStatus Status { get; set; } = ReviewStatus.Pending;
    public string? ModeratorNote { get; set; }

    // Oylar
    public int ThumbsUp { get; set; } = 0;
    public int ThumbsDown { get; set; } = 0;

    // Navigation
    public ICollection<ReviewVote> Votes { get; set; } = new List<ReviewVote>();
}

public enum ReviewStatus
{
    Pending = 0,     // Beklemede
    Approved = 1,    // Onaylandı
    Rejected = 2     // Reddedildi
}
