using System.ComponentModel.DataAnnotations;

namespace Uni_Connect.Models
{
    public class Report
    {
        public int ReportID { get; set; }

        public int ReporterID { get; set; }

        public int TargetID { get; set; }

        [Required, StringLength(100)]
        public string TargetType { get; set; } = string.Empty;

        [Required, MaxLength(500)]
        public string Reason { get; set; } = string.Empty;

        public bool IsResolved { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;

        public User? Reporter { get; set; }
    }
}
