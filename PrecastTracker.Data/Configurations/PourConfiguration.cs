using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrecastTracker.Data.Entities;

namespace PrecastTracker.Data.Configurations;

public class PourConfiguration : IEntityTypeConfiguration<Pour>
{
    public void Configure(EntityTypeBuilder<Pour> builder)
    {
        builder.HasKey(p => p.PourId);

        builder.HasOne(p => p.Job)
            .WithMany(j => j.Pours)
            .HasForeignKey(p => p.JobId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Bed)
            .WithMany(b => b.Pours)
            .HasForeignKey(p => p.BedId)
            .OnDelete(DeleteBehavior.Restrict);

        // Placements relationship is configured in PlacementConfiguration
    }
}
