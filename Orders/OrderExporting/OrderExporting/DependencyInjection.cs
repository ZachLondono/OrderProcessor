using Microsoft.Extensions.DependencyInjection;
using OrderExporting.CNC.Programs;
using OrderExporting.CNC.Programs.WSXML;
using OrderExporting.CNC.ReleasePDF;
using OrderExporting.CNC.ReleasePDF.Services;
using OrderExporting.DovetailDBPackingList;
using OrderExporting.DoweledDrawerBoxCutList;
using OrderExporting.ExtExport.Services;
using OrderExporting.FivePieceDoorCutList;
using OrderExporting.Invoice;
using OrderExporting.JobSummary;
using OrderExporting.PackingList;

namespace OrderExporting;

public static class DependencyInjection {

	public static IServiceCollection AddOrderExporting(this IServiceCollection services) {
		return services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly))
						.AddTransient<IExtWriter, ExtWriter>()
                        .AddTransient<JobSummaryDecorator>()
                        .AddTransient<JobSummaryDecoratorFactory>()
                        .AddTransient<IInvoiceDecorator, InvoiceDecorator>()
                        .AddTransient<InvoiceDecoratorFactory>()
                        .AddTransient<IPackingListDecorator, PackingListDecorator>()
                        .AddTransient<PackingListDecoratorFactory>()
                        .AddTransient<IDovetailDBPackingListDecoratorFactory, DovetailDBPackingListDecoratorFactory>()
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
