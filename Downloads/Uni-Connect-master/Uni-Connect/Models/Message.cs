namespace Uni_Connect.Models
{
    public class Message
    {
        public int MessageID { get; set; }
        public int SessionID { get; set; }
        public int SenderID { get; set; }
        public string MessageText { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }
        public bool IsDeleted { get; set; } = false;
        // Navigation
        public PrivateSession Session { get; set; }
        public User Sender { get; set; }
       
    }
}
