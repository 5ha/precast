using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PrecastTracker.Data.Repositories;

namespace PrecastTracker.Data;

public static class DependencyInjection
{
    public static IServiceCollection AddData(this IServiceCollection services, IConfiguration cfg)
    {
        services.AddLocalDataServices(cfg);
        services.AddRepositoryServices();
        return services;
    }

    private static IServiceCollection AddLocalDataServices(this IServiceCollection services, IConfiguration cfg)
    {
        services.AddDbContext<ApplicationDbContext>(o =>
            o.UseSqlite(cfg.GetConnectionString("Default")));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    private static IServiceCollection AddRepositoryServices(this IServiceCollection services)
    {
        services.Scan(s => s
            .FromAssembliesOf(typeof(IRepository))
            .AddClasses(c => c.AssignableTo<IRepository>()) // only classes implementing IRepository
                .AsImplementedInterfaces()
                .WithScopedLifetime());

        return services;
    }
}
