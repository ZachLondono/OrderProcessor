using ApplicationCore.Features.Tools.Contracts;
using ApplicationCore.Infrastructure.Bus;
using Microsoft.Extensions.DependencyInjection;

namespace ApplicationCore.Features.Tools;

public static class DepenencyInjection {

    public static IServiceCollection AddToolEditor(this IServiceCollection services) {

        services.AddTransient<ToolFileEditorViewModel>();

        services.AddTransient<CNCToolBox.GetToolCarousels>(s => {

            var bus = s.GetRequiredService<IBus>();
            return async () => {
                var result = await bus.Send(new GetTools.Query(ToolFileEditorViewModel.FILE_PATH));

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
