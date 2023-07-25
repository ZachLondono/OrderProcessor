using Microsoft.Extensions.DependencyInjection;

namespace ApplicationCore.Features.Updates;

public static class DependencyInjection {

    public static IServiceCollection AddUpdates(this IServiceCollection services) {
        return services.AddTransient<UpdatesDialogViewModel>()
                        .AddTransient<ApplicationVersionService>();
    }

}
