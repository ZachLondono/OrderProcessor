using ApplicationCore.Infrastructure.Bus;
using ApplicationCore.Infrastructure.Data;
using ApplicationCore.Infrastructure.UI;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ApplicationCore.Infrastructure;

public static class DependencyInjection {

    public static IServiceCollection AddApplicationInfrastructure(this IServiceCollection services, IConfiguration configuration) {

        var cacheConfig = configuration.GetRequiredSection("Cache").Get<CacheConfiguration>();

        if (cacheConfig is null || !cacheConfig.UseLocalCache) {
            services.AddSingleton<IBus, MediatRBus>();
        } else {
            services.AddSingleton(cacheConfig);
            services.AddSingleton<MediatRBus>();
            services.AddSingleton<IBus, CachedBusDecorator>();
        }

        services.AddSingleton<IUIBus, UIBus>();

        SqlMapper.RemoveTypeMap(typeof(decimal));
        SqlMapper.AddTypeHandler(new SqliteDecimalTypeHandler());
        SqlMapper.AddTypeHandler(new SqliteDimensionTypeHandler());
        SqlMapper.AddTypeHandler(new SqliteDictionaryEnumerableTypeHandler());
        SqlMapper.AddTypeHandler(new SqliteGuidTypeHandler());
        SqlMapper.AddTypeHandler(new DimensionArrayTypeHandler());
        SqlMapper.RemoveTypeMap(typeof(Guid));
        SqlMapper.RemoveTypeMap(typeof(Guid?));

        return services;

    }

}