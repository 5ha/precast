using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrecastTracker.Data.Entities;

namespace PrecastTracker.Data.Configurations;

public class TestSetDayConfiguration : IEntityTypeConfiguration<TestSetDay>
{
    public void Configure(EntityTypeBuilder<TestSetDay> builder)
    {
        builder.HasKey(tsd => tsd.TestSetDayId);

        builder.Property(tsd => tsd.DayNum)
            .IsRequired();

        builder.Property(tsd => tsd.IsComplete)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(tsd => tsd.DateDue)
            .IsRequired();

        builder.Property(tsd => tsd.Comments)
            .HasMaxLength(500);

        builder.HasOne(tsd => tsd.TestSet)
            .WithMany(ts => ts.TestSetDays)
            .HasForeignKey(tsd => tsd.TestSetId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(tsd => tsd.TestCylinders)
            .WithOne(tc => tc.TestSetDay)
            .HasForeignKey(tc => tc.TestSetDayId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
