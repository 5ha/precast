using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrecastTracker.Data.Entities;

namespace PrecastTracker.Data.Configurations;

public class MixDesignConfiguration : IEntityTypeConfiguration<MixDesign>
{
    public void Configure(EntityTypeBuilder<MixDesign> builder)
    {
        builder.HasKey(m => m.MixDesignId);

        builder.Property(m => m.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(m => m.Code)
            .IsUnique();
    }
}
