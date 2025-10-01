using Microsoft.Extensions.DependencyInjection;

namespace PrecastTracker.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.Scan(s => s
            .FromAssembliesOf(typeof(IService))
            .AddClasses(c => c.AssignableTo<IService>()) // only classes implementing IService
                .AsImplementedInterfaces()
                .WithScopedLifetime());

        return services;
    }
}
