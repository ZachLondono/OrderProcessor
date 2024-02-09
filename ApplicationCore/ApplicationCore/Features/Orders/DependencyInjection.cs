using ApplicationCore.Features.Orders.OrderExport;
using OrderExporting.ExtExport.Services;
using ApplicationCore.Features.Orders.OrderRelease;
using Domain.Orders.Builders;
using OrderExporting.Invoice;
using OrderExporting.PackingList;
using OrderExporting.JobSummary;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ApplicationCore.Features.Orders.CustomerOrderNumber;
using ApplicationCore.Features.Orders.Details.Models.WorkingDirectory;
using OrderExporting.DovetailDBPackingList;
using OrderExporting.FivePieceDoorCutList;
using OrderExporting.DoweledDrawerBoxCutList;
using OrderLoading.LoadAllmoxyOrderData.AllmoxyXMLModels;
using OrderLoading.LoadAllmoxyOrderData.LoadAllmoxyWebOrderData;
using OrderLoading.LoadAllmoxyOrderData.XMLValidation;
using OrderLoading.LoadClosetOrderSpreadsheetOrderData;
using OrderLoading.LoadDoweledDBSpreadsheetOrderData.Models;
using Domain.Orders.ValueObjects;
using Domain.Orders.Persistance;
using OrderLoading.ClosetProCSVCutList;
using OrderLoading.LoadDoweledDBSpreadsheetOrderData;
using OrderLoading.LoadClosetProOrderData.LoadClosetProWebOrderData;
using OrderLoading.LoadClosetProOrderData.LoadClosetProFileOrderData;
using OrderLoading.LoadAllmoxyOrderData.LoadAllmoxyFileOrderData;
using OrderLoading.LoadHafeleDBSpreadsheetOrderData;
using ApplicationCore.Features.Orders.OrderLoading.Dialog;
using ApplicationCore.Features.Orders.OrderLoading.Models;
using ApplicationCore.Features.Orders.OrderLoading.PickOrderSource;

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
