using ApplicationCore.Features.Configuration;
using ApplicationCore.Infrastructure.Bus;
using ApplicationCore.Shared.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ApplicationCore.Features.DataFilePaths;

public static class DependencyInjection {

    public static IServiceCollection AddConfiguration(this IServiceCollection services) {

        services.AddTransient<Shared.Data.DataFilePaths.GetConfiguration>(s => {
            var bus = s.GetRequiredService<IBus>();
            var options = s.GetRequiredService<IOptions<ConfigurationFiles>>();
            return async () => {
                var result = await bus.Send(new GetDataFilePaths.Query(options.Value.DataConfigFile));
                return result.Match<Shared.Data.DataFilePaths?>(
                    config => config,
                    error => null);
            };
        });

        return services.AddTransient<DataFilePathsEditorViewModel>();
    }

}