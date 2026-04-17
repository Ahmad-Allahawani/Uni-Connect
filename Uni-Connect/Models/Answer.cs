using System.ComponentModel.DataAnnotations;

namespace Uni_Connect.Models
{
    public class Answer
    {
        public int AnswerID { get; set; }
        public int PostID { get; set; }
        public int UserID { get; set; }

        [Required, MaxLength(5000)]
        public string Content { get; set; }

        public bool IsAccepted { get; set; }
        public int Upvotes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;
        public string? ImageUrl { get; set; }

        // Navigation
        public Post Post { get; set; }
        public User User { get; set; }

    }
}
