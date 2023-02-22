using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;
using ApplicationCore.Features.Companies;
using Blazored.Modal;
using ApplicationCore.Infrastructure;
using ApplicationCore.Features.Orders.List;
using ApplicationCore.Features.Companies.Queries;
using ApplicationCore.Features.WorkOrders;
using ApplicationCore.Pages.OrderDetails;
using ApplicationCore.Features.WorkOrders.AllWorkOrders;
using ApplicationCore.Features.Orders;
using ApplicationCore.Features.Orders.Details.OrderRelease;
using ApplicationCore.Features.Orders.Details.OrderExport;
using ApplicationCore.Infrastructure.Bus;
using ApplicationCore.Features.Shared.Services;

[assembly: InternalsVisibleTo("ApplicationCore.Tests.Unit")]

namespace ApplicationCore;

public static class DependencyInjection {

    public static IServiceCollection AddApplicationCoreServices(this IServiceCollection services, IConfiguration configuration) {

        services.AddMediatR(typeof(DependencyInjection));

        services.AddSingleton<CompanyState>();
        services.AddSingleton<IServiceProvider>(sp => sp);
        services.AddTransient<IAccessDBConnectionFactory, AccessDBConnectionFactory>();

        services.AddTransient<NavigationService>();
        services.AddTransient<CompanyInfo.GetCompanyNameById>((sp) => {
            var bus = sp.GetRequiredService<IBus>();
            return async (id) => {
                var response = await bus.Send(new GetCompanyNameById.Query(id));
                string? name = null;
                response.OnSuccess(result => name = result);
                return name;
            };
        });

        services.AddViewModels();

        services.AddOrdering(configuration);

        services.AddWorkOrders();

        services.AddBlazoredModal();

        services.AddSingleton<IFileReader, FileReader>();

        services.AddApplicationInfrastructure(configuration);

        return services;
    }

    private static IServiceCollection AddViewModels(this IServiceCollection services)
        => services.AddTransient<OrderListViewModel>()
                    .AddTransient<OrderTaskListViewModel>()
                    .AddTransient<OrderDetailsPageViewModel>()
                    .AddTransient<BarCodeScanningDialogViewModel>()
                    .AddTransient<AllWorkOrdersListViewModel>()
                    .AddTransient<ReleaseProgressViewModel>()
                    .AddTransient<ExportProgressViewModel>();

}
