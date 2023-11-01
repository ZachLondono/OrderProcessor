using ApplicationCore.Shared.CNC.ReleasePDF;
using ApplicationCore.Shared.CNC.ReleasePDF.Services;
using ApplicationCore.Shared.CNC.WSXML;
using Microsoft.Extensions.DependencyInjection;

namespace ApplicationCore.Shared.CNC;

public static class DependencyInjection {

    public static IServiceCollection AddCNCServices(this IServiceCollection services) {
        services.AddTransient<ReleasePDFDecoratorFactory>();
        services.AddTransient<ICNCReleaseDecorator, CNCReleaseDecorator>();
        services.AddTransient<IWSXMLParser, WSXMLParser>();
        return services;

    }

}
