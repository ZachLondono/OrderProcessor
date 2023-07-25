using ApplicationCore.Widgets.Orders.Release;
using Microsoft.Extensions.DependencyInjection;

namespace ApplicationCore.Widgets.Orders;

internal static class DependencyInjection {

    public static IServiceCollection AddOrderWidgets(this IServiceCollection services) {
        return services.AddTransient<OrderReleaseModalViewModel>();
    }

}
