using ApplicationCore.Features.Orders.OrderExport;
using ApplicationCore.Features.Orders.OrderExport.Handlers.ExtExport.Services;
using ApplicationCore.Features.Orders.OrderRelease;
using Domain.Orders.Builders;
using ApplicationCore.Features.Orders.OrderRelease.Handlers.Invoice;
using ApplicationCore.Features.Orders.OrderRelease.Handlers.PackingList;
using ApplicationCore.Features.Orders.OrderRelease.Handlers.JobSummary;
using ApplicationCore.Features.Orders.OrderRelease.Handlers.CNC;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ApplicationCore.Features.Orders.OrderLoading.PickOrderSource;
using ApplicationCore.Features.Orders.CustomerOrderNumber;
using ApplicationCore.Features.Orders.Details.Models.WorkingDirectory;
using ApplicationCore.Features.Orders.OrderRelease.Handlers.DovetailDBPackingList;
using ApplicationCore.Features.Orders.OrderRelease.Handlers.FivePieceDoorCutList;
using ApplicationCore.Features.Orders.OrderRelease.Handlers.DoweledDrawerBoxCutList;
using ApplicationCore.Shared.CNC;
using ApplicationCore.Features.ClosetProCSVCutList;
using ApplicationCore.Features.Orders.OrderLoading.Dialog;
using ApplicationCore.Features.Orders.OrderLoading.LoadAllmoxyOrderData.AllmoxyXMLModels;
using ApplicationCore.Features.Orders.OrderLoading.LoadAllmoxyOrderData.LoadAllmoxyFileOrderData;
using ApplicationCore.Features.Orders.OrderLoading.LoadAllmoxyOrderData.LoadAllmoxyWebOrderData;
using ApplicationCore.Features.Orders.OrderLoading.LoadAllmoxyOrderData.XMLValidation;
using ApplicationCore.Features.Orders.OrderLoading.LoadClosetOrderSpreadsheetOrderData;
using ApplicationCore.Features.Orders.OrderLoading.LoadClosetProOrderData.LoadClosetProFileOrderData;
using ApplicationCore.Features.Orders.OrderLoading.LoadClosetProOrderData.LoadClosetProWebOrderData;
using ApplicationCore.Features.Orders.OrderLoading.LoadDoweledDBSpreadsheetOrderData.Models;
using ApplicationCore.Features.Orders.OrderLoading.LoadDoweledDBSpreadsheetOrderData;
using ApplicationCore.Features.Orders.OrderLoading.LoadHafeleDBSpreadsheetOrderData;
using ApplicationCore.Features.Orders.OrderLoading.Models;
using Domain.Orders.ValueObjects;
using Domain.Orders.Persistance;

namespace ApplicationCore.Features.Orders;

public static class DependencyInjection {

    public static IServiceCollection AddOrderFeatures(this IServiceCollection services, IConfiguration configuration) {

        services.AddTransient<IOrderingDbConnectionFactory, SqliteOrderingDbConnectionFactory>();
        services.AddSingleton<ProductBuilderFactory>();
        services.AddSingleton<ComponentBuilderFactory>();
        services.AddTransient<IFileHandler, FileHandler>();

        services.AddViewModels();
        services.AddLoadingServices(configuration);
        services.AddReleaseServices();
        services.AddExportServices();

        return services;

    }

    private static IServiceCollection AddReleaseServices(this IServiceCollection services) {
        services.AddTransient<CNCPartGCodeGenerator>();
        services.AddTransient<CNCReleaseDecoratorFactory>();
        services.AddTransient<IJobSummaryDecorator, JobSummaryDecorator>();
        services.AddTransient<JobSummaryDecoratorFactory>();
        services.AddTransient<IInvoiceDecorator, InvoiceDecorator>();
        services.AddTransient<InvoiceDecoratorFactory>();
        services.AddTransient<IPackingListDecorator, PackingListDecorator>();
        services.AddTransient<PackingListDecoratorFactory>();
        services.AddTransient<IDovetailDBPackingListDecoratorFactory, DovetailDBPackingListDecoratorFactory>();
        services.AddTransient<ReleaseService>();
        services.AddTransient<IFivePieceDoorCutListWriter, FivePieceDoorCutListWriter>();
        services.AddTransient<IDoweledDrawerBoxCutListWriter, DoweledDrawerBoxCutListWriter>();
        return services;
    }

    private static IServiceCollection AddExportServices(this IServiceCollection services) {
        services.AddTransient<ExportService>();
        services.AddTransient<IExtWriter, ExtWriter>();
        services.AddTransient<OrderExportModalViewModel>();
        return services;
    }

    public static IServiceCollection AddLoadingServices(this IServiceCollection services, IConfiguration configuration) {

        services.Configure<OrderProvidersConfiguration>(configuration.GetRequiredSection("OrderProviders"));
        services.Configure<ClosetProSoftwareCredentials>(configuration.GetRequiredSection("ClosetProSoftwareCredentials"));
        services.Configure<AllmoxyCredentials>(configuration.GetRequiredSection("AllmoxyCredentials"));
        services.Configure<AllmoxyConfiguration>(configuration.GetRequiredSection("AllmoxyConfiguration"));
        services.Configure<ConstructionValues>(configuration.GetRequiredSection("DrawerBoxConstruction"));
        services.Configure<DoweledDBOrderProviderOptions>(configuration.GetRequiredSection("DoweledDBOrderProviderOptions"));

        services.AddTransient<IOrderProviderFactory, OrderProviderFactory>();
        services.AddTransient<HafeleDBSpreadSheetOrderProvider>();
        services.AddTransient<AllmoxyWebXMLOrderProvider>();
        services.AddTransient<AllmoxyFileXMLOrderProvider>();
        services.AddTransient<ClosetProFileCSVOrderProvider>();
        services.AddTransient<ClosetProWebCSVOrderProvider>();
        services.AddTransient<ClosetProCSVReader>();
        services.AddTransient<ClosetProPartMapper>();
        services.AddTransient<ClosetProClientFactory>();
        services.AddTransient<DoweledDBSpreadsheetOrderProvider>();
        services.AddTransient<ClosetSpreadsheetOrderProvider>();
        services.AddTransient<AllmoxyClientFactory>();
        services.AddTransient<IXMLValidator, XMLValidator>();
        services.AddTransient<OrderLoadWidgetViewModel>();

        return services;

    }

    private static IServiceCollection AddViewModels(this IServiceCollection services)
        => services.AddTransient<ChooseOrderProviderViewModel>()
                    .AddTransient<CustomerOrderNumberViewModel>();

}
