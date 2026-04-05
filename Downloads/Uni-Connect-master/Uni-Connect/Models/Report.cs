namespace Uni_Connect.Models
{
    public class Report
    {
        public int ReportID { get; set; }
        public int ReporterID { get; set; }
        public int PostID { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; } // Pending, Reviewed
        public bool IsDeleted { get; set; } = false;

        // Navigation
        public User Reporter { get; set; }
        public Post Post { get; set; }
    }
}
