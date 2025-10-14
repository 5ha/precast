using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrecastTracker.Data.Entities;

namespace PrecastTracker.Data.Configurations;

public class BedConfiguration : IEntityTypeConfiguration<Bed>
{
    public void Configure(EntityTypeBuilder<Bed> builder)
    {
        builder.HasKey(b => b.BedId);

        // BedId is manually set from CSV data - configure to not use auto-increment
        builder.Property(b => b.BedId)
            .ValueGeneratedNever();

        // Don't configure Pours relationship here - it's configured in PourConfiguration
    }
}
