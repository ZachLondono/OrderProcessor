using ApplicationCore.Features.CNC.ReleasePDF;
using ApplicationCore.Features.Orders.Shared.Domain;
using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using ApplicationCore.Shared;
using ApplicationCore.Shared.CNC.Job;
using ApplicationCore.Shared.CNC.WSXML;
using ApplicationCore.Shared.Domain;
using ApplicationCore.Shared.Settings.CNC;
using ApplicationCore.Shared.Settings.Tools;
using CADCodeProxy;
using CADCodeProxy.CNC;
using CADCodeProxy.Machining;
using Microsoft.Extensions.Options;

namespace ApplicationCore.Features.Orders.OrderRelease.Handlers.GCode;

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

    public async Task<ReleasedJob?> GenerateGCode(IEnumerable<Order> orders, string customerName, string vendorName) {

        if (!orders.Any()) {
            return null;
        }

        var firstOrder = orders.First();

        var parts = orders.SelectMany(o => o.Products)
                        .OfType<ICNCPartContainer>()
                        .Where(p => p.ContainsCNCParts())
                        .SelectMany(p => p.GetCNCParts(customerName))
                        .ToArray();

        if (!parts.Any()) {
            return null;
        }

        var machines = _cncSettings.MachineSettings
                                    .Select(kv => new Machine() {
                                        Name = kv.Key,
                                        ToolFilePath = kv.Value.ToolFile,
                                        NestOutputDirectory = kv.Value.NestOutputDirectory,
                                        SingleProgramOutputDirectory = kv.Value.SingleProgramOutputDirectory,
                                        PictureOutputDirectory = kv.Value.PictureOutputDirectory,
                                        LabelDatabaseOutputDirectory = kv.Value.LabelDatabaseOutputDirectory,
                                    });

        Batch batch = new() {
            Name = $"{firstOrder.Number} - {firstOrder.Name}",
            Parts = parts,
            InfoFields = new()
        };

        var generator = new GCodeGenerator(CADCodeProxy.Enums.LinearUnits.Millimeters);

        var defaultInventorySize = _cncSettings.DefaultInventorySize;

        parts.Select(p => (p.Material, p.Thickness))
            .Distinct()
            .ForEach(material => {

                if (_cncSettings.Inventory.TryGetValue(material.Material, out var item)
                    && item.Thickness == material.Thickness) {

                    item.Sizes
                        .ForEach(size => {

                            var invItem = new CADCodeProxy.CNC.InventoryItem() {
                                MaterialName = material.Material,
                                AvailableQty = 9999,
                                IsGrained = true,
                                PanelLength = size.Length,
                                PanelWidth = size.Width,
                                PanelThickness = material.Thickness,
                                Priority = size.Priority,
                            };

                            generator.Inventory.Add(invItem);

                        });

                } else {

                    var invItem = new CADCodeProxy.CNC.InventoryItem() {
                        MaterialName = material.Material,
                        AvailableQty = 9999,
                        IsGrained = true,
                        PanelLength = defaultInventorySize.Length,
                        PanelWidth = defaultInventorySize.Width,
                        PanelThickness = material.Thickness,
                        Priority = 1,
                    };
                    generator.Inventory.Add(invItem);

                }

            });

        ShowProgressBar?.Invoke();

        if (SetProgressBarValue is not null) generator.CADCodeProgressEvent += SetProgressBarValue.Invoke;
        if (OnError is not null) generator.CADCodeErrorEvent += (msg) => OnError.Invoke(msg);
        if (OnProgressReport is not null) generator.GenerationEvent += OnProgressReport.Invoke;

        var result = await Task.Run(() => generator.GeneratePrograms(machines, batch, ""));
        DateTime timestamp = DateTime.Now;

        HideProgressBar?.Invoke();

        static string GetImageFileName(string patternName) {

            int idx = patternName.IndexOf('.');
            if (idx < 0) {
                return patternName;
            }

            return patternName[..idx];

        }

        var releases = result.MachineResults.Select(machineResult => {

            var programs = machineResult.MaterialGCodeGenerationResults
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
                                                    ProductNumber = label.GetValueOrEmpty("Cabinet Number"),
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


            var orientation = ApplicationCore.Shared.CNC.Domain.TableOrientation.Standard;
            if (_cncSettings.MachineSettings.TryGetValue(machineResult.MachineName, out var settings) && settings.IsTableRotated) {
                orientation = ApplicationCore.Shared.CNC.Domain.TableOrientation.Rotated;
            }

            // TODO: add figure out how to check if the single part programs where actually generated
            var singlePrograms = batch.Parts
                                        .SelectMany(p => {

                                            if (!p.InfoFields.TryGetValue("Description", out string? description)) {
                                                description = "";
                                            }

                                            if (!p.InfoFields.TryGetValue("Cabinet Number", out string? productNumber)) {
                                                productNumber = "";
                                            }

                                            string partId = p.Id.ToString();
                                            var width = Dimension.FromMillimeters(p.Width);
                                            var length = Dimension.FromMillimeters(p.Length);
                                            bool hasBackSideProgram = false;

                                            List<SinglePartProgram> programs = new() {
                                                new SinglePartProgram() {
                                                    FileName = p.PrimaryFace.ProgramName,
                                                    Name = p.PrimaryFace.ProgramName,
                                                    Description = description ?? "",
                                                    HasBackSideProgram = hasBackSideProgram,
                                                    Width = width,
                                                    Length = length,
                                                    ProductNumber = productNumber ?? "",
                                                    PartId = partId
                                                }
                                            };

                                            if (p.SecondaryFace is not null) {

                                                programs.Add(new SinglePartProgram() {
                                                    FileName = p.SecondaryFace.ProgramName,
                                                    Name = p.SecondaryFace.ProgramName,
                                                    Description = description ?? "",
                                                    HasBackSideProgram = hasBackSideProgram,
                                                    Width = width,
                                                    Length = length,
                                                    ProductNumber = productNumber ?? "",
                                                    PartId = partId
                                                });

                                            }

                                            return programs;

                                        });

            var usedToolNames = batch.Parts
                                        .SelectMany(p => new PartFace?[] { p.PrimaryFace, p.SecondaryFace })
                                        .OfType<PartFace>()
                                        .SelectMany(f => f.Tokens)
                                        .Select(t => t.ToolName)
                                        .Distinct();

            var toolTable = CreateMachineToolTable(machineResult.MachineName, _toolConfiguration.MachineToolMaps, usedToolNames);

            return new MachineRelease() {
                MachineName = machineResult.MachineName,
                MachineTableOrientation = orientation,
                ToolTable = toolTable,
                Programs = programs,
                SinglePrograms = singlePrograms
            };

        });

        return new ReleasedJob() {
            JobName = batch.Name,
            CustomerName = customerName,
            VendorName = vendorName,
            TimeStamp = timestamp,
            OrderDate = firstOrder.OrderDate,
            ReleaseDate = DateTime.Today,
            DueDate = firstOrder.DueDate,
            Releases = releases
        };

    }

    public static IReadOnlyDictionary<int, string> CreateMachineToolTable(string machineName, IEnumerable<MachineToolMap> toolMaps, IEnumerable<string> usedToolNames) {

        var toolTable = new Dictionary<int, string>();

        var machineCarousel = toolMaps.FirstOrDefault(c => c.MachineName == machineName);

        if (machineCarousel is null) return toolTable;

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
