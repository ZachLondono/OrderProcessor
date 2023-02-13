using ApplicationCore.Features.CNC.LabelDB.Services;
using ApplicationCore.Features.CNC.ReleasePDF.Configuration;
using ApplicationCore.Features.CNC.ReleasePDF.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ApplicationCore.Features.CNC;

public static class DependencyInjection {

    public static IServiceCollection AddCADCode(this IServiceCollection services, IConfiguration config) {

        var cadcode = config.GetRequiredSection("CADCode");
        var pdfconfig = cadcode.GetValue<string>("ReleasePDFConfig");
        if (pdfconfig is null) throw new InvalidOperationException("Release PDF configuration was not found");
        var jsonPDF = new JSONPDFConfigurationProvider(pdfconfig);
        services.AddTransient<IPDFConfigurationProvider>(s => jsonPDF);

        services.AddTransient<IReleasePDFWriter, QuestPDFWriter>();
        services.AddTransient<MachineNameProvider>();
        services.AddTransient<IAvailableJobProvider, AvailableJobProvider>();
        services.AddTransient<IExistingJobProvider, CADCodeLabelDBExistingJobProvider>();

        return services;

    }

}
