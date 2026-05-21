namespace Uni_Connect.Models
{
    public class PostView
    {
        public int UserID { get; set; }
        public int PostID { get; set; }
        public DateTime ViewedAt { get; set; } = DateTime.UtcNow;
        public User User { get; set; } = null!;
        public Post Post { get; set; } = null!;
    }
}
