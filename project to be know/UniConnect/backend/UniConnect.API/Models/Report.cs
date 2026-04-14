namespace UniConnect.API.Models;

public class Report
{
    public int Id { get; set; }
    public int ReportedById { get; set; }
    public int? PostId { get; set; }
    public int? CommentId { get; set; }
    public int? UserId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending"; // Pending, Reviewed, Resolved
    public string? AdminNotes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }

    // Navigation properties
    public User? ReportedBy { get; set; }
    public Post? Post { get; set; }
    public Comment? Comment { get; set; }
    public User? User { get; set; }
}
