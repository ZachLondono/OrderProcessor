using ApplicationCore.Features.Orders.OrderRelease;
using ApplicationCore.Widgets.Orders.OrderHeader;
using ApplicationCore.Widgets.Orders.OrderList;
using Microsoft.Extensions.DependencyInjection;

namespace ApplicationCore.Widgets.Orders;

internal static class DependencyInjection {

    public static IServiceCollection AddOrderWidgets(this IServiceCollection services) {
        return services.AddTransient<OrderReleaseModalViewModel>()
                        .AddTransient<OrderListWidgetViewModel>()
                        .AddTransient<OrderHeaderViewModel>();
    }

}
