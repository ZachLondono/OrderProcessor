using ApplicationCore.Pages.OrderDetails;
using Microsoft.Extensions.DependencyInjection;

namespace ApplicationCore.Pages;

internal static class DependencyInjection {

    public static IServiceCollection AddPages(this IServiceCollection services)
        => services.AddTransient<OrderDetailsPageViewModel>();

}
