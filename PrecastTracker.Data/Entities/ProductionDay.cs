namespace PrecastTracker.Data.Entities;

public class ProductionDay
{
    public int ProductionDayId { get; set; }
    public DateTime Date { get; set; }

    // Navigation properties
    public ICollection<MixBatch> MixBatches { get; set; } = new List<MixBatch>();
}
