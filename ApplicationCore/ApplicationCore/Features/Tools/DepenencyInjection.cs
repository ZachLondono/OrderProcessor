using Microsoft.Extensions.DependencyInjection;

namespace ApplicationCore.Features.Tools;

public static class DepenencyInjection {

    public static IServiceCollection AddToolEditor(this IServiceCollection services) {

        return services.AddTransient<ToolFileEditorViewModel>();

    }

}
