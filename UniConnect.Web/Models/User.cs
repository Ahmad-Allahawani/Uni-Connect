using Microsoft.AspNetCore.Identity;

namespace UniConnect.Web.Models
{
    public class User : IdentityUser
    {
        public string UniversityId { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Faculty { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public int Points { get; set; } = 0;
        public bool IsVerified { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? LastLoginAt { get; set; }
        public int TotalPosts { get; set; } = 0;
        public int TotalComments { get; set; } = 0;
        public int TotalSessions { get; set; } = 0;
        public double Rating { get; set; } = 0;
        public int TotalRatings { get; set; } = 0;
        public int Streak { get; set; } = 0;
        public DateTime? LastActiveDate { get; set; }
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<AcademicRequest> Requests { get; set; } = new List<AcademicRequest>();
        public virtual ICollection<Proposal> Proposals { get; set; } = new List<Proposal>();
        public virtual ICollection<PrivateSession> SessionsAsStudent { get; set; } = new List<PrivateSession>();
        public virtual ICollection<PrivateSession> SessionsAsTutor { get; set; } = new List<PrivateSession>();
        public virtual ICollection<Message> SentMessages { get; set; } = new List<Message>();
        public virtual ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();
        public virtual ICollection<Report> ReportsMade { get; set; } = new List<Report>();
        public virtual ICollection<Report> ReportsReceived { get; set; } = new List<Report>();
        public virtual ICollection<PointTransaction> PointTransactions { get; set; } = new List<PointTransaction>();
        public virtual ICollection<UserAchievement> Achievements { get; set; } = new List<UserAchievement>();
    }

    public class UserAchievement
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string AchievementName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime EarnedAt { get; set; } = DateTime.Now;
        
        public virtual User? User { get; set; }
    }

    public class PointTransaction
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int Amount { get; set; }
        public string Type { get; set; } = string.Empty; // "Post", "Comment", "Session", "Request", "Rating", "Bonus"
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public virtual User? User { get; set; }
    }
}
