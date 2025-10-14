namespace PrecastTracker.Data.Entities;

public class ConcreteTest
{
    public int ConcreteTestId { get; set; }
    public int BreakPsi { get; set; } // Compression strength result (PSI)

    // Foreign key
    public int TestSetId { get; set; }

    // Navigation property
    public TestSet TestSet { get; set; } = null!;
}
