namespace PrecastTracker.Data.Entities;

public class TestSet
{
    public int TestSetId { get; set; }
    public int TestType { get; set; } // 1, 7, or 28 days
    public DateTime? TestingDate { get; set; } // When the cylinders are/were crushed
    public string? Comments { get; set; }

    // Foreign key
    public int PlacementId { get; set; }

    // Navigation property
    public Placement Placement { get; set; } = null!;
    public ICollection<TestCylinder> TestCylinders { get; set; } = new List<TestCylinder>();
}
