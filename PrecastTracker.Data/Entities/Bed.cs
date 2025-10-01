namespace PrecastTracker.Data.Entities;

public class Bed
{
    public int BedId { get; set; }
    public string Code { get; set; } = string.Empty; // e.g., "16", "2", "13"
    public BedStatus Status { get; set; } = BedStatus.Active;
    public string? Location { get; set; }
    public string? ConfigurationNotes { get; set; }

    // Navigation properties
    public ICollection<Pour> Pours { get; set; } = new List<Pour>();
    public ICollection<BedStatusLog> StatusLogs { get; set; } = new List<BedStatusLog>();
}
