using ApplicationCore.Features.CADCode.Contracts;
using ApplicationCore.Features.CADCode.Contracts.ProgramRelease;
using ApplicationCore.Features.CADCode.Services.Domain;
using ApplicationCore.Features.CADCode.Services.Domain.CADCode;
using ApplicationCore.Features.CADCode.Services.Domain.CADCode.Configuration;
using ApplicationCore.Features.CADCode.Services.Services.CADCodeGCode.Configuration;
using ApplicationCore.Features.CADCode.Services.Services.CADCodeGCode.PDF;

namespace ApplicationCore.Features.CADCode.Services.Services.CADCodeGCode;

public class CADCodeGCodeCNCService : ICNCService {

    private readonly ICADCodeConfigurationProvider _configProvider;
    private readonly IInventoryService _inventoryService;
    private readonly ICADCodeMachineConfigurationProvider _ccmachineConfigProvider;

    public CADCodeGCodeCNCService(ICADCodeConfigurationProvider configProvider, IInventoryService inventoryService, ICADCodeMachineConfigurationProvider ccmachineConfigProvider) {
        _configProvider = configProvider;
        _inventoryService = inventoryService;
        _ccmachineConfigProvider = ccmachineConfigProvider;
    }

    public async Task<ReleasedJob> ExportToCNC(CNCBatch batch, IEnumerable<CNCMachineConfiguration> machineConfigs) {

        var cncConfig = _configProvider.GetConfiguration();
        var availableInventory = await _inventoryService.GetInventory();

        var ccmachineConfigs = _ccmachineConfigProvider.GetConfigurations();
        var ccmachineConfigDict = ccmachineConfigs.ToDictionary(c => c.MachineName, c => c);

        var machineReleases = await Task.Run<IEnumerable<MachineRelease>>(() => {

            var cadCode = new CADCodeManager(batch.Name, availableInventory);
            foreach (var part in batch.Parts)
                cadCode.AddPart(part);

            var machineReleases = new List<MachineRelease>();
            foreach (var machineConfig in machineConfigs) {

                if (!ccmachineConfigDict.TryGetValue(machineConfig.MachineName, out CADCodeMachineConfiguration? ccmachineconfig) || ccmachineconfig is null) {
                    Console.WriteLine($"No CADCode configuration for machine {machineConfig.MachineName}");
                    // TODO show warning for missing CADCode configuration
                    continue;
                }

                // TODO: pass machine name to function call so it can generate machine-specific results
                _ = cadCode.GenerateSinglePrograms(cncConfig, machineConfig, ccmachineconfig);
                var nestResults = cadCode.GenerateNestedCode(cncConfig, machineConfig, ccmachineconfig);

                machineReleases.Add(GetMachineRelease(cncConfig, machineConfig, nestResults, batch.Parts));
            }

            return machineReleases;

        });


		return new ReleasedJob() {
            JobName = batch.Name,
            Releases = machineReleases
        };

    }

    public static MachineRelease GetMachineRelease(CADCodeConfiguration ccConfig, CNCMachineConfiguration machineConfig, IEnumerable<OptimizationResult> optiResult, IReadOnlyList<CNCPart> allParts) {

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
                        ImagePath = Path.Combine(ccConfig.PictureFileOutputDirectory, $"{Path.GetFileNameWithoutExtension(programName)}.wmf"),
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
                                ImagePath = Path.Combine(ccConfig.PictureFileOutputDirectory, $"{cncpart.FileName}.wmf"),
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

            var toolsUsed = result.PlacedParts.Select(p => p.FileName).Distinct().Select(name => partDict[name]).SelectMany(p => p.Tokens).Select(t => t.Tool);
            foreach (var machiningTool in toolsUsed) {

                // TODO: find a better way to distinguish between a router bit and a drill bit
                // TODO: find a way to show which drill bits are being used. The tool map for the machine should have a drill block section with position numbers similar to the tool spindles.
                if (machiningTool.Name == "") continue;

                if (machineConfig.ToolMap.Tools.TryGetValue(machiningTool.Name, out (RouterBit tool, int position) toolSpec)) {

                    if (toolTable.ContainsKey(toolSpec.position)) {
                        if (!toolTable[toolSpec.position].Equals(machiningTool.Name)) {
                            Console.WriteLine($"Two toools in same position {machiningTool.Name} and {toolTable[toolSpec.position]}");
                            // TODO: warn the user that two tools from the same spindle where used
                        } else {
                            // do nothing
                        }
                    } else { 
                        toolTable.Add(toolSpec.position, machiningTool.Name);
                    }

                } else {

                    // TODO: warn that tool was not found in machine tool map

                }

            }
        }

        for (int i = 1; i <= machineConfig.ToolMap.SpindleCount; i++) {
            if (toolTable.ContainsKey(i)) continue;
            toolTable[i] = "";
        }

        var release = new MachineRelease() {
            MachineName = machineConfig.MachineName,
            Programs = programs,
            ToolTable = toolTable,
            MachineTableOrientation = machineConfig.Orientation
        };

        return release;

    }

}