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
        public string Role { get; set; } 
        public int Points { get; set; }
        public string Faculty { get; set; }
        public string YearOfStudy { get; set; }
        public bool IsDeleted { get; set; } = false;

        public string? ProfileImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }

       
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpiry { get; set; }

        
        public int FailedLoginAttempts { get; set; } = 0;
        public DateTime? AccountLockedUntil { get; set; }

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