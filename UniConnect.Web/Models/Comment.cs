namespace UniConnect.Web.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public int Upvotes { get; set; } = 0;
        public int Downvotes { get; set; } = 0;
        public bool IsBestAnswer { get; set; } = false;
        public bool IsFlagged { get; set; } = false;
        public string? FlagReason { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        
        public virtual Post? Post { get; set; }
        public virtual User? User { get; set; }
        public virtual ICollection<CommentVote> Votes { get; set; } = new List<CommentVote>();
    }

    public class CommentVote
    {
        public int Id { get; set; }
        public int CommentId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public bool IsUpvote { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public virtual Comment? Comment { get; set; }
        public virtual User? User { get; set; }
    }
}
