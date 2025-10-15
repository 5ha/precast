using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrecastTracker.Data.Entities;

namespace PrecastTracker.Data.Configurations;

public class TestSetConfiguration : IEntityTypeConfiguration<TestSet>
{
    public void Configure(EntityTypeBuilder<TestSet> builder)
    {
        builder.HasKey(ts => ts.TestSetId);

        builder.HasOne(ts => ts.Placement)
            .WithMany(p => p.TestSets)
            .HasForeignKey(ts => ts.PlacementId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(ts => ts.TestSetDays)
            .WithOne(tsd => tsd.TestSet)
            .HasForeignKey(tsd => tsd.TestSetId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
