using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrecastTracker.Data.Entities;

namespace PrecastTracker.Data.Configurations;

public class TestSetConfiguration : IEntityTypeConfiguration<TestSet>
{
    public void Configure(EntityTypeBuilder<TestSet> builder)
    {
        builder.HasKey(ts => ts.TestSetId);

        builder.Property(ts => ts.TestType)
            .IsRequired();

        builder.Property(ts => ts.Comments)
            .HasMaxLength(500);

        builder.HasOne(ts => ts.Placement)
            .WithMany(p => p.TestSets)
            .HasForeignKey(ts => ts.PlacementId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(ts => ts.ConcreteTests)
            .WithOne(ct => ct.TestSet)
            .HasForeignKey(ct => ct.TestSetId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
