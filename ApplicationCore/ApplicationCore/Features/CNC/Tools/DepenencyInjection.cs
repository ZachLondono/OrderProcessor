using Microsoft.Extensions.DependencyInjection;

namespace ApplicationCore.Features.CNC.Tools;

public static class DependencyInjection {

    public static IServiceCollection AddToolEditor(this IServiceCollection services) {

        services.AddTransient<ToolFileEditorViewModel>();

        return services;

    }

}
