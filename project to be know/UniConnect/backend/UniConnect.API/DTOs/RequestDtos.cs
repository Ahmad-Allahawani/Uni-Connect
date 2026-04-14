namespace UniConnect.API.DTOs;

public class AcademicRequestDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Faculty { get; set; }
    public int Course { get; set; }
    public string Topic { get; set; } = string.Empty;
    public int PointsOffered { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateAcademicRequestDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Faculty { get; set; }
    public int Course { get; set; }
    public string Topic { get; set; } = string.Empty;
    public int PointsOffered { get; set; }
}

public class ProposalDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsAccepted { get; set; }
}

public class CreateProposalDto
{
    public string Message { get; set; } = string.Empty;
}
