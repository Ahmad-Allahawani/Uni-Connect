namespace UniConnect.Web.Models
{
    public class Post
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public int Upvotes { get; set; } = 0;
        public int Downvotes { get; set; } = 0;
        public int ViewCount { get; set; } = 0;
        public bool IsResolved { get; set; } = false;
        public bool IsFlagged { get; set; } = false;
        public string? FlagReason { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        
        public virtual User? User { get; set; }
        public virtual Category? Category { get; set; }
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<PostVote> Votes { get; set; } = new List<PostVote>();
    }

    public class PostVote
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public bool IsUpvote { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public virtual Post? Post { get; set; }
        public virtual User? User { get; set; }
    }
}
