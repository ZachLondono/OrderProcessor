using ApplicationCore.Features.Orders.Details.OrderExport;
using ApplicationCore.Features.Orders.Details.OrderExport.Handlers.ExtExport.Services;
using ApplicationCore.Features.Orders.Details.OrderRelease;
using ApplicationCore.Features.Orders.Loader;
using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.State;
using ApplicationCore.Features.Orders.Data;
using ApplicationCore.Features.Orders.List;
using ApplicationCore.Features.Orders.Contracts;
using ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.Invoice;
using ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.PackingList;
using ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.JobSummary;
using ApplicationCore.Features.Orders.Loader.Providers.DoorOrderModels;
using ApplicationCore.Features.Orders.Loader.Providers;
using ApplicationCore.Features.Orders.Loader.Providers.Dialog;
using ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.CNC;
using ApplicationCore.Infrastructure.Bus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Dapper;

namespace ApplicationCore.Features.Orders;

public static class DependencyInjection {

    public static IServiceCollection AddOrdering(this IServiceCollection services, IConfiguration configuration) {

        services.AddViewModels();

        services.AddTransient<IOrderingDbConnectionFactory, SqliteOrderingDbConnectionFactory>();

        SqlMapper.AddTypeHandler(new ToeTypeTypeHandler());
        SqlMapper.AddTypeHandler(new SqliteFixedDivdersCountsTypeHandler());
        SqlMapper.AddTypeHandler(new SqliteUBoxDimensionTypeHandler());

        services.AddOrderLoading(configuration);
        services.AddSingleton<ProductBuilderFactory>();
        services.AddSingleton<ComponentBuilderFactory>();
        services.AddSingleton<OrderState>();

        AddReleaseServices(services);
        AddExportServices(services, configuration);
        AddOrderProviders(services, configuration);

        services.AddTransient<Ordering.GetOrderNumberById>(sp => {

            var bus = sp.GetRequiredService<IBus>();
            return async (id) => {

                var response = await bus.Send(new GetOrderNumberById.Query(id));

                string number = "";
                response.Match(
                    result => number = result,
                    error => number = "Unknown"
                );

                return number;

            };

        });

        return services;

    }

    private static void AddReleaseServices(IServiceCollection services) {
        services.AddTransient<CNCReleaseDecoratorFactory>();
        services.AddTransient<IJobSummaryDecorator, JobSummaryDecorator>();
        services.AddTransient<IInvoiceDecorator, InvoiceDecorator>();
        services.AddTransient<IPackingListDecorator, PackingListDecorator>();
        services.AddTransient<ReleaseService>();
    }

    private static void AddExportServices(IServiceCollection services, IConfiguration configuration) {
        services.Configure<ExportOptions>(configuration.GetRequiredSection("ExportOptions"));
        services.AddTransient<ExportService>();
        services.AddTransient<IExtWriter, ExtWriter>();
        services.AddTransient<ExportWidgetViewModel>();
    }

    private static void AddOrderProviders(IServiceCollection services, IConfiguration configuration) {
        services.Configure<DoorOrderProviderOptions>(configuration.GetRequiredSection("DoorOrderProviderOptions"));
        services.AddTransient<DoorSpreadsheetOrderProvider>();
    }

    private static IServiceCollection AddViewModels(this IServiceCollection services)
        => services.AddTransient<OrderListViewModel>()
                    .AddTransient<ChooseOrderProviderViewModel>();

}
