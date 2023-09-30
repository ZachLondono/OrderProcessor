using ApplicationCore.Features.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ApplicationCore.Features.DataFilePaths;

public static class DependencyInjection {

    public static IServiceCollection AddConfiguration(this IServiceCollection services) {
        return services.AddTransient<DataFilePathsEditorViewModel>();
    }

}