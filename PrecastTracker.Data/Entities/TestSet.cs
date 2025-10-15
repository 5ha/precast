namespace PrecastTracker.Data.Entities;

public class TestSet
{
    public int TestSetId { get; set; }

    // Foreign key
    public int PlacementId { get; set; }

    // Navigation properties
    public Placement Placement { get; set; } = null!;
    public ICollection<TestSetDay> TestSetDays { get; set; } = new List<TestSetDay>();
}
