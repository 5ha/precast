namespace PrecastTracker.Contracts.DTOs.RequestResponse;

public class UntestedPlacementResponse
{
    public int PourId { get; set; }
    public int PlacementId { get; set; }
    public DateTime CastDate { get; set; }
    public TimeSpan? CastTime { get; set; }
    public string JobCode { get; set; } = string.Empty;
    public string JobName { get; set; } = string.Empty;
    public string MixDesignCode { get; set; } = string.Empty;
    public string PieceType { get; set; } = string.Empty;
    public decimal Volume { get; set; }
}
