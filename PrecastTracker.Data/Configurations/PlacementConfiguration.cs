using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrecastTracker.Data.Entities;

namespace PrecastTracker.Data.Configurations;

public class PlacementConfiguration : IEntityTypeConfiguration<Placement>
{
    public void Configure(EntityTypeBuilder<Placement> builder)
    {
        builder.HasKey(p => p.PlacementId);

        builder.Property(p => p.YardsPerBed)
            .HasPrecision(18, 2);

        builder.Property(p => p.TruckNumbers)
            .HasMaxLength(200);

        builder.Property(p => p.PieceType)
            .HasMaxLength(100);

        builder.Property(p => p.OvenId)
            .HasMaxLength(50);

        builder.HasOne(p => p.Pour)
            .WithMany(pour => pour.Placements)
            .HasForeignKey(p => p.PourId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.MixDesign)
            .WithMany()
            .HasForeignKey(p => p.MixDesignId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.ConcreteTests)
            .WithOne(ct => ct.Placement)
            .HasForeignKey(ct => ct.PlacementId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
