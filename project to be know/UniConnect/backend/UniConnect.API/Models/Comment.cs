namespace UniConnect.API.Models;

public class Comment
{
    public int Id { get; set; }
    public int PostId { get; set; }
    public int UserId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? ImagePath { get; set; }
    public int Upvotes { get; set; } = 0;
    public bool IsModerated { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Post? Post { get; set; }
    public User? User { get; set; }
}
