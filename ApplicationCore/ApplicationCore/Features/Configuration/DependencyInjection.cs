using ApplicationCore.Infrastructure.Bus;
using Microsoft.Extensions.DependencyInjection;

namespace ApplicationCore.Features.Configuration;

public static class DependencyInjection {

    public static IServiceCollection AddConfiguration(this IServiceCollection services) {

        services.AddTransient<AppConfiguration.GetConfiguration>(s => {
            var bus = s.GetRequiredService<IBus>();
            return async () => {
                var result = await bus.Send(new GetConfiguration.Query(ConfigurationEditorViewModel.FILE_PATH));
                return result.Match(
                    config => config,
                    error => (AppConfiguration?)null);
            };
        });

        return services.AddTransient<ConfigurationEditorViewModel>();
    }

}