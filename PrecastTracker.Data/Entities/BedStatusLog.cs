namespace PrecastTracker.Data.Entities;

public class BedStatusLog
{
    public int BedStatusLogId { get; set; }

    // Foreign key
    public int BedId { get; set; }

    // Status transition
    public BedStatus? FromStatus { get; set; } // Null when bed first created
    public BedStatus ToStatus { get; set; }
    public DateTime ChangedDate { get; set; }
    public string? Reason { get; set; } // Free text: "Form repair - bolt holes stripped", "Surface degradation beyond repair", etc.

    // Navigation properties
    public Bed Bed { get; set; } = null!;
}
