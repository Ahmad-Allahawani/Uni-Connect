namespace Uni_Connect.Models
{
    public class User
    {
        public int UserID { get; set; }
        public string UniversityID { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; } // Student, Admin
        public int Points { get; set; }
        public bool IsDeleted { get; set; } = false;

        public string? ProfileImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }

        // Navigation Properties
        public ICollection<Post> Posts { get; set; }
        public ICollection<Answer> Answers { get; set; }
        public ICollection<Request> Requests { get; set; }
        public ICollection<Message> Messages { get; set; }
        public ICollection<Report> Reports { get; set; }
        public ICollection<Notification> Notifications { get; set; }
        public ICollection<PrivateSession> StudentSessions { get; set; }
        public ICollection<PrivateSession> HelperSessions { get; set; }
    }
}
