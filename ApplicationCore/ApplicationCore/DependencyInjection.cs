using ApplicationCore.Features.ClosetOrderRelease;
using ApplicationCore.Features.ClosetOrderSelector;
using ApplicationCore.Features.ClosetProToAllmoxyOrder;
using ApplicationCore.Features.CreateOrderRelationship;
using ApplicationCore.Features.CustomizationScripts.ViewModels;
using ApplicationCore.Features.DataFilePaths;
using ApplicationCore.Features.DeleteOrder;
using ApplicationCore.Features.DoorOrderRelease;
using ApplicationCore.Features.FivePieceOrderRelease;
using ApplicationCore.Features.GeneralReleasePDF;
using ApplicationCore.Features.OrderList;
using ApplicationCore.Features.OrderRelationshipList;
using ApplicationCore.Features.Orders.CustomerOrderNumber;
using ApplicationCore.Features.Orders.Details.Models.WorkingDirectory;
using ApplicationCore.Features.Orders.Details.ViewModels;
using ApplicationCore.Features.Orders.OrderExport;
using ApplicationCore.Features.Orders.OrderLoading.Dialog;
using ApplicationCore.Features.Orders.OrderLoading.Models;
using ApplicationCore.Features.Orders.OrderLoading.PickOrderSource;
using ApplicationCore.Features.Orders.OrderRelease;
using ApplicationCore.Features.Orders.ProductDrawings.ViewModels;
using ApplicationCore.Features.Products.UpdateClosetPart;
using ApplicationCore.Features.Settings;
using ApplicationCore.Features.Updates;
using ApplicationCore.Pages.OrderList;
using ApplicationCore.Shared.Services;
using Blazored.Modal;
using Companies;
using Companies.AllmoxyId;
using Domain;
using Domain.Orders.Builders;
using Domain.Orders.Persistance;
using Domain.Orders.ValueObjects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderExporting;
using OrderExporting.FivePieceDoorCutList;
using OrderLoading;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("ApplicationCore.Tests.Unit")]

namespace ApplicationCore;

public static class DependencyInjection {

    public static IServiceCollection AddApplicationCoreServices(this IServiceCollection services, IConfiguration configuration) {

        services.AddCompanies();
        services.AddDomainServices();
        services.AddOrderFeatures(configuration);

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        services.AddConfiguration();
        services.AddBlazoredModal();
        services.AddUpdates();
        services.AddHttpClient();

        services.AddSingleton(sp => sp);
        services.AddTransient<IEmailService, EmailService>();
        services.AddSingleton<IFileWriter, FileWriter>();
        services.AddTransient<ReleasePDFBuilder>();

        services.AddViewModels();

        return services;

    }

    public static IServiceCollection AddOrderFeatures(this IServiceCollection services, IConfiguration configuration) {
        services.AddTransient<IOrderingDbConnectionFactory, SqliteOrderingDbConnectionFactory>();
        services.AddSingleton<ProductBuilderFactory>();
        services.AddSingleton<ComponentBuilderFactory>();
        services.AddTransient<IFileHandler, FileHandler>();

        services.Configure<OrderProvidersConfiguration>(configuration.GetRequiredSection("OrderProviders"));
        services.Configure<ConstructionValues>(configuration.GetRequiredSection("DrawerBoxConstruction"));
        services.AddTransient<IOrderProviderFactory, OrderProviderFactory>();
        services.AddOrderLoading(configuration);

        services.AddTransient<ReleaseService>();

        services.AddTransient<ExportService>();
        services.AddOrderExporting();

        return services;

    }

    public static IServiceCollection AddViewModels(this IServiceCollection services) {
        return services
                .AddTransient<OrderExportModalViewModel>()
                .AddTransient<OrderLoadWidgetViewModel>()
                .AddTransient<ChooseOrderProviderViewModel>()
                .AddTransient<CustomerOrderNumberViewModel>()
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
                .AddTransient<ReleasePDFDialogViewModel>()
                .AddTransient<DoorOrderReleaseActionRunnerFactory>()
                .AddTransient<ClosetOrderReleaseActionRunnerFactory>()
                .AddTransient<OrderListPageViewModel>()
                .AddTransient<DoorOrderReleaseActionRunner>()
                .AddTransient<ClosetOrderReleaseActionRunner>()
                .AddTransient<ClosetProToAllmoxyMapper>()
                .AddTransient<FivePieceDoorOrderFileSelectorViewModel>()
                .AddTransient<FivePieceDoorReleaseActionRunner>()
                .AddTransient<FivePieceDoorReleasePDFGenerator>()
                .AddTransient<FivePieceDoorCutListWriter>()
                .AddTransient<FivePieceOrderSheetParser>();

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