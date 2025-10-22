namespace PrecastTracker.Data.Entities;

public class Bed
{
    public int BedId { get; set; }
    public BedStatus Status { get; set; } = BedStatus.Active;

    // Navigation properties
    public ICollection<Pour> Pours { get; set; } = new List<Pour>();
}
