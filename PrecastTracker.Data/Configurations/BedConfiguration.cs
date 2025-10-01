using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrecastTracker.Data.Entities;

namespace PrecastTracker.Data.Configurations;

public class BedConfiguration : IEntityTypeConfiguration<Bed>
{
    public void Configure(EntityTypeBuilder<Bed> builder)
    {
        builder.HasKey(b => b.BedId);

        builder.Property(b => b.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(b => b.Code)
            .IsUnique();

        builder.HasMany(b => b.Pours)
            .WithOne(p => p.Bed)
            .HasForeignKey(p => p.BedId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
