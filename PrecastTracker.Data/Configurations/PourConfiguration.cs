using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrecastTracker.Data.Entities;

namespace PrecastTracker.Data.Configurations;

public class PourConfiguration : IEntityTypeConfiguration<Pour>
{
    public void Configure(EntityTypeBuilder<Pour> builder)
    {
        builder.HasKey(p => p.PourId);

        builder.Property(p => p.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasOne(p => p.Job)
            .WithMany(j => j.Pours)
            .HasForeignKey(p => p.JobId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Bed)
            .WithMany(b => b.Pours)
            .HasForeignKey(p => p.BedId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Placements)
            .WithOne(placement => placement.Pour)
            .HasForeignKey(placement => placement.PourId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
