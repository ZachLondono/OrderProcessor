using Microsoft.Extensions.DependencyInjection;
using OrderExporting.CNC.Programs;
using OrderExporting.CNC.Programs.WSXML;
using OrderExporting.CNC.ReleasePDF;
using OrderExporting.CNC.ReleasePDF.Services;

namespace OrderExporting.CNC;

public static class DependencyInjection {

    public static IServiceCollection AddCNCServices(this IServiceCollection services) {
        services.AddTransient<ReleasePDFDecoratorFactory>();
        services.AddTransient<ICNCReleaseDecorator, CNCReleaseDecorator>();
        services.AddTransient<IWSXMLParser, WSXMLParser>();
        services.AddTransient<CNCPartGCodeGenerator>();
        services.AddTransient<CNCReleaseDecoratorFactory>();
        services.AddTransient<PatternImageFactory>();
        return services;

    }

}
