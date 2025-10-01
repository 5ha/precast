namespace PrecastTracker.Data.Entities;

public class ConcreteTest
{
    public int ConcreteTestId { get; set; }
    public string TestCode { get; set; } = string.Empty; // e.g., "9005", "9005.1"
    public string CylinderId { get; set; } = string.Empty; // e.g., "7C", "28C", "1C"
    public DateTime? TestingDate { get; set; }
    public int RequiredPsi { get; set; }
    public int? Break1 { get; set; }
    public int? Break2 { get; set; }
    public int? Break3 { get; set; }
    public string? Comments { get; set; }

    // Foreign key
    public int PlacementId { get; set; }

    // Navigation property
    public Placement Placement { get; set; } = null!;
}
