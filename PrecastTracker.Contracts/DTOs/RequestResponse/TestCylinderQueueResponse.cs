namespace PrecastTracker.Contracts.DTOs.RequestResponse;

public class TestCylinderQueueResponse
{
    public string TestCylinderCode { get; set; } = string.Empty;
    public string? OvenId { get; set; }
    public int DayNum { get; set; }
    public DateTime CastDate { get; set; }
    public TimeSpan? CastTime { get; set; }
    public string JobCode { get; set; } = string.Empty;
    public string JobName { get; set; } = string.Empty;
    public string MixDesignCode { get; set; } = string.Empty;
    public int RequiredPsi { get; set; }
    public string PieceType { get; set; } = string.Empty;
    public int TestSetId { get; set; }
    public DateTime DateDue { get; set; }
}
