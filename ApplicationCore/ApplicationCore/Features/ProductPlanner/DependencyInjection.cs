using ApplicationCore.Features.ProductPlanner.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ApplicationCore.Features.ProductPlanner;
public static class DependencyInjection {

    public static IServiceCollection AddProductPlanner(this IServiceCollection services) {

        return services.AddTransient<ExtWriter>();

    }

}
