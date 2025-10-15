namespace PrecastTracker.Data.Entities;

public class TestCylinder
{
    public int TestCylinderId { get; set; }
    public int TestType { get; set; } // 1, 7, or 28 days
    public DateTime? TestingDate { get; set; } // When the cylinder is/was crushed
    public string? Comments { get; set; }
    public int? BreakPsi { get; set; } // Compression strength result (PSI) - nullable until tested

    // Foreign key
    public int TestSetId { get; set; }

    // Navigation property
    public TestSet TestSet { get; set; } = null!;
}
