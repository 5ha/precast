using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrecastTracker.Data.Entities;

namespace PrecastTracker.Data.Configurations;

public class JobConfiguration : IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> builder)
    {
        builder.HasKey(j => j.JobId);

        builder.Property(j => j.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(j => j.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(j => j.Code)
            .IsUnique();

        builder.HasMany(j => j.Pours)
            .WithOne(p => p.Job)
            .HasForeignKey(p => p.JobId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
