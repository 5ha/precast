using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrecastTracker.Data.Entities;

namespace PrecastTracker.Data.Configurations;

public class ProductionDayConfiguration : IEntityTypeConfiguration<ProductionDay>
{
    public void Configure(EntityTypeBuilder<ProductionDay> builder)
    {
        builder.HasKey(pd => pd.ProductionDayId);

        builder.Property(pd => pd.Date)
            .IsRequired();

        builder.HasIndex(pd => pd.Date)
            .IsUnique();

        builder.HasMany(pd => pd.MixBatches)
            .WithOne(mb => mb.ProductionDay)
            .HasForeignKey(mb => mb.ProductionDayId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
