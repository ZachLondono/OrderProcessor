using ApplicationCore.Features.CNC.GCode.Contracts;
using ApplicationCore.Features.CNC.GCode.Contracts.Results;
using ApplicationCore.Features.CNC.GCode.Domain;
using ApplicationCore.Features.CNC.GCode.Domain.CADCode;

namespace ApplicationCore.Features.CNC.GCode.Services;

public interface ICADCodeProxy {

	void SetBatch(Batch batch);

	void AddLabels(IEnumerable<Label> labels);

	void AddInventory(IEnumerable<InventorySheetStock> inventory);

	Task AddToolsAsync(string toolFilePath);

	OptimizationResult OptimizeNestedParts(bool generateLabels, string outputDirectory);

	SinglePartGenerationResult SiglePartPrograms(string outputDirectory);

	void Reset();

	void OnProgress(Action<CADCodeProgress> onprogress);

	void OnError(Action<CADCodeError> onerror);

}