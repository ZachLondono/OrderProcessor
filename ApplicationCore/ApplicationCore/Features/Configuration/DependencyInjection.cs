using Microsoft.Extensions.DependencyInjection;

namespace ApplicationCore.Features.Configuration;

public static class DependencyInjection {

    public static IServiceCollection AddConfiguration(this IServiceCollection services) {
        return services.AddTransient<IConfigurationDBConnectionFactory, SqliteConfigurationDBConnectionFactory>();
    }

}