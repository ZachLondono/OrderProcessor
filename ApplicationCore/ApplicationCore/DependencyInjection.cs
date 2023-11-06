using ApplicationCore.Features.ClosetOrderSelector;
using ApplicationCore.Features.Companies.AllmoxyId;
using ApplicationCore.Features.CreateOrderRelationship;
using ApplicationCore.Features.CustomizationScripts.ViewModels;
using ApplicationCore.Features.DeleteOrder;
using ApplicationCore.Features.OrderList;
using ApplicationCore.Features.OrderRelationshipList;
using ApplicationCore.Features.Orders.Details.ViewModels;
using ApplicationCore.Features.Orders.OrderRelease;
using ApplicationCore.Features.Orders.ProductDrawings.ViewModels;
using ApplicationCore.Features.Products.UpdateClosetPart;
using ApplicationCore.Features.Settings;
using ApplicationCore.Layouts.MainLayout.ReleaseDialog;
using Microsoft.Extensions.DependencyInjection;

namespace ApplicationCore;

public static class DependencyInjection {

    public static IServiceCollection AddViewModels(this IServiceCollection services) {
        return services
                .ProductDrawingsViewModels()
                .OrderDetailsViewModels()
                .ProductEditorViewModels()
                .SettingsViewModels()
                .CustomizationScriptsManagerViewModels()
                .AddTransient<CustomerAllmoxyIdViewModel>()
                .AddTransient<CreateOrderRelationshipsViewModel>()
                .AddTransient<ClosetOrderSelectorViewModel>()
                .AddTransient<OrderRelationshipListViewModel>()
                .AddTransient<DeleteOrderConfirmationModalViewModel>()
                .AddTransient<OrderListWidgetViewModel>()
                .AddTransient<OrderReleaseModalViewModel>()
                .AddTransient<ReleasePDFDialogViewModel>();

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

    public static IServiceCollection ProductEditorViewModels(this IServiceCollection services) {

        return services
                .AddTransient<ClosetPartEditorViewModel>();

    }

    public static IServiceCollection SettingsViewModels(this IServiceCollection services) {

        return services
                .AddTransient<ToolFileEditorViewModel>();

    }

    public static IServiceCollection CustomizationScriptsManagerViewModels(this IServiceCollection services) {

        return services
                .AddTransient<CustomizationScriptManagerViewModel>()
                .AddTransient<AddNewScriptViewModel>();

    }

}