namespace UniConnect.API.Models;

public class User
{
    public int Id { get; set; }
    public string UniversityId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public int Points { get; set; } = 100; // Initial bonus
    public bool IsVerified { get; set; } = false;
    public bool IsAdmin { get; set; } = false;
    public bool IsBanned { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public ICollection<Post>? Posts { get; set; }
    public ICollection<Comment>? Comments { get; set; }
    public ICollection<AcademicRequest>? AcademicRequests { get; set; }
    public ICollection<Proposal>? Proposals { get; set; }
    public ICollection<PointsTransaction>? PointsTransactions { get; set; }
    public ICollection<Message>? SentMessages { get; set; }
    public ICollection<Message>? ReceivedMessages { get; set; }
    public ICollection<Report>? SubmittedReports { get; set; }
    public ICollection<LearningSession>? InitiatedSessions { get; set; }
    public ICollection<LearningSession>? AcceptedSessions { get; set; }
}
