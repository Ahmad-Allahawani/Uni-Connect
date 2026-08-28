namespace Uni_Connect.Models
{
    public class Request
    {
        public int RequestID { get; set; }
        public int OwnerID { get; set; }
        public int PostID { get; set; }
        public string Description { get; set; }
        public string Status { get; set; } // Open, Accepted, Closed
        public DateTime CreatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;

        // Navigation
        public User Owner { get; set; }
        public Post Post { get; set; }
        public PrivateSession PrivateSession { get; set; }
    }
}
