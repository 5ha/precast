namespace PrecastTracker.Data.Entities;

public class TestSetDay
{
    public int TestSetDayId { get; set; }
    public int DayNum { get; set; } // 1, 7, or 28 days
    public bool IsComplete { get; set; } // Marks when all cylinders for this day are tested in UI
    public string? Comments { get; set; }

    // Foreign key
    public int TestSetId { get; set; }

    // Navigation properties
    public TestSet TestSet { get; set; } = null!;
    public ICollection<TestCylinder> TestCylinders { get; set; } = new List<TestCylinder>();
}
