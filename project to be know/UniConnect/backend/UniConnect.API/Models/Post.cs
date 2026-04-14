namespace UniConnect.API.Models;

public class Post
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int Faculty { get; set; } // Faculty ID
    public int Course { get; set; } // Course ID
    public string Topic { get; set; } = string.Empty;
    public int Upvotes { get; set; } = 0;
    public int Downvotes { get; set; } = 0;
    public bool IsModerated { get; set; } = false;
    public bool IsFlagged { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User? User { get; set; }
    public ICollection<Comment>? Comments { get; set; }
    public ICollection<Report>? Reports { get; set; }
}
