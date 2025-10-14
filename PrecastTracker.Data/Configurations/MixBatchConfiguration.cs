using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrecastTracker.Data.Entities;

namespace PrecastTracker.Data.Configurations;

public class MixBatchConfiguration : IEntityTypeConfiguration<MixBatch>
{
    public void Configure(EntityTypeBuilder<MixBatch> builder)
    {
        builder.HasKey(mb => mb.MixBatchId);

        builder.HasOne(mb => mb.ProductionDay)
            .WithMany(pd => pd.MixBatches)
            .HasForeignKey(mb => mb.ProductionDayId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(mb => mb.MixDesign)
            .WithMany(md => md.MixBatches)
            .HasForeignKey(mb => mb.MixDesignId)
            .OnDelete(DeleteBehavior.Restrict);

        // Placements relationship is configured in PlacementConfiguration
        // Deliveries are now linked to Placement, not MixBatch
    }
}
