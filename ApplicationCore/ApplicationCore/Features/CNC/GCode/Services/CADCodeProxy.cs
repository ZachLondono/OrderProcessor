using ApplicationCore.Features.CNC.GCode.Configuration;
using ApplicationCore.Features.CNC.GCode.Contracts;
using ApplicationCore.Features.CNC.GCode.Contracts.Results;
using ApplicationCore.Features.CNC.GCode.Domain;
using ApplicationCore.Features.CNC.GCode.Domain.CADCode;

namespace ApplicationCore.Features.CNC.GCode.Services;

internal class CADCodeProxy : ICADCodeProxy {

	/*
	*  Defers the actual interaction to CADCode once it has already been set up and its data has been validated
	*/

	private readonly CADCodeConfiguration _configuration;

	public CADCodeProxy(CADCodeConfiguration configuration) {
		_configuration = configuration;
	}

	public void AddBatch(Batch batch) => throw new NotImplementedException();

	public void AddLabels(IEnumerable<Label> labels) => throw new NotImplementedException();

	public void AddInventory(IEnumerable<InventorySheetStock> inventory) => throw new NotImplementedException();

	public void AddTools(IEnumerable<Tool> tools) => throw new NotImplementedException();

	public OptimizationResult OptimizeNestedParts(bool generateLabels, string outputDirectory) => throw new NotImplementedException();

	public SinglePartGenerationResult SiglePartPrograms(string outputDirectory) => throw new NotImplementedException();

	public void Reset() => throw new NotImplementedException();

	public void OnProgress(Action<CADCodeProgress> onprogress) => throw new NotImplementedException();

	public void OnError(Action<CADCodeError> onerror) => throw new NotImplementedException();

}