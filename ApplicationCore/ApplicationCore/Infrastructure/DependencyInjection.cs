using ApplicationCore.Infrastructure.Bus;
using ApplicationCore.Infrastructure.UI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ApplicationCore.Infrastructure;

public static class DependencyInjection {

    public static IServiceCollection AddApplicationInfrastructure(this IServiceCollection services, IConfiguration configuration) {

        var cacheConfig = configuration.GetRequiredSection("Cache").Get<CacheConfiguration>();

        if (cacheConfig is null || !cacheConfig.UseLocalCache) {
            services.AddSingleton<IBus, MediatRBus>();
        } else {
            services.Configure<CacheConfiguration>(configuration.GetRequiredSection("Cache"));
            services.AddSingleton<MediatRBus>();
            services.AddSingleton<IBus, CachedBusDecorator>();
        }

        services.AddSingleton<IUIBus, UIBus>();

        SqlMapping.AddSqlMaps();

        return services;

    }


}
