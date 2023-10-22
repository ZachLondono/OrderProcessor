using ApplicationCore.Features.Orders.OrderExport;
using ApplicationCore.Features.Orders.OrderExport.Handlers.ExtExport.Services;
using ApplicationCore.Features.Orders.OrderRelease;
using ApplicationCore.Features.Orders.OrderLoading;
using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Contracts;
using ApplicationCore.Features.Orders.OrderRelease.Handlers.Invoice;
using ApplicationCore.Features.Orders.OrderRelease.Handlers.PackingList;
using ApplicationCore.Features.Orders.OrderRelease.Handlers.JobSummary;
using ApplicationCore.Features.Orders.OrderRelease.Handlers.CNC;
using ApplicationCore.Infrastructure.Bus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ApplicationCore.Features.Orders.Delete;
using ApplicationCore.Features.Orders.OrderLoading.PickOrderSource;
using ApplicationCore.Shared.Data.Ordering;
using ApplicationCore.Features.Orders.CustomerOrderNumber;
using ApplicationCore.Features.Orders.WorkingDirectory;

namespace ApplicationCore.Features.Orders;

public static class DependencyInjection {

    public static IServiceCollection AddOrderFeatures(this IServiceCollection services, IConfiguration configuration) {

        services.AddViewModels();

        services.AddTransient<IOrderingDbConnectionFactory, SqliteOrderingDbConnectionFactory>();

        services.AddOrderLoading(configuration);
        services.AddSingleton<ProductBuilderFactory>();
        services.AddSingleton<ComponentBuilderFactory>();

        services.AddTransient<IFileHandler, FileHandler>();

        AddReleaseServices(services);
        AddExportServices(services);

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
        services.AddTransient<JobSummaryDecoratorFactory>();
        services.AddTransient<IInvoiceDecorator, InvoiceDecorator>();
        services.AddTransient<InvoiceDecoratorFactory>();
        services.AddTransient<IPackingListDecorator, PackingListDecorator>();
        services.AddTransient<PackingListDecoratorFactory>();
        services.AddTransient<ReleaseService>();
    }

    private static void AddExportServices(IServiceCollection services) {
        services.AddTransient<ExportService>();
        services.AddTransient<IExtWriter, ExtWriter>();
        services.AddTransient<OrderExportModalViewModel>();
    }

    private static IServiceCollection AddViewModels(this IServiceCollection services)
        => services.AddTransient<ChooseOrderProviderViewModel>()
                    .AddTransient<DeleteOrderConfirmationModalViewModel>()
                    .AddTransient<CustomerOrderNumberViewModel>();

}
