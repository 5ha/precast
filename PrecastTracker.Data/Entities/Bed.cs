namespace PrecastTracker.Data.Entities;

public class Bed
{
    public int BedId { get; set; }

    // Navigation properties
    public ICollection<Pour> Pours { get; set; } = new List<Pour>();
}
