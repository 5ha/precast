using Microsoft.EntityFrameworkCore;
using PrecastTracker.Data.Entities;

namespace PrecastTracker.Data;

public class ApplicationDbContext : DbContext
{
    // Lazy loading is explicitly disabled for this application
    // Use explicit Include() statements when related data is needed
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
        // Ensure lazy loading is disabled
        ChangeTracker.LazyLoadingEnabled = false;
    }

    public DbSet<ConcreteTest> ConcreteTests { get; set; }
    public DbSet<MixDesign> MixDesigns { get; set; }
    public DbSet<Job> Jobs { get; set; }
    public DbSet<Bed> Beds { get; set; }
    public DbSet<Pour> Pours { get; set; }
    public DbSet<Placement> Placements { get; set; }
    public DbSet<BedStatusLog> BedStatusLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Configurations auto loaded, no need to add manually
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
