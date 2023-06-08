using Microsoft.Extensions.DependencyInjection;

namespace ApplicationCore.Pages.OrderList;

public static class DependencyInjection {

    public static IServiceCollection AddOrderListPage(this IServiceCollection services) {
        return services.AddTransient<OrderListPageViewModel>();
    }

}
