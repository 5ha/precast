namespace PrecastTracker.Data.Entities;

public class Placement
{
    public int PlacementId { get; set; }
    public decimal YardsPerBed { get; set; }
    public TimeSpan? BatchingStartTime { get; set; }
    public string? TruckNumbers { get; set; } // e.g., "1, 2, 3, 4"
    public string? PieceType { get; set; } // e.g., "Walls", "Tees", "Slabs"
    public string? OvenId { get; set; }

    // Foreign keys
    public int PourId { get; set; }
    public int MixDesignId { get; set; }

    // Navigation properties
    public Pour Pour { get; set; } = null!;
    public MixDesign MixDesign { get; set; } = null!;
    public ICollection<ConcreteTest> ConcreteTests { get; set; } = new List<ConcreteTest>();
}
