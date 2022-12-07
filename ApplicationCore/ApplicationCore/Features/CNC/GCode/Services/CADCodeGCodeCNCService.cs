using ApplicationCore.Features.CNC.GCode.Configuration;
using ApplicationCore.Features.CNC.GCode.Contracts;
using ApplicationCore.Features.CNC.GCode.Domain;

namespace ApplicationCore.Features.CNC.GCode.Services;

public class CADCodeGCodeCNCService : ICNCService
{

    private readonly ICADCodeConfigurationProvider _configProvider;
    private readonly IInventoryService _inventoryService;
    private readonly ICADCodeMachineConfigurationProvider _ccmachineConfigProvider;

    public CADCodeGCodeCNCService(ICADCodeConfigurationProvider configProvider, IInventoryService inventoryService, ICADCodeMachineConfigurationProvider ccmachineConfigProvider)
    {
        _configProvider = configProvider;
        _inventoryService = inventoryService;
        _ccmachineConfigProvider = ccmachineConfigProvider;
    }

    public async Task<GCodeGenerationResult> ExportToCNC(CNCBatch batch, IEnumerable<CNCMachineConfiguration> machineConfigs)
    {

        var cncConfig = _configProvider.GetConfiguration();
        var availableInventory = await _inventoryService.GetInventory();

        var ccmachineConfigs = _ccmachineConfigProvider.GetConfigurations();
        var ccmachineConfigDict = ccmachineConfigs.ToDictionary(c => c.MachineName, c => c);

        var machineResults = await Task.Run<IEnumerable<MachineNestResult>>(() =>
        {

            var cadCode = new CADCodeManager(batch.Name, availableInventory);
            foreach (var part in batch.Parts)
                cadCode.AddPart(part);

            var machineReleases = new List<MachineNestResult>();
            foreach (var machineConfig in machineConfigs)
            {

                if (!ccmachineConfigDict.TryGetValue(machineConfig.MachineName, out CADCodeMachineConfiguration? ccmachineconfig) || ccmachineconfig is null)
                {
                    Console.WriteLine($"No CADCode configuration for machine {machineConfig.MachineName}");
                    // TODO show warning for missing CADCode configuration
                    continue;
                }

                // TODO: pass machine name to function call so it can generate machine-specific results
                _ = cadCode.GenerateSinglePrograms(cncConfig, machineConfig, ccmachineconfig);
                var nestResults = cadCode.GenerateNestedCode(cncConfig, machineConfig, ccmachineconfig);

                machineReleases.Add(new MachineNestResult()
                {
					MachineName = machineConfig.MachineName,
					TableOrientation = machineConfig.Orientation,
					OptimizationResults = nestResults,
                    PictureFileOutputDirectory = cncConfig.PictureFileOutputDirectory
				});
            }

            return machineReleases;

        });


        return new GCodeGenerationResult()
        {
            BatchName = batch.Name,
			MachineResults = machineResults
        };

    }

}
