using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;
using Blazored.Modal;
using ApplicationCore.Features.Orders;
using ApplicationCore.Features.Companies;
using ApplicationCore.Shared.Services;
using ApplicationCore.Shared.Components.ProgressModal;
using ApplicationCore.Pages.OrderList;
using ApplicationCore.Shared.Data;
using ApplicationCore.Shared.Bus;
using ApplicationCore.Features.DataFilePaths;
using ApplicationCore.Features.Updates;
using ApplicationCore.Shared.CNC;

[assembly: InternalsVisibleTo("ApplicationCore.Tests.Unit")]

namespace ApplicationCore.Application;

public static class DependencyInjection {

    public static IServiceCollection AddApplicationCoreServices(this IServiceCollection services, IConfiguration configuration) {

        services.AddMediatR(typeof(DependencyInjection));

        services.AddSingleton(sp => sp);

        services.AddTransient<NavigationService>();

        services.AddCompanies();

        services.AddOrderFeatures(configuration);

        services.AddCNCServices();

        services.AddConfiguration();

        services.AddBlazoredModal();

        services.AddOrderListPage();

        services.AddUpdates();

        services.AddHttpClient();

        services.AddTransient<IEmailService, EmailService>();

        services.AddSingleton<IFileReader, FileReader>();
        services.AddSingleton<IFileWriter, FileWriter>();

        services.AddTransient<ProgressModalViewModel>();

        services.AddBus(configuration);

        SqlMapping.AddSqlMaps();

        return services;

    }



}
