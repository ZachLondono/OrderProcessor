using ApplicationCore.Features.Orders.Providers;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;
using ApplicationCore.Features.Orders;
using ApplicationCore.Features.Companies;
using Blazored.Modal;
using ApplicationCore.Features.Orders.Loader.Providers.AllmoxyXMLModels;
using ApplicationCore.Features.ExcelTemplates.Domain;
using ApplicationCore.Shared;
using ApplicationCore.Infrastructure;
using ApplicationCore.Features.Orders.Domain.ValueObjects;
using ApplicationCore.Features.Labels.Services;
using ApplicationCore.Features.Orders.Complete;
using ApplicationCore.Features.Emails;
using ApplicationCore.Features.Orders.Loader.Providers;
using ApplicationCore.Features.CADCode;
using ApplicationCore.Features.Orders.Loader.Providers.RichelieuXMLModels;

[assembly: InternalsVisibleTo("ApplicationCore.Tests.Unit")]

namespace ApplicationCore;

public static class DependencyInjection {

    public static IServiceCollection AddApplicationCoreServices(this IServiceCollection services, IConfiguration configuration) {
        
        services.AddMediatR(typeof(DependencyInjection));

        services.AddSingleton<OrderState>();
        services.AddSingleton<CompanyState>();
        services.AddSingleton<IServiceProvider>(sp => sp);

        // TODO: validate configuration data

        services.AddOrderLoading(configuration);

        services.AddEmailing();

        services.AddCADCode(configuration);

        services.AddBlazoredModal();

        services.AddSingleton<IFileReader, FileReader>();

        services.AddApplicationInfrastructure(configuration);

        return services;
    }

}
