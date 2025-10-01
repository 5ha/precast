using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PrecastTracker.Data;

/// <summary>
/// This factory is used by EF Core tools for migrations
/// e.g. when adding or removing migrations
/// The database update happens when the application runs if automatic update is enabled
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        // Use SQLite with a connection string pointing to the App_Data directory in the solution root
        // We can hard code this because we only use this in the development environment and never anywhere else
        // This class is called by the EF tool so very difficult getting this information from config anyway
        optionsBuilder.UseSqlite("Data Source=../App_Data/PrecastTracker.db");

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
