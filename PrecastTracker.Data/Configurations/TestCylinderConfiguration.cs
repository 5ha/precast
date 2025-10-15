using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrecastTracker.Data.Entities;

namespace PrecastTracker.Data.Configurations;

public class TestCylinderConfiguration : IEntityTypeConfiguration<TestCylinder>
{
    public void Configure(EntityTypeBuilder<TestCylinder> builder)
    {
        builder.HasKey(tc => tc.TestCylinderId);

        builder.Property(tc => tc.Code)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasOne(tc => tc.TestSetDay)
            .WithMany(tsd => tsd.TestCylinders)
            .HasForeignKey(tc => tc.TestSetDayId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
