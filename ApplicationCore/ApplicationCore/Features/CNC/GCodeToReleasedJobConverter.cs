using ApplicationCore.Features.CNC.GCode.Configuration;
using ApplicationCore.Features.CNC.GCode.Contracts;
using ApplicationCore.Features.CNC.GCode.Domain.CADCode;
using ApplicationCore.Features.CNC.GCode.Domain;
using ApplicationCore.Features.CNC.GCode.Services;
using ApplicationCore.Features.CNC.ReleasePDF.Contracts;

namespace ApplicationCore.Features.CNC;

public class GCodeToReleasedJobConverter {

	public ReleasedJob ConvertResult(GCodeGenerationResult result, string jobName, IEnumerable<CNCPart> parts) {

		List<MachineRelease> releases = new();

		foreach (var machine in result.MachineResults) {
			var release = GetMachineRelease(machine.PictureFileOutputDirectory, machine.MachineName, machine.TableOrientation, machine.OptimizationResults, parts);
			releases.Add(release);
		}

		return new ReleasedJob() {
			JobName = jobName,
			Releases = releases
		};

	}

	private static MachineRelease GetMachineRelease(string pictureFileOutputDirectory, string machineName, TableOrientation tableOrientation, IEnumerable<OptimizationResult> optiResult, IEnumerable<CNCPart> allParts) {

		var partDict = allParts.DistinctBy(p => p.FileName).ToDictionary(p => p.FileName, p => p);

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
							var xOffset = (float)((p.IsRotated ? cncpart.Width : cncpart.Length) / 2);
							var yOffset = (float)((p.IsRotated ? cncpart.Length : cncpart.Width) / 2);
							return new NestedPart() {
								Name = cncpart.FileName,
								ImagePath = Path.Combine(pictureFileOutputDirectory, $"{cncpart.FileName}.wmf"),
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