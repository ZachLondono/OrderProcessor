using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;
using ApplicationCore.Features.Companies;
using Blazored.Modal;
using ApplicationCore.Features.Shared;
using ApplicationCore.Infrastructure;
using ApplicationCore.Features.Emails;
using ApplicationCore.Features.CNC;
using ApplicationCore.Features.Orders.Loader;
using ApplicationCore.Features.ProductPlanner;
using ApplicationCore.Features.Orders.Shared.State;
using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.List;
using ApplicationCore.Features.Companies.Queries;
using static ApplicationCore.Features.Orders.Release.Handlers.GenerateCabinetPackingList;
using ApplicationCore.Features.WorkOrders;
using ApplicationCore.Pages.OrderDetails;
using ApplicationCore.Features.WorkOrders.AllWorkOrders;
using ApplicationCore.Features.Orders;

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

        services.AddTransient<VendorInfo.GetVendorInfoById>((sp) => {
            var bus = sp.GetRequiredService<IBus>();
            return async (id) => {

                var response = await bus.Send(new GetCompanyById.Query(id));
                Vendor? vendor = null;
                response.OnSuccess(result => {
                    
                    if (result is null) {
                        return;
                    }

                    vendor = new() {
                        Name = result.Name,
                        Address = new() {
                            Line1 = result.Address.Line1,
                            Line2 = result.Address.Line2,
                            Line3 = result.Address.Line3,
                            City = result.Address.City,
                            State = result.Address.State,
                            Zip = result.Address.Zip,
                            Country = result.Address.Country
                        }
                    };

                });

                return vendor;

            };
        });

        services.AddViewModels();

        services.AddOrdering(configuration);

        services.AddWorkOrders();

        services.AddEmailing();

        services.AddCADCode(configuration);

        services.AddProductPlanner();

        services.AddBlazoredModal();

        services.AddSingleton<IFileReader, FileReader>();

        services.AddApplicationInfrastructure(configuration);

        return services;
    }

    private static IServiceCollection AddViewModels(this IServiceCollection services) 
        =>  services.AddTransient<OrderListViewModel>()
                    .AddTransient<OrderTaskListViewModel>()
                    .AddTransient<OrderDetailsPageViewModel>()
                    .AddTransient<BarCodeScanningDialogViewModel>()
                    .AddTransient<AllWorkOrdersListViewModel>();

}
