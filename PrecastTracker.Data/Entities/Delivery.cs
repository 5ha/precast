namespace PrecastTracker.Data.Entities;

public class Delivery
{
    public int DeliveryId { get; set; }
    public string TruckId { get; set; } = string.Empty; // e.g., "3", "6", "7"

    // Foreign key - changed to Placement since truck numbers are placement-specific in the data
    public int PlacementId { get; set; }

    // Navigation property
    public Placement Placement { get; set; } = null!;
}
