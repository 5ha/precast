using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrecastTracker.Data.Entities;

namespace PrecastTracker.Data.Configurations;

public class DeliveryConfiguration : IEntityTypeConfiguration<Delivery>
{
    public void Configure(EntityTypeBuilder<Delivery> builder)
    {
        builder.HasKey(d => d.DeliveryId);

        builder.Property(d => d.TruckId)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasOne(d => d.Placement)
            .WithMany(p => p.Deliveries)
            .HasForeignKey(d => d.PlacementId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
