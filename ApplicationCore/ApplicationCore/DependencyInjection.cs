using ApplicationCore.Features.ClosetOrderSelector;
using ApplicationCore.Features.Companies.AllmoxyId;
using ApplicationCore.Features.CreateOrderRelationship;
using ApplicationCore.Features.DeleteOrder;
using ApplicationCore.Features.OrderList;
using ApplicationCore.Features.OrderRelationshipList;
using ApplicationCore.Features.Orders.Details.ViewModels;
using ApplicationCore.Features.Orders.OrderRelease;
using ApplicationCore.Features.Orders.ProductDrawings.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace ApplicationCore;

public static class DependencyInjection {

    public static IServiceCollection AddViewModels(this IServiceCollection services) {
        return services
                .ProductDrawingsViewModels()
                .OrderDetailsViewModels()
                .AddTransient<CustomerAllmoxyIdViewModel>()
                .AddTransient<CreateOrderRelationshipsViewModel>()
                .AddTransient<ClosetOrderSelectorViewModel>()
                .AddTransient<OrderRelationshipListViewModel>()
                .AddTransient<DeleteOrderConfirmationModalViewModel>()
                .AddTransient<OrderListWidgetViewModel>()
                .AddTransient<OrderReleaseModalViewModel>();

    }

    public static IServiceCollection ProductDrawingsViewModels(this IServiceCollection services) {

        return services
                    .AddTransient<ProductDrawingManagerViewModel>()
                    .AddTransient<ProductDrawingManagerButtonViewModel>()
                    .AddTransient<ProductDrawingRowViewModel>();

    }

    public static IServiceCollection OrderDetailsViewModels(this IServiceCollection services) {

        return services
                .AddTransient<OrderHeaderViewModel>();

    }

}