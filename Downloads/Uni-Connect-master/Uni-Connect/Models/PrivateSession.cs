namespace Uni_Connect.Models
{
    public class PrivateSession
    {
        public int PrivateSessionID { get; set; }
        public int RequestID { get; set; }
        public int StudentID { get; set; }
        public int HelperID { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; } = false;
        // Navigation
        public Request Request { get; set; }
        public User Student { get; set; }
        public User Helper { get; set; }
        public ICollection<Message> Messages { get; set; }
        
    }
}
