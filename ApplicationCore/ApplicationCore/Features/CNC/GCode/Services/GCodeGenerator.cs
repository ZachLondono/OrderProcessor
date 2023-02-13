using ApplicationCore.Features.CNC.GCode.Configuration;
using ApplicationCore.Features.CNC.GCode.Contracts;
using ApplicationCore.Features.CNC.GCode.Contracts.Notifications;
using ApplicationCore.Features.CNC.GCode.Contracts.Options;
using ApplicationCore.Features.CNC.GCode.Contracts.Results;
using ApplicationCore.Features.CNC.GCode.Domain.CADCode;
using ApplicationCore.Infrastructure.UI;

namespace ApplicationCore.Features.CNC.GCode.Services;

public class GCodeGenerator {

    private readonly IUIBus _uiBus;
    private readonly ICADCodeFactory _cadcodeFactory;
    private readonly IInventoryFileReader _inventoryReader;
    private readonly GCodeGenerationConfiguration _configuration;

    public GCodeGenerator(IUIBus uiBus,
                            ICADCodeFactory cadcodeFactory,
                            IInventoryFileReader inventoryReader,
                            GCodeGenerationConfiguration configuration) {
        _uiBus = uiBus;
        _cadcodeFactory = cadcodeFactory;
        _inventoryReader = inventoryReader;
        _configuration = configuration;
    }

    public async Task<GCodeGenerationResult> GenerateGCodeAsync(Batch batch, GCodeGenerationOptions options) {

        var cadcode = _cadcodeFactory.CreateCADCode();
        cadcode.OnProgress(OnCADCodeProgress);
        cadcode.OnError(OnCADCodeError);

        var inventory = await _inventoryReader.GetAvailableInventoryAsync(_configuration.InventoryFilePath);
        var labels = batch.GetLabels();

        var machineresults = new List<MachineGenerationResult>();

        foreach (var machine in options.Machines) {

            if (!_configuration.Machines.TryGetValue(machine.Name, out var machineConfig)) {
                // publish notification about missing configuration error
                continue;
            }

            await cadcode.AddToolsAsync(machineConfig.ToolFilePath);
            cadcode.SetBatch(batch);
            cadcode.AddLabels(labels);
            cadcode.AddInventory(inventory);

            var machineresult = GenerateMachineGCode(cadcode,
                                                    machine,
                                                    machineConfig.NestOutputDirectory,
                                                    machineConfig.SinglePartOutputDirectory);

            machineresults.Add(machineresult);

            cadcode.Reset();


        }

        return new() {
            BatchName = batch.Name,
            MachineResults = machineresults
        };

    }

    private void OnCADCodeProgress(CADCodeProgress progress) {
        _uiBus.Publish(new GCodeGenerationProgress(progress.Progress / 100.0, progress.Message));
    }

    private void OnCADCodeError(CADCodeError error) {
        _uiBus.Publish(new GCodeGenerationError(error.Message));
    }

    public static MachineGenerationResult GenerateMachineGCode(ICADCodeProxy cadcode,
                                                                MachineGCodeOptions options,
                                                                string nestOutputDirectory,
                                                                string singlePartOutputDirectory) {

        var machineResult = new MachineGenerationResult() {
            MachineName = options.Name,
            OptimizationResults = null,
            SinglePartResults = null
        };

        if (options.GenerateNestPrograms) {
            machineResult.OptimizationResults = cadcode.OptimizeNestedParts(options.GenerateLabels, nestOutputDirectory);
        }

        if (options.GenerateSinglePartPrograms) {
            machineResult.SinglePartResults = cadcode.SiglePartPrograms(singlePartOutputDirectory);
        }

        return machineResult;

    }

}
