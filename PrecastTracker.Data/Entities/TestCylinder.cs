namespace PrecastTracker.Data.Entities;

public class TestCylinder
{
    public int TestCylinderId { get; set; }
    public DateTime? DateTested { get; set; } // The date we actually ran the test
    public int? BreakPsi { get; set; } // Compression strength result (PSI) - nullable until tested

    // Foreign key
    public int TestSetDayId { get; set; }

    // Navigation property
    public TestSetDay TestSetDay { get; set; } = null!;
}
