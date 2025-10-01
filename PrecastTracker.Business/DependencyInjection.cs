using Microsoft.Extensions.DependencyInjection;
using PrecastTracker.Business.Core;

namespace PrecastTracker.Business;

public static class DependencyInjection
{
    public static IServiceCollection AddBusiness(this IServiceCollection services)
    {
        services.Scan(s => s
            .FromAssembliesOf(typeof(IBusiness))
            .AddClasses(c => c.AssignableTo<IBusiness>()) // only classes implementing IBusiness
                .AsImplementedInterfaces()
                .WithScopedLifetime());

        return services;
    }
}
