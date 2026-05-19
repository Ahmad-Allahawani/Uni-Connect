namespace Uni_Connect.Models
{
    public class AnswerVote
    {
        public int UserID { get; set; }
        public int AnswerID { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public User User { get; set; } = null!;
        public Answer Answer { get; set; } = null!;
    }
}
