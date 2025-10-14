using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrecastTracker.Data.Entities;

namespace PrecastTracker.Data.Configurations;

public class ConcreteTestConfiguration : IEntityTypeConfiguration<ConcreteTest>
{
    public void Configure(EntityTypeBuilder<ConcreteTest> builder)
    {
        builder.HasKey(ct => ct.ConcreteTestId);

        builder.Property(ct => ct.BreakPsi)
            .IsRequired();

        builder.HasOne(ct => ct.TestSet)
            .WithMany(ts => ts.ConcreteTests)
            .HasForeignKey(ct => ct.TestSetId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
