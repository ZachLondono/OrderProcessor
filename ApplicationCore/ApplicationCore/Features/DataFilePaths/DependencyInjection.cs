using ApplicationCore.Features.Shared.Settings;
using ApplicationCore.Infrastructure.Bus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ApplicationCore.Features.Configuration;

public static class DependencyInjection {

    public static IServiceCollection AddConfiguration(this IServiceCollection services) {

        services.AddTransient<DataFilePaths.GetConfiguration>(s => {
            var bus = s.GetRequiredService<IBus>();
            var options = s.GetRequiredService<IOptions<ConfigurationFiles>>();
            return async () => {
                var result = await bus.Send(new GetDataFilePaths.Query(options.Value.DataConfigFile));
                return result.Match(
                    config => config,
                    error => (DataFilePaths?)null);
            };
        });

        return services.AddTransient<DataFilePathsEditorViewModel>();
    }

}