namespace PrecastTracker.Data.Entities;

public class Job
{
    public int JobId { get; set; }
    public string Code { get; set; } = string.Empty; // e.g., "25-020"
    public string Name { get; set; } = string.Empty; // e.g., "Woodbury HS"

    // Navigation properties
    public ICollection<Pour> Pours { get; set; } = new List<Pour>();
}
