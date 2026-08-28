namespace Uni_Connect.Models
{
    public class Answer
    {
        public int AnswerID { get; set; }
        public int PostID { get; set; }
        public int UserID { get; set; }
        public string Content { get; set; }
        public bool IsAccepted { get; set; }
        public int Upvotes { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;

        // Navigation
        public Post Post { get; set; }
        public User User { get; set; }

    }
}
