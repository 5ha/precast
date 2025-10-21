namespace PrecastTracker.Data.Entities;

public class TestSetDay
{
    public int TestSetDayId { get; set; }
    public int DayNum { get; set; } // 1, 7, or 28 days
    public string? Comments { get; set; }
    public DateTime DateDue { get; set; } // Scheduled testing date: ProductionDay.Date + DayNum
    public DateTime? DateTested { get; set; } // The date we actually ran the test

    // Foreign key
    public int TestSetId { get; set; }

    // Navigation properties
    public TestSet TestSet { get; set; } = null!;
    public ICollection<TestCylinder> TestCylinders { get; set; } = new List<TestCylinder>();
}
