namespace UniConnect.API.Models;

public class PointsTransaction
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int Amount { get; set; }
    public string Reason { get; set; } = string.Empty; // Post, Comment, Request, Upvote, etc.
    public int? RelatedPostId { get; set; }
    public int? RelatedRequestId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User? User { get; set; }
}
