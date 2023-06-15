using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;
using Blazored.Modal;
using ApplicationCore.Features.Orders;
using ApplicationCore.Features.Companies;
using ApplicationCore.Shared.Services;
using ApplicationCore.Features.CNC;
using ApplicationCore.Shared.Components.ProgressModal;
using ApplicationCore.Pages.OrderList;
using ApplicationCore.Shared.Data;
using ApplicationCore.Shared.Bus;
using ApplicationCore.Infrastructure.UI;
using ApplicationCore.Shared.Settings;
using ApplicationCore.Features.DataFilePaths;

[assembly: InternalsVisibleTo("ApplicationCore.Tests.Unit")]

namespace ApplicationCore.Application;

public static class DependencyInjection {

    public static IServiceCollection AddApplicationCoreServices(this IServiceCollection services, IConfiguration configuration) {

        services.AddMediatR(typeof(DependencyInjection));

        services.AddSingleton(sp => sp);

        services.AddTransient<NavigationService>();

        services.AddCompanies();

        services.AddOrdering(configuration);

        services.AddCNC(configuration);

        services.AddConfiguration();

        services.AddBlazoredModal();

        services.AddOrderListPage();

        services.AddSingleton<IFileReader, FileReader>();
        services.AddSingleton<IFileWriter, FileWriter>();

        services.AddTransient<ProgressModalViewModel>();

        services.Configure<ConfigurationFiles>(configuration.GetRequiredSection("ConfigurationFiles"));

        services.AddBus(configuration);
        services.AddSingleton<IUIBus, UIBus>();

        SqlMapping.AddSqlMaps();

        return services;

    }



}
