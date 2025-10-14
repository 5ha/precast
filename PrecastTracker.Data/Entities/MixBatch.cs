namespace PrecastTracker.Data.Entities;

public class MixBatch
{
    public int MixBatchId { get; set; }

    // Foreign keys
    public int ProductionDayId { get; set; }
    public int MixDesignId { get; set; }

    // Navigation properties
    public ProductionDay ProductionDay { get; set; } = null!;
    public MixDesign MixDesign { get; set; } = null!;
    public ICollection<Placement> Placements { get; set; } = new List<Placement>();
}
