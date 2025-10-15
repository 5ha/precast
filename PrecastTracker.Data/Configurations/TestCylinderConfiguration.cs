using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrecastTracker.Data.Entities;

namespace PrecastTracker.Data.Configurations;

public class TestCylinderConfiguration : IEntityTypeConfiguration<TestCylinder>
{
    public void Configure(EntityTypeBuilder<TestCylinder> builder)
    {
        builder.HasKey(tc => tc.TestCylinderId);

        builder.Property(tc => tc.TestType)
            .IsRequired();

        builder.Property(tc => tc.Comments)
            .HasMaxLength(500);

        builder.HasOne(tc => tc.TestSet)
            .WithMany(ts => ts.TestCylinders)
            .HasForeignKey(tc => tc.TestSetId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
