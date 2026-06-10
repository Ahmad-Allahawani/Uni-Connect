namespace Uni_Connect.Models
{
    public class Notification
    {
        public int NotificationID { get; set; }
        public int UserID { get; set; }
        public string Type { get; set; } // Message, Answer, Like, etc.
        public int ReferenceID { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;

        // Navigation
        public User User { get; set; }
    }
}
