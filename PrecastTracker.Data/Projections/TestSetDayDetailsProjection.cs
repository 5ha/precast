namespace PrecastTracker.Data.Projections;

public class TestSetDayDetailsProjection
{
    // TestSetDay information
    public int TestSetDayId { get; set; }
    public int DayNum { get; set; }
    public string? Comments { get; set; }
    public DateTime DateDue { get; set; }
    public DateTime? DateTested { get; set; }

    // Context information from related entities
    public string JobCode { get; set; } = string.Empty;
    public string JobName { get; set; } = string.Empty;
    public string MixDesignCode { get; set; } = string.Empty;
    public int RequiredPsi { get; set; }
    public string PieceType { get; set; } = string.Empty;
    public DateTime CastDate { get; set; }
    public TimeSpan? CastTime { get; set; }

    // Test cylinders for this test set day
    public List<TestCylinderBreakProjection> TestCylinders { get; set; } = new();
}

public class TestCylinderBreakProjection
{
    public int TestCylinderId { get; set; }
    public string Code { get; set; } = string.Empty;
    public int? BreakPsi { get; set; }
}
