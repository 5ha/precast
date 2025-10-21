namespace PrecastTracker.Contracts.DTOs.RequestResponse;

public class SaveTestSetDayDataRequest
{
    public int TestSetDayId { get; set; }
    public DateTime DateTested { get; set; }
    public string? Comments { get; set; }
    public List<TestCylinderBreakInput> CylinderBreaks { get; set; } = new();
}

public class TestCylinderBreakInput
{
    public int TestCylinderId { get; set; }
    public int BreakPsi { get; set; }
}
