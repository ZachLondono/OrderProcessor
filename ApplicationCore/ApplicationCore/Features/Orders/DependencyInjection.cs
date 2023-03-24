using ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.CNC.ReleasePDF.Configuration;
using ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.CNC.ReleasePDF.Services;
using ApplicationCore.Features.Orders.Details.OrderExport;
using ApplicationCore.Features.Orders.Details.OrderExport.Handlers;
using ApplicationCore.Features.Orders.Details.OrderExport.Handlers.ExtExport;
using ApplicationCore.Features.Orders.Details.OrderExport.Handlers.ExtExport.Services;
using ApplicationCore.Features.Orders.Details.OrderRelease;
using ApplicationCore.Features.Orders.Loader;
using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.State;
using ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.CNC.ReleasePDF;
using ApplicationCore.Features.Orders.Data;
using ApplicationCore.Features.Orders.List;
using ApplicationCore.Infrastructure.Bus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Dapper;
using ApplicationCore.Features.Orders.Contracts;
using ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.Invoice;
using ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.PackingList;
using ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.JobSummary;

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

        services.AddTransient<JobSummaryDecorator>();
        services.AddTransient<CNCReleaseDecorator>();
        services.AddTransient<InvoiceDecorator>();
        services.AddTransient<PackingListDecorator>();
        services.AddTransient<ReleaseService>();

        services.Configure<ExportOptions>(configuration.GetRequiredSection("ExportOptions"));

        services.AddTransient<ExportService>();
        services.AddTransient<DoorOrderHandler>();
        services.AddTransient<DovetailOrderHandler>();
        services.AddTransient<ExtOrderHandler>();
        services.AddTransient<IExtWriter, ExtWriter>();

        var cadcode = configuration.GetRequiredSection("CADCode");
        var pdfconfig = cadcode.GetValue<string>("ReleasePDFConfig");
        if (pdfconfig is null) throw new InvalidOperationException("Release PDF configuration was not found");
        var jsonPDF = new JSONPDFConfigurationProvider(pdfconfig);
        services.AddTransient<IPDFConfigurationProvider>(s => jsonPDF);

        services.AddTransient<IReleasePDFWriter, QuestPDFWriter>();

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

    private static IServiceCollection AddViewModels(this IServiceCollection services)
        => services.AddTransient<OrderListViewModel>()
                    .AddTransient<ReleaseProgressViewModel>()
                    .AddTransient<ExportProgressViewModel>();

}
