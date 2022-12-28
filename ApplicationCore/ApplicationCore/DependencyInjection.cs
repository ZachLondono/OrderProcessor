using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;
using ApplicationCore.Features.Orders;
using ApplicationCore.Features.Companies;
using Blazored.Modal;
using ApplicationCore.Shared;
using ApplicationCore.Infrastructure;
using ApplicationCore.Features.Emails;
using ApplicationCore.Features.CNC;
using ApplicationCore.Features.Orders.Loader;
using ApplicationCore.Features.ProductPlanner;

[assembly: InternalsVisibleTo("ApplicationCore.Tests.Unit")]

namespace ApplicationCore;

public static class DependencyInjection {

    public static IServiceCollection AddApplicationCoreServices(this IServiceCollection services, IConfiguration configuration) {
        
        services.AddMediatR(typeof(DependencyInjection));

        services.AddSingleton<OrderState>();
        services.AddSingleton<CompanyState>();
        services.AddSingleton<IServiceProvider>(sp => sp);
		services.AddTransient<IAccessDBConnectionFactory, AccessDBConnectionFactory>();

		// TODO: validate configuration data

		services.AddOrderLoading(configuration);

        services.AddEmailing();

        services.AddCADCode(configuration);

        services.AddProductPlanner();

        services.AddBlazoredModal();

        services.AddSingleton<IFileReader, FileReader>();

        services.AddApplicationInfrastructure(configuration);

        return services;
    }

}
