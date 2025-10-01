namespace PrecastTracker.Data.Entities;

public class Bed
{
    public int BedId { get; set; }
    public string Code { get; set; } = string.Empty; // e.g., "16", "2", "13"

    // Navigation properties
    public ICollection<Pour> Pours { get; set; } = new List<Pour>();
}
