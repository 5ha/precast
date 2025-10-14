namespace PrecastTracker.Data.Entities;

public class MixDesignRequirement
{
    public int MixDesignRequirementId { get; set; }
    public int TestType { get; set; } // 1, 7, or 28 days
    public int RequiredPsi { get; set; }

    // Foreign key
    public int MixDesignId { get; set; }

    // Navigation property
    public MixDesign MixDesign { get; set; } = null!;
}
