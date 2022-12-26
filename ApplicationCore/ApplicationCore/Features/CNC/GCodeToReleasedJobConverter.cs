using ApplicationCore.Features.CNC.GCode.Contracts;
using ApplicationCore.Features.CNC.GCode.Domain;
using ApplicationCore.Features.CNC.ReleasePDF.Contracts;
using ApplicationCore.Features.CNC.GCode.Contracts.Results;

namespace ApplicationCore.Features.CNC;

public class GCodeToReleasedJobConverter {

	public static ReleasedJob ConvertResult(IEnumerable<MachineGenerationResult> machineGenerationResults, string jobName, IEnumerable<Part> parts) {

		List<MachineRelease> releases = new();

		foreach (var machine in machineGenerationResults) {
			if (machine.OptimizationResults is null) continue;
			var optimizationResults = new List<OptimizationResult>() { machine.OptimizationResults };
			var release = GetMachineRelease("Y:\\CADCode\\pix", machine.MachineName, TableOrientation.Standard, optimizationResults, parts);
			releases.Add(release);			
		}

		return new ReleasedJob() {
			JobName = jobName,
			Releases = releases
		};

	}

	private static MachineRelease GetMachineRelease(string pictureFileOutputDirectory, string machineName, TableOrientation tableOrientation, IEnumerable<OptimizationResult> optiResult, IEnumerable<Part> allParts) {

		var partDict = allParts.DistinctBy(p => p.PrimaryFace.FileName).ToDictionary(p => p.PrimaryFace.FileName, p => p);

		var programs = new List<ReleasedProgram>();
		var toolTable = new Dictionary<int, string>();

		foreach (var result in optiResult) {
			if (result.Programs.Length != 0) {
				var matPrograms = result.PlacedParts.Where(p => p.ProgramIndex != -1).GroupBy(p => p.ProgramIndex).Select(g => {
					var programName = result.Programs[g.Key];
					var inventory = result.UsedInventory[g.First().UsedInventoryIndex];
					var totalArea = inventory.Width * inventory.Length;
					var usedArea = g.Sum(p => p.Area);

					// TODO: get image file path from OptimizationResult
					return new ReleasedProgram() {
						Name = programName,
						ImagePath = Path.Combine(pictureFileOutputDirectory, $"{Path.GetFileNameWithoutExtension(programName)}.wmf"),
						Material = new() {
							Name = inventory.Name,
							Width = inventory.Width,
							Length = inventory.Length,
							Thickness = inventory.Thickness,
							IsGrained = inventory.IsGrained,
							Yield = usedArea / totalArea
						},
						Parts = g.Select(p => {
							var cncpart = partDict[p.FileName];
							var xOffset = (float)((p.IsRotated ? cncpart.Width.AsMillimeters() : cncpart.Length.AsMillimeters()) / 2);
							var yOffset = (float)((p.IsRotated ? cncpart.Length.AsMillimeters() : cncpart.Width.AsMillimeters()) / 2);
							return new NestedPart() {
								Name = cncpart.PrimaryFace.FileName,
								ImagePath = Path.Combine(pictureFileOutputDirectory, $"{cncpart.PrimaryFace.FileName}.wmf"),
								Description = cncpart.Description,
								Width = cncpart.Width,
								Length = cncpart.Length,
								Center = new() {
									X = p.InsertionPoint.X + xOffset,
									Y = p.InsertionPoint.Y + yOffset
								}
							};
						}).ToList()
					};
				});
				programs.AddRange(matPrograms);
			}
		}

		var release = new MachineRelease() {
			MachineName = machineName,
			Programs = programs,
			ToolTable = toolTable,
			MachineTableOrientation = tableOrientation
		};

		return release;

	}

}