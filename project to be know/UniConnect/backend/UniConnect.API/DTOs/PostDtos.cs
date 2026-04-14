namespace UniConnect.API.DTOs;

public class PostDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int Faculty { get; set; }
    public int Course { get; set; }
    public string Topic { get; set; } = string.Empty;
    public int Upvotes { get; set; }
    public int Downvotes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreatePostDto
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int Faculty { get; set; }
    public int Course { get; set; }
    public string Topic { get; set; } = string.Empty;
}

public class CommentDto
{
    public int Id { get; set; }
    public int PostId { get; set; }
    public int UserId { get; set; }
    public string Content { get; set; } = string.Empty;
    public int Upvotes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateCommentDto
{
    public string Content { get; set; } = string.Empty;
    public string? ImagePath { get; set; }
}
