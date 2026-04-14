namespace UniConnect.API.Models;

public class Message
{
    public int Id { get; set; }
    public int SenderId { get; set; }
    public int ReceiverId { get; set; }
    public string Content { get; set; } = string.Empty;
    public int? LearningSessionId { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User? Sender { get; set; }
    public User? Receiver { get; set; }
    public LearningSession? LearningSession { get; set; }
}
