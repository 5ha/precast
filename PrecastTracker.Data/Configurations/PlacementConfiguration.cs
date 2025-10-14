using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrecastTracker.Data.Entities;

namespace PrecastTracker.Data.Configurations;

public class PlacementConfiguration : IEntityTypeConfiguration<Placement>
{
    public void Configure(EntityTypeBuilder<Placement> builder)
    {
        builder.HasKey(p => p.PlacementId);

        builder.Property(p => p.PieceType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.StartTime)
            .IsRequired();

        builder.Property(p => p.Volume)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(p => p.OvenId)
            .HasMaxLength(50);

        // Simple foreign key to Pour
        builder.HasOne(p => p.Pour)
            .WithMany(pour => pour.Placements)
            .HasForeignKey(p => p.PourId)
            .OnDelete(DeleteBehavior.Restrict);

        // Simple foreign key to MixBatch
        builder.HasOne(p => p.MixBatch)
            .WithMany(mb => mb.Placements)
            .HasForeignKey(p => p.MixBatchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.TestSets)
            .WithOne(ts => ts.Placement)
            .HasForeignKey(ts => ts.PlacementId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
