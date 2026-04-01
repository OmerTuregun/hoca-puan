namespace HocaPuan.Core.DTOs.Review;

public class ReviewDto
{
    public int Id { get; set; }
    public int ProfessorId { get; set; }
    public string ProfessorFullName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;   // Yorumu yazan
    public string? CourseCode { get; set; }
    public string? Grade { get; set; }
    public int Year { get; set; }
    public int QualityRating { get; set; }
    public int DifficultyRating { get; set; }
    public bool WouldTakeAgain { get; set; }
    public bool AttendanceMandatory { get; set; }
    public string Comment { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public string Status { get; set; } = string.Empty;
    public int ThumbsUp { get; set; }
    public int ThumbsDown { get; set; }
    public bool? CurrentUserVote { get; set; }   // null=oy vermedi, true=up, false=down
    public DateTime CreatedAt { get; set; }
}

public class CreateReviewDto
{
    public int ProfessorId { get; set; }
    public string? CourseCode { get; set; }
    public string? Grade { get; set; }
    public int Year { get; set; }
    public int QualityRating { get; set; }      // 1-5
    public int DifficultyRating { get; set; }   // 1-5
    public bool WouldTakeAgain { get; set; }
    public bool AttendanceMandatory { get; set; }
    public string Comment { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
}

public class ModerateReviewDto
{
    public bool Approve { get; set; }
    public string? ModeratorNote { get; set; }
}

public class VoteResultDto
{
    public int ThumbsUp { get; set; }
    public int ThumbsDown { get; set; }
    public bool? UserVote { get; set; }
}
