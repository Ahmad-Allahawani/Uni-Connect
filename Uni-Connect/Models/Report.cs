using System.ComponentModel.DataAnnotations;

namespace Uni_Connect.Models
{
    public class Report
    {
        public int ReportID { get; set; }
        public int ReporterID { get; set; }
        public int? TargetID { get; set; } // Can be Post or Answer

        [Required, StringLength(100)]
        public string TargetType { get; set; } // "Post", "Answer"

        [Required, MaxLength(500)]
        public string Reason { get; set; }

        public bool IsResolved { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;

        // Navigation
        public User Reporter { get; set; }
        public Post Post { get; set; }
    }
}
