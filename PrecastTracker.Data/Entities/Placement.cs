namespace PrecastTracker.Data.Entities;

public class Placement
{
    public int PlacementId { get; set; }
    public string PieceType { get; set; } = string.Empty; // e.g., "Walls", "Tees", "Slabs"
    public TimeSpan StartTime { get; set; } // Time when concrete placement began (date comes from MixBatch.ProductionDay)
    public decimal Volume { get; set; } // Volume in cubic yards
    public string? OvenId { get; set; }

    // Foreign keys
    public int PourId { get; set; }
    public int MixBatchId { get; set; }

    // Navigation properties
    public Pour Pour { get; set; } = null!;
    public MixBatch MixBatch { get; set; } = null!;
    public ICollection<TestSet> TestSets { get; set; } = new List<TestSet>();
    public ICollection<Delivery> Deliveries { get; set; } = new List<Delivery>();
}
