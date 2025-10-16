namespace PrecastTracker.Data.Entities;

public class TestCylinder
{
    public int TestCylinderId { get; set; }
    public string Code { get; set; } = string.Empty; // What was written on the cylinder (e.g., "12345-7-25-020")
    public int? BreakPsi { get; set; } // Compression strength result (PSI) - nullable until tested

    // Foreign key
    public int TestSetDayId { get; set; }

    // Navigation property
    public TestSetDay TestSetDay { get; set; } = null!;
}
