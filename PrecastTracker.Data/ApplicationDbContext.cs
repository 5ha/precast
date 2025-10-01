using Microsoft.EntityFrameworkCore;

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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Configurations auto loaded, no need to add manually
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
