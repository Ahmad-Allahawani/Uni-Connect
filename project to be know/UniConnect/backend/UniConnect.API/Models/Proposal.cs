namespace UniConnect.API.Models;

public class Proposal
{
    public int Id { get; set; }
    public int AcademicRequestId { get; set; }
    public int UserId { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsAccepted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public AcademicRequest? AcademicRequest { get; set; }
    public User? User { get; set; }
}
