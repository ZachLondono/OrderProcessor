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
using ApplicationCore.Features.DataFilePaths;
using ApplicationCore.Features.Updates;
using ApplicationCore.Widgets.Orders;
using ApplicationCore.Widgets.Companies;
using ApplicationCore.Features.Orders.ProductDrawings.ViewModels;

[assembly: InternalsVisibleTo("ApplicationCore.Tests.Unit")]

namespace ApplicationCore.Application;

public static class DependencyInjection {

    public static IServiceCollection AddApplicationCoreServices(this IServiceCollection services, IConfiguration configuration) {

        services.AddMediatR(typeof(DependencyInjection));

        services.AddSingleton(sp => sp);

        services.AddTransient<NavigationService>();

        services.AddCompanies();
        services.AddCompanyWidgets();

        services.AddOrderFeatures(configuration);
        services.AddOrderWidgets();
        services.AddTransient<ProductDrawingManagerButtonViewModel>();

        services.AddCNC(configuration);

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
        services.AddSingleton<IUIBus, UIBus>();

        SqlMapping.AddSqlMaps();

        return services;

    }



}
