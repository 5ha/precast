using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrecastTracker.Data.Entities;

namespace PrecastTracker.Data.Configurations;

public class BedConfiguration : IEntityTypeConfiguration<Bed>
{
    public void Configure(EntityTypeBuilder<Bed> builder)
    {
        builder.HasKey(b => b.BedId);

        builder.Property(b => b.BedId);

        builder.Property(b => b.Status)
            .HasConversion<string>()
            .IsRequired();
    }
}
