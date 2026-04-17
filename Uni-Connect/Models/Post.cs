using System.ComponentModel.DataAnnotations;

namespace Uni_Connect.Models
{
    public class Post
    {
        public int PostID { get; set; }
        public int UserID { get; set; }
        public int CategoryID { get; set; }

        [Required, StringLength(200)]
        public string Title { get; set; }

        [Required, MaxLength(5000)]
        public string Content { get; set; }

        public int ViewsCount { get; set; }
        public int Upvotes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;
        public string? ImageUrl { get; set; }

        // Navigation
        public User User { get; set; }
        public Category Category { get; set; }
        public ICollection<Answer> Answers { get; set; }
        public ICollection<PostTag> PostTags { get; set; }
    }
}
