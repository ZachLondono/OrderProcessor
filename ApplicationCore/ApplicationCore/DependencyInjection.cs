using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;
using Blazored.Modal;
using ApplicationCore.Infrastructure;
using ApplicationCore.Features.Orders;
using ApplicationCore.Features.WorkOrders;
using ApplicationCore.Features.Companies;
using ApplicationCore.Features.Shared.Services;
using ApplicationCore.Features.Configuration;
using ApplicationCore.Features.CNC;
using ApplicationCore.Features.Shared.Components.ProgressModal;
using ApplicationCore.Pages.OrderList;
using ApplicationCore.Features.Shared.Settings;

[assembly: InternalsVisibleTo("ApplicationCore.Tests.Unit")]

namespace ApplicationCore;

public static class DependencyInjection {

    public static IServiceCollection AddApplicationCoreServices(this IServiceCollection services, IConfiguration configuration) {

        services.AddMediatR(typeof(DependencyInjection));

        services.AddSingleton<IServiceProvider>(sp => sp);

        services.AddTransient<NavigationService>();

        services.AddCompanies();

        services.AddOrdering(configuration);

        services.AddCNC(configuration);

        services.AddWorkOrders();

        services.AddConfiguration();

        services.AddBlazoredModal();

        services.AddOrderListPage();

        services.AddSingleton<IFileReader, FileReader>();
        services.AddSingleton<IFileWriter, FileWriter>();

        services.AddTransient<ProgressModalViewModel>();

        services.Configure<ConfigurationFiles>(configuration.GetRequiredSection("ConfigurationFiles"));

        services.AddApplicationInfrastructure(configuration);

        return services;

    }



}
