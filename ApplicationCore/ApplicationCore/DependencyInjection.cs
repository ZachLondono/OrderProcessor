using ApplicationCore.Features.Companies.AllmoxyId;
using ApplicationCore.Features.Orders.ProductDrawings.ViewModels;
using ApplicationCore.Widgets.Orders.OrderRelationshipList;
using Microsoft.Extensions.DependencyInjection;

namespace ApplicationCore;

public static class DependencyInjection {

    public static IServiceCollection AddViewModels(this IServiceCollection services) {
        return services
                .ProductDrawingsViewModels()
                .AddTransient<CustomerAllmoxyIdViewModel>();

    }

    public static IServiceCollection ProductDrawingsViewModels(this IServiceCollection services) {

        return services
                    .AddTransient<ProductDrawingManagerViewModel>()
                    .AddTransient<ProductDrawingManagerButtonViewModel>()
                    .AddTransient<ProductDrawingRowViewModel>()
                    .AddTransient<OrderRelationshipListWidgetViewModel>();

    }

}