namespace UniConnect.Web.Models
{
    public class PrivateSession
    {
        public int Id { get; set; }
        public int? RequestId { get; set; }
        public string StudentId { get; set; } = string.Empty;
        public string TutorId { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime ScheduledAt { get; set; }
        public int DurationMinutes { get; set; } = 60;
        public string Status { get; set; } = "Pending"; // "Pending", "Active", "Completed", "Cancelled"
        public int PointsEarned { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        
        public virtual AcademicRequest? Request { get; set; }
        public virtual User? Student { get; set; }
        public virtual User? Tutor { get; set; }
        public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
        public virtual ICollection<SessionRating> Ratings { get; set; } = new List<SessionRating>();
    }

    public class SessionRating
    {
        public int Id { get; set; }
        public int SessionId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int Rating { get; set; } // 1-5
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public virtual PrivateSession? Session { get; set; }
        public virtual User? User { get; set; }
    }
}
