using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;
using Blazored.Modal;
using ApplicationCore.Features.Orders;
using ApplicationCore.Shared.Services;
using ApplicationCore.Features.DataFilePaths;
using ApplicationCore.Features.Updates;
using Domain.Infrastructure.Bus;
using Domain.Components.ProgressModal;
using Domain.Infrastructure.Data;
using Domain.Services;
using Companies;
using Domain;
using OrderExporting.CNC;

[assembly: InternalsVisibleTo("ApplicationCore.Tests.Unit")]

namespace ApplicationCore.Application;

public static class DependencyInjection {

    public static IServiceCollection AddApplicationCoreServices(this IServiceCollection services, IConfiguration configuration) {

        services.AddDomainServices();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        services.AddSingleton(sp => sp);

        services.AddTransient<NavigationService>();

        services.AddCompanies();

        services.AddOrderFeatures(configuration);

        services.AddCNCServices();

        services.AddConfiguration();

        services.AddBlazoredModal();

        services.AddUpdates();

        services.AddHttpClient();

        services.AddTransient<IEmailService, EmailService>();

        services.AddSingleton<IFileReader, FileReader>();
        services.AddSingleton<IFileWriter, FileWriter>();

        services.AddTransient<ProgressModalViewModel>();

        services.AddSingleton<IBus, MediatRBus>();

        SqlMapping.AddSqlMaps();

        return services;

    }

}
