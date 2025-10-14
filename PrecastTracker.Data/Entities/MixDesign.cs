namespace PrecastTracker.Data.Entities;

public class MixDesign
{
    public int MixDesignId { get; set; }
    public string Code { get; set; } = string.Empty; // e.g., "824.1", "2515.11"

    // Navigation properties
    public ICollection<MixDesignRequirement> MixDesignRequirements { get; set; } = new List<MixDesignRequirement>();
    public ICollection<MixBatch> MixBatches { get; set; } = new List<MixBatch>();
}
