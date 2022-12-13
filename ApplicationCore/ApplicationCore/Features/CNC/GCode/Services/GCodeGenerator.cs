using ApplicationCore.Features.CNC.GCode.Configuration;
using ApplicationCore.Features.CNC.GCode.Contracts;
using ApplicationCore.Features.CNC.GCode.Contracts.Options;
using ApplicationCore.Features.CNC.GCode.Contracts.Results;
using ApplicationCore.Features.CNC.GCode.Domain.CADCode;

namespace ApplicationCore.Features.CNC.GCode.Services;

public class GCodeGenerator {

	private readonly ICADCodeFactory _cadcodeFactory;
	private readonly IInventoryFileReader _inventoryReader;
	private readonly IToolFileReader _toolFileReader;
	private readonly GCodeGenerationConfiguration _configuration;

	public GCodeGenerator(ICADCodeFactory cadcodeFactory,
							IInventoryFileReader inventoryReader,
							IToolFileReader toolFileReader,
							GCodeGenerationConfiguration configuration) {
		_cadcodeFactory = cadcodeFactory;
		_inventoryReader = inventoryReader;
		_toolFileReader = toolFileReader;
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
			var tools = await _toolFileReader.GetAvailableToolsAsync(machineConfig.ToolFilePath);

			cadcode.AddBatch(batch);
			cadcode.AddLabels(labels);
			cadcode.AddTools(tools);
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
		// publish notification about progress
		throw new NotImplementedException();
	}

	private void OnCADCodeError(CADCodeError error) {
		// publish notification about error
		throw new NotImplementedException();
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
