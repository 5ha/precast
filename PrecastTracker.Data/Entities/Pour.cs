namespace PrecastTracker.Data.Entities;

public class Pour
{
    public int PourId { get; set; }
    public string Code { get; set; } = string.Empty; // e.g., "6455", "6530"
    public DateTime CastingDate { get; set; }

    // Foreign keys
    public int JobId { get; set; }
    public int BedId { get; set; }

    // Navigation properties
    public Job Job { get; set; } = null!;
    public Bed Bed { get; set; } = null!;
    public ICollection<Placement> Placements { get; set; } = new List<Placement>();
}
