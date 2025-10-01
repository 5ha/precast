using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrecastTracker.Data.Entities;

namespace PrecastTracker.Data.Configurations;

public class ConcreteTestConfiguration : IEntityTypeConfiguration<ConcreteTest>
{
    public void Configure(EntityTypeBuilder<ConcreteTest> builder)
    {
        builder.HasKey(ct => ct.ConcreteTestId);

        builder.Property(ct => ct.TestCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(ct => ct.CylinderId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(ct => ct.Comments)
            .HasMaxLength(500);

        builder.HasOne(ct => ct.Placement)
            .WithMany(p => p.ConcreteTests)
            .HasForeignKey(ct => ct.PlacementId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
