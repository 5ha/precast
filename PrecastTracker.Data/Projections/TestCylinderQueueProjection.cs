namespace PrecastTracker.Data.Projections;

public class TestCylinderQueueProjection
{
    public string TestCylinderCode { get; set; } = string.Empty;
    public string? OvenId { get; set; }
    public int DayNum { get; set; }
    public DateTime CastDate { get; set; } // The date from ProductionDay.Date
    public TimeSpan? CastTime { get; set; } // The time from PlacementStartTime
    public string JobCode { get; set; } = string.Empty;
    public string JobName { get; set; } = string.Empty;
    public string MixDesignCode { get; set; } = string.Empty;
    public int RequiredPsi { get; set; }
    public string PieceType { get; set; } = string.Empty;
    public int TestSetId { get; set; }
    public int TestSetDayId { get; set; }
    public DateTime DateDue { get; set; }
}
