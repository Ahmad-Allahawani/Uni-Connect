using System.ComponentModel.DataAnnotations;

namespace Uni_Connect.Models
{
    public class BestAnswerRequest
    {
        public int BestAnswerRequestID { get; set; }

        public int PostID { get; set; }
        public int AnswerID { get; set; }

        public int RequestedByUserID { get; set; }

        public bool IsApproved { get; set; } = false;
        public bool IsRejected { get; set; } = false;
        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReviewedAt { get; set; }

        public int? ReviewedByAdminID { get; set; }

        public Post Post { get; set; }
        public Answer Answer { get; set; }
        public User RequestedByUser { get; set; }
        public User? ReviewedByAdmin { get; set; }
    }
}