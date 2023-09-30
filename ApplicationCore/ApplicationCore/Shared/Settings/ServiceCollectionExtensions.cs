using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ApplicationCore.Shared.Settings;

public static class ServiceCollectionExtensions {
    public static IServiceCollection ConfigureWritable<T>(this IServiceCollection services, IConfigurationSection section, string file = "appsettings.json") where T : class, new() {

        return services
            .Configure<T>(section)
            .AddTransient<IWritableOptions<T>>(provider => {
                var options = provider.GetRequiredService<IOptionsMonitor<T>>();
                return new WritableOptions<T>(options, section.Key, file);
            });

    }
}
