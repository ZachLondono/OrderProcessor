using ApplicationCore.Features.CADCode.Services;
using ApplicationCore.Features.CADCode.Services.Services.CADCodeGCode;
using ApplicationCore.Features.CADCode.Services.Services.CADCodeGCode.Configuration;
using ApplicationCore.Features.CADCode.Services.Services.CADCodeGCode.PDF;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ApplicationCore.Features.CADCode;

public static class DependencyInjection {

    public static IServiceCollection AddCADCode(this IServiceCollection services, IConfiguration config) {

        var cadcode = config.GetRequiredSection("CADCode");
        
        // TODO: improve how configuration is stored

        var inventory = cadcode.GetValue<string>("InventoryConfig");
        var jsonInventory = new JSONInventoryService(inventory);
        services.AddTransient<IInventoryService>(s => jsonInventory);

        var cadcodeconfig = cadcode.GetValue<string>("CADCodeConfig");
        var jsonCADCode = new JSONCADCodeConfigurationProvider(cadcodeconfig);
        services.AddTransient<ICADCodeConfigurationProvider>(s => jsonCADCode);

        var cncmachine = cadcode.GetValue<string>("CNCMachineConfig");
        var jsonMachine = new JSONCADCodeMachineConfigurationProvider(cncmachine);
        services.AddTransient<ICADCodeMachineConfigurationProvider>(s => jsonMachine);

        var pdfconfig = cadcode.GetValue<string>("ReleasePDFConfig");
        var jsonPDF = new JSONPDFConfigurationProvider(pdfconfig);
        services.AddTransient<IPDFConfigurationProvider>(s => jsonPDF);

        services.AddTransient<ICNCConfigurationProvider, MockConfigurationProvider>(); // TODO: replace with real configuration provider
        services.AddTransient<IReleasePDFService, QuestPDFReleasePDFService>();
        services.AddTransient<ICNCService, CADCodeGCodeCNCService>();

        return services;

    }

}
