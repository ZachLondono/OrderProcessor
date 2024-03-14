using Microsoft.Extensions.DependencyInjection;
using OrderExporting.CNC.Programs;
using OrderExporting.CNC.Programs.WSXML;
using OrderExporting.CNC.ReleasePDF;
using OrderExporting.CNC.ReleasePDF.Services;
using OrderExporting.DoweledDrawerBoxCutList;
using OrderExporting.ExtExport.Services;
using OrderExporting.FivePieceDoorCutList;

namespace OrderExporting;

public static class DependencyInjection {

	public static IServiceCollection AddOrderExporting(this IServiceCollection services) {
		return services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly))
						.AddTransient<IExtWriter, ExtWriter>()
                        .AddTransient<IFivePieceDoorCutListWriter, FivePieceDoorCutListWriter>()
                        .AddTransient<IDoweledDrawerBoxCutListWriter, DoweledDrawerBoxCutListWriter>()
						.AddCNCServices();
	}

    private static IServiceCollection AddCNCServices(this IServiceCollection services) {
        services.AddTransient<ReleasePDFDecoratorFactory>();
        services.AddTransient<ICNCReleaseDecorator, CNCReleaseDecorator>();
        services.AddTransient<IWSXMLParser, WSXMLParser>();
        services.AddTransient<CNCPartGCodeGenerator>();
        services.AddTransient<CNCReleaseDecoratorFactory>();
        services.AddTransient<PatternImageFactory>();
        return services;
    }

}
