using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrecastTracker.Data.Entities;

namespace PrecastTracker.Data.Configurations;

public class MixDesignRequirementConfiguration : IEntityTypeConfiguration<MixDesignRequirement>
{
    public void Configure(EntityTypeBuilder<MixDesignRequirement> builder)
    {
        builder.HasKey(mdr => mdr.MixDesignRequirementId);

        builder.Property(mdr => mdr.TestType)
            .IsRequired();

        builder.Property(mdr => mdr.RequiredPsi)
            .IsRequired();

        builder.HasOne(mdr => mdr.MixDesign)
            .WithMany(md => md.MixDesignRequirements)
            .HasForeignKey(mdr => mdr.MixDesignId)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique constraint: A MixDesign can only have one requirement per TestType
        builder.HasIndex(mdr => new { mdr.MixDesignId, mdr.TestType })
            .IsUnique();
    }
}
