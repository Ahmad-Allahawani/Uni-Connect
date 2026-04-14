namespace UniConnect.API.Models;

public class LearningSession
{
    public int Id { get; set; }
    public int InitiatorId { get; set; }
    public int AcceptorId { get; set; }
    public int? PostId { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Active, Completed, Cancelled
    public string? StudentRating { get; set; }
    public string? TutorRating { get; set; }
    public string? StudentFeedback { get; set; }
    public string? TutorFeedback { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Navigation properties
    public User? Initiator { get; set; }
    public User? Acceptor { get; set; }
    public Post? Post { get; set; }
    public ICollection<Message>? Messages { get; set; }
}
