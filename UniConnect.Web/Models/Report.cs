namespace UniConnect.Web.Models
{
    public class Report
    {
        public int Id { get; set; }
        public string ReporterId { get; set; } = string.Empty;
        public string ReportedUserId { get; set; } = string.Empty;
        public string ReportType { get; set; } = string.Empty; // "Post", "Comment", "User", "Session", "Message"
        public int? ContentId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending"; // "Pending", "Reviewed", "Resolved", "Dismissed"
        public string? Resolution { get; set; }
        public string? ReviewedById { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? ReviewedAt { get; set; }
        
        public virtual User? Reporter { get; set; }
        public virtual User? ReportedUser { get; set; }
        public virtual User? ReviewedBy { get; set; }
    }
}
