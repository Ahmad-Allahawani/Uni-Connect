namespace UniConnect.API.Models;

public class AcademicRequest
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Faculty { get; set; }
    public int Course { get; set; }
    public string Topic { get; set; } = string.Empty;
    public int PointsOffered { get; set; } = 50;
    public string Status { get; set; } = "Open"; // Open, Accepted, Closed
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User? User { get; set; }
    public ICollection<Proposal>? Proposals { get; set; }
}
