using ApplicationCore.Features.CNC.Tools.Contracts;
using ApplicationCore.Infrastructure.Bus;
using ApplicationCore.Shared.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ApplicationCore.Features.CNC.Tools;

public static class DependencyInjection {

    public static IServiceCollection AddToolEditor(this IServiceCollection services) {

        services.AddTransient<ToolFileEditorViewModel>();

        services.AddTransient<CNCToolBox.GetToolCarousels>(s => {

            var bus = s.GetRequiredService<IBus>();
            var options = s.GetRequiredService<IOptions<ConfigurationFiles>>();
            return async () => {
                var result = await bus.Send(new GetTools.Query(options.Value.ToolConfigFile));

                return result.Match(
                   toolMap => toolMap.Select(map => new ToolCarousel() {
                       MachineName = map.MachineName,
                       PositionCount = map.ToolPositionCount,
                       Tools = map.Tools.Select(t => new Tool(t.Name, t.Position, t.AlternativeNames.ToArray())).ToArray()
                   }),
                   error => Enumerable.Empty<ToolCarousel>());
            };

        });

        return services;

    }

}
