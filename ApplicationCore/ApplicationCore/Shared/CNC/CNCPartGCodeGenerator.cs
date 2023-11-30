using ApplicationCore.Features.CNC.ReleasePDF;
using ApplicationCore.Shared.CNC.Domain;
using ApplicationCore.Shared.CNC.Job;
using ApplicationCore.Shared.Domain;
using ApplicationCore.Shared.Settings.CNC;
using ApplicationCore.Shared.Settings.Tools;
using CADCodeProxy;
using CADCodeProxy.CNC;
using CADCodeProxy.Machining;
using CADCodeProxy.Results;
using Microsoft.Extensions.Options;

namespace ApplicationCore.Shared.CNC;

public class CNCPartGCodeGenerator {

    public Action<string>? OnProgressReport;
    public Action? ShowProgressBar;
    public Action? HideProgressBar;
    public Action<int>? SetProgressBarValue;
    public Action<string>? OnError;

    private readonly CNCSettings _cncSettings;
    private readonly ToolConfiguration _toolConfiguration;

    public CNCPartGCodeGenerator(IOptions<CNCSettings> cncOptions, IOptions<ToolConfiguration> toolOptions) {
        _cncSettings = cncOptions.Value;
        _toolConfiguration = toolOptions.Value;
    }

    public async Task<ReleasedJob?> GenerateGCode(Batch batch, string customerName, string vendorName, DateTime orderDate, DateTime? dueDate) {

        var generator = new GCodeGenerator(CADCodeProxy.Enums.LinearUnits.Millimeters);

        if (SetProgressBarValue is not null) generator.CADCodeProgressEvent += SetProgressBarValue.Invoke;
        if (OnError is not null) generator.CADCodeErrorEvent += OnError.Invoke;
        if (OnProgressReport is not null) generator.GenerationEvent += OnProgressReport.Invoke;

        GetInventoryForBatch(batch).ForEach(generator.Inventory.Add);

        var machines = GetMachinesToReleaseTo();

        ShowProgressBar?.Invoke();

        var result = await Task.Run(() => generator.GeneratePrograms(machines, batch, ""));
        DateTime timestamp = DateTime.Now;

        HideProgressBar?.Invoke();

        var machineToolMaps = _toolConfiguration.MachineToolMaps.ToDictionary(m => m.MachineName, m => m);
        var machineSettings = _cncSettings.MachineSettings;

        var usedToolNames = GetUsedToolNamesFromBatch(batch);

        var releases = result.MachineResults
                             .Select(machineResult => GetMachineRelease(machineResult, machineToolMaps, machineSettings, usedToolNames));

        return new ReleasedJob() {
            JobName = batch.Name,
            CustomerName = customerName,
            VendorName = vendorName,
            TimeStamp = timestamp,
            OrderDate = orderDate,
            ReleaseDate = DateTime.Today,
            DueDate = dueDate,
            Releases = releases
        };

    }

    private List<CADCodeProxy.CNC.InventoryItem> GetInventoryForBatch(Batch batch) {

        List<CADCodeProxy.CNC.InventoryItem> inventory = [];

        var partMaterials = batch.Parts.Select(p => (p.Material, p.Thickness)).Distinct().ToList();
        foreach (var (name, item) in _cncSettings.Inventory) {

            if (partMaterials.Contains((name, item.Thickness)) && item.Sizes.Any()) {
                partMaterials.Remove((name, item.Thickness));
            }

            foreach (var size in item.Sizes) {
                inventory.Add(new CADCodeProxy.CNC.InventoryItem() {
                    MaterialName = name,
                    AvailableQty = 9999,
                    IsGrained = true,
                    PanelLength = size.Length,
                    PanelWidth = size.Width,
                    PanelThickness = item.Thickness,
                    Priority = size.Priority,
                });
            }

        }

        inventory.AddRange(GetDefaultInventoryItems(partMaterials, _cncSettings.DefaultInventorySize));

        return inventory;

    }

    private IEnumerable<Machine> GetMachinesToReleaseTo() {
        return _cncSettings.MachineSettings
                           .Select(kv => new Machine() {
                               Name = kv.Key,
                               ToolFilePath = kv.Value.ToolFile,
                               NestOutputDirectory = kv.Value.NestOutputDirectory,
                               SingleProgramOutputDirectory = kv.Value.SingleProgramOutputDirectory,
                               PictureOutputDirectory = kv.Value.PictureOutputDirectory,
                               LabelDatabaseOutputDirectory = kv.Value.LabelDatabaseOutputDirectory,
                           })
                           .ToList();
    }

    private static MachineRelease GetMachineRelease(MachineGCodeGenerationResult machineResult, Dictionary<string, MachineToolMap> machineToolMaps, Dictionary<string, MachineSettings> machineSettings, IEnumerable<string> usedToolNames) {

        // TODO: get used tool names from result

        var programs = GetProgramsFromResult(machineResult);
        var singlePrograms = GetSinglePartPrograms(machineResult);

        var machineCarousel = machineToolMaps.GetValueOrDefault(machineResult.MachineName);
        var toolTable = machineCarousel is null ? new() : CreateMachineToolTable(machineCarousel, usedToolNames);

        var orientation = TableOrientation.Standard;
        if (machineSettings.GetValueOrDefault(machineResult.MachineName)?.IsTableRotated ?? false) {
            orientation = TableOrientation.Rotated;
        }

        return new MachineRelease() {
            MachineName = machineResult.MachineName,
            MachineTableOrientation = orientation,
            ToolTable = toolTable,
            Programs = programs,
            SinglePrograms = singlePrograms
        };
    }

    private static SinglePartProgram[] GetSinglePartPrograms(MachineGCodeGenerationResult machineResult) {

        // TODO: The machine generation result should have a property that lists all single programs that where successfully generated - not just placed parts which may not have a single program associated with it
        // TODO: PlacedParts only includes Face 5 Parts, but face 6 programs should contain face 6 programs as well (maybe PlacedParts should also show if there was a face 6 program generated for this part)

        var labels = machineResult.MaterialGCodeGenerationResults
                                   .SelectMany(re => re.PartLabels)
                                   .DistinctBy(l => l.PartId)
                                   .ToDictionary(l => l.PartId, l => l);

        return machineResult.MaterialGCodeGenerationResults
                       .SelectMany(matResult => matResult.PlacedParts)
                       .DistinctBy(part => part.Name)
                       .Select(part => {

                           string description = "", productNumber = "";
                           bool hasBackSideProgram = false;
                           if (labels.TryGetValue(part.PartId, out PartLabel? label)) {
                               description = label.Fields.GetValueOrEmpty("Description");
                               productNumber = label.Fields.GetValueOrEmpty("CabinetNumber");
                               hasBackSideProgram = label.Fields.GetValueOrEmpty("HasBackSideProgram") == "Y";
                           }

                           return new SinglePartProgram() {
                               Name = part.Name,     // TODO: this should be the part name, not the specific program name. Although there is no concept of a "Part Name" in the CADCode proxy class library
                               FileName = part.Name,
                               Width = Dimension.FromMillimeters(part.Width),
                               Length = Dimension.FromMillimeters(part.Length),
                               Description = description,
                               PartId = part.PartId.ToString(),
                               ProductNumber = productNumber,
                               HasBackSideProgram = hasBackSideProgram
                           };
                       })
                       .ToArray();

    }

    private static string GetImageFileName(string patternName) {

        int idx = patternName.IndexOf('.');
        if (idx < 0) {
            return patternName;
        }

        return patternName[..idx];

    }

    private static IEnumerable<ReleasedProgram> GetProgramsFromResult(MachineGCodeGenerationResult machineResult) {
        return machineResult.MaterialGCodeGenerationResults
                            .SelectMany(genResult => {

                                var labelsByPartId = genResult.PartLabels.ToDictionary(pl => pl.PartId, pl => pl.Fields);

                                return genResult.ProgramNames
                                        .Select((program, idx) => {

                                            var parts = genResult.PlacedParts.Where(p => p.ProgramIndex == idx).ToList();

                                            int inventoryIndex = parts.First().UsedInventoryIndex; // TODO: get inventory index for program name
                                            var inventory = genResult.UsedInventory[inventoryIndex];

                                            var area = inventory.Width * inventory.Length;
                                            var usedArea = parts.Sum(part => part.Width * part.Length);
                                            var yield = usedArea / area;

                                            string materialName = genResult.MaterialName;
                                            if (PSIMaterial.TryParse(materialName, out var psiMat)) {
                                                materialName = psiMat.GetSimpleName();
                                            }

                                            return new ReleasedProgram() {
                                                Name = program,
                                                ImagePath = @$"C:\Users\Zachary Londono\Desktop\CC Output\{GetImageFileName(program)}.wmf",
                                                HasFace6 = false,
                                                Material = new() {
                                                    Name = materialName,
                                                    Width = inventory.Width,
                                                    Length = inventory.Length,
                                                    Thickness = inventory.Thickness,
                                                    IsGrained = inventory.IsGrained,
                                                    Yield = yield
                                                },
                                                Parts = parts.Select(placedPart => {

                                                    var label = labelsByPartId[placedPart.PartId];

                                                    return new NestedPart() {
                                                        Name = placedPart.Name,
                                                        FileName = label.GetValueOrEmpty("Face5Filename"),
                                                        HasFace6 = false,
                                                        Face6FileName = null,
                                                        ImageData = "",
                                                        Width = Dimension.FromMillimeters(placedPart.Width),
                                                        Length = Dimension.FromMillimeters(placedPart.Length),
                                                        Description = label.GetValueOrEmpty("Description"),
                                                        Center = new() {
                                                            X = placedPart.InsertionPoint.X + (placedPart.IsRotated ? placedPart.Width : placedPart.Length) / 2,
                                                            Y = placedPart.InsertionPoint.Y + (placedPart.IsRotated ? placedPart.Length : placedPart.Width) / 2
                                                        },
                                                        ProductNumber = label.GetValueOrEmpty("CabinetNumber"),
                                                        ProductId = Guid.Empty, // TODO: find a way to get thr product id
                                                        PartId = placedPart.PartId.ToString(), //placedPart.Id, TODO: cadcode generated id 
                                                        IsRotated = placedPart.IsRotated,
                                                        HasBackSideProgram = false,
                                                        Note = label.GetValueOrEmpty("PEFinishedSide")
                                                    };
                                                })
                                                .ToList()
                                            };

                                        });

                            });
    }

    private static IEnumerable<string> GetUsedToolNamesFromBatch(Batch batch) {
        return batch.Parts
                    .SelectMany(p => new PartFace?[] { p.PrimaryFace, p.SecondaryFace })
                    .OfType<PartFace>()
                    .SelectMany(f => f.Tokens)
                    .OfType<IMachiningOperation>()
                    .Select(t => t.ToolName)
                    .Distinct();
    }

    private IEnumerable<CADCodeProxy.CNC.InventoryItem> GetDefaultInventoryItems(IEnumerable<(string Material, double Thickness)> partMaterials, InventorySize defaultInventorySize) {
        return partMaterials
            .SelectMany(material => {

                if (_cncSettings.Inventory.TryGetValue(material.Material, out var item)
                    && item.Thickness == material.Thickness) {

                    return item.Sizes
                                .Select(size =>
                                    new CADCodeProxy.CNC.InventoryItem() {
                                        MaterialName = material.Material,
                                        AvailableQty = 9999,
                                        IsGrained = true,
                                        PanelLength = size.Length,
                                        PanelWidth = size.Width,
                                        PanelThickness = material.Thickness,
                                        Priority = size.Priority,
                                    });

                }

                var invItem = new CADCodeProxy.CNC.InventoryItem() {
                    MaterialName = material.Material,
                    AvailableQty = 9999,
                    IsGrained = true,
                    PanelLength = defaultInventorySize.Length,
                    PanelWidth = defaultInventorySize.Width,
                    PanelThickness = material.Thickness,
                    Priority = 1,
                };

                return new[] { invItem };

            });
    }

    public static Dictionary<int, string> CreateMachineToolTable(MachineToolMap machineCarousel, IEnumerable<string> usedToolNames) {

        var toolTable = new Dictionary<int, string>();

        for (int i = 0; i < machineCarousel.ToolPositionCount; i++) {
            toolTable[i + 1] = "";
        }

        foreach (var toolName in usedToolNames) {

            foreach (var tool in machineCarousel.Tools) {

                if (string.Equals(toolName, tool.Name, StringComparison.OrdinalIgnoreCase) || tool.AlternativeNames.Contains(toolName, StringComparer.OrdinalIgnoreCase)) {
                    toolTable[tool.Position] = tool.Name;
                    break;
                }

            }

        }

        return toolTable;

    }

}
