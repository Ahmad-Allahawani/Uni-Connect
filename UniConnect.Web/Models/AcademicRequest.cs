namespace UniConnect.Web.Models
{
    public class AcademicRequest
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public int PointsOffered { get; set; }
        public string Status { get; set; } = "Open";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        
        public virtual User? User { get; set; }
        public virtual Category? Category { get; set; }
        public virtual ICollection<Proposal> Proposals { get; set; } = new List<Proposal>();
    }

    public class Proposal
    {
        public int Id { get; set; }
        public int RequestId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public virtual AcademicRequest? Request { get; set; }
        public virtual User? User { get; set; }
    }
}
