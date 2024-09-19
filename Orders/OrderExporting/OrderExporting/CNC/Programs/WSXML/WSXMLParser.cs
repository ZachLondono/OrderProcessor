using Domain.ValueObjects;
using Microsoft.Extensions.Options;
using System.Xml.Linq;
using Domain.Extensions;
using OrderExporting.CNC.Programs.Domain;
using OrderExporting.CNC.Programs.Job;
using OrderExporting.CNC.Programs.WSXML.Report;
using ApplicationCore.Shared.Settings.Tools;

namespace OrderExporting.CNC.Programs.WSXML;

public partial class WSXMLParser : IWSXMLParser {

    private readonly ToolConfiguration _toolConfiguration;

    public WSXMLParser(IOptions<ToolConfiguration> toolConfiguration) {
        _toolConfiguration = toolConfiguration.Value;
    }

    public static WSXMLReport? ParseWSXMLReport(string reportFilePath) {

        XDocument xdoc = XDocument.Load(reportFilePath);
        if (xdoc.Root is null) {
            Console.WriteLine("No root found");
            return null;
        }

        var timestamp = new FileInfo(reportFilePath).LastWriteTime;

        var job = xdoc.Root.Element("Job");
        if (job is null) {
            Console.WriteLine("No job found");
            return null;
        }

        var manufacturing = job.Element("Manufacturing");
        if (manufacturing is null) {
            Console.WriteLine("No manufacturing data found");
            return null;
        }

        var nestedParts = job.Elements("Item")
                           .Where(item => item.ElementValue("Note") == "Nested blocknest" || item.ElementValue("Description") == "blocknest")
                           .Where(nest => nest.Elements("Part").Any())
                           .SelectMany(item =>
                                item.Elements("Part")
                                            .Select(Part.FromXElement)
                                            .ToList())
                           .DistinctBy(part => part.Id)
                           .ToDictionary(part => part.Id, part => part);

        var singleParts = job.Elements("Item")
                           .Where(item => item.ElementValue("Note") == "Single Program Code" || item.ElementValue("Description") == "Single Part Programs")
                           .Where(nest => nest.Elements("Part").Any())
                           .SelectMany(item =>
                                item.Elements("Part")
                                            .Select(Part.FromXElement)
                                            .ToList())
                           .DistinctBy(part => part.Id)
                           .ToDictionary(part => part.Id, part => part);

        var allParts = new Dictionary<string, Part>();
        nestedParts.ForEach(kv => allParts[kv.Key] = kv.Value);
        singleParts.ForEach(kv => allParts[kv.Key] = kv.Value);

        var patternSchedules = manufacturing.Elements("PatternSchedule")
                                            .Select(PatternSchedule.FromXElement)
                                            .ToArray();

        var items = job.Elements("Item")
                        .Select(Item.FromXElement)
                        .ToArray();

        var materials = job.Elements("Material")
                            .Select(MaterialRecord.FromXElement)
                            .ToDictionary(m => m.Id);

        var labels = manufacturing.Elements("Label")
                                    .Select(PartLabels.FromXElement)
                                    .ToDictionary(l => l.Id);

        var operationGroups = manufacturing.Elements("OperationGroups")
                                            .Select(OperationGroups.FromXElement)
                                            .ToArray();

        var report = new WSXMLReport() {
            JobName = job.ElementValue("Name"),
            Parts = allParts,
            PatternSchedules = patternSchedules,
            Items = items,
            Materials = materials,
            PartLabels = labels,
            OperationGroups = operationGroups,
            TimeStamp = timestamp
        };

        return report;

    }

    public ReleasedJob MapDataToReleasedJob(WSXMLReport report, DateTime orderDate, DateTime? dueDate, string customerName, string vendorName) {

        var allToolNames = report.OperationGroups.Where(g => g.PartId is not null)
                                        .SelectMany(g => g.ToolName)
                                        .Distinct();

        List<MachineRelease> releases = report.PatternSchedules
                                                .GroupBy(sched => GetMachineName(sched.Name))
                                                .Select(group => new MachineRelease() {
                                                    MachineName = group.Key,
                                                    ToolTable = CreateMachineToolTable(group.Key, _toolConfiguration.MachineToolMaps, allToolNames),
                                                    MachineTableOrientation = GetTableOrientationFromMachineName(group.Key),
                                                    Programs = group.SelectMany(group => group.Patterns)
                                                                    .Select(pattern => {

                                                                        var material = report.Materials[pattern.MaterialId];
                                                                        string materialName = material.Name;
                                                                        if (PSIMaterial.TryParse(materialName, out var psiMat)) {
                                                                            materialName = psiMat.GetSimpleName();
                                                                        }

                                                                        double area = material.XDim * material.YDim;
                                                                        double usedArea = pattern.Parts
                                                                                              .Select<PatternPart, (int qty, Part part)>(part => (part.Locations.Count(), report.Parts[part.PartId]))
                                                                                              .Sum(data => data.qty * data.part.Width * data.part.Length);
                                                                        double yield = usedArea / area;

                                                                        return new ReleasedProgram() {
                                                                            Name = pattern.Name,
                                                                            HasFace6 = false,
                                                                            ImagePath = $"y:\\CADCode\\pix\\{GetImageFileName(pattern.Name)}.wmf",
                                                                            Material = new() {
                                                                                IsGrained = false,
                                                                                Yield = yield,
                                                                                Name = materialName,
                                                                                Width = material.YDim,
                                                                                Length = material.XDim,
                                                                                Thickness = material.ZDim
                                                                            },
                                                                            Parts = pattern.Parts
                                                                                            .SelectMany(part => {
                                                                                                List<(string partId, PatternPartLocation location)> points = new();
                                                                                                part.Locations.ToList().ForEach(loc => points.Add((part.PartId, loc)));
                                                                                                return points;
                                                                                            })
                                                                                            .Select(nestPart => {
                                                                                                var part = report.Parts[nestPart.partId];
                                                                                                var label = report.PartLabels[part.LabelId];
                                                                                                if (!Guid.TryParse(label.Fields.GetValueOrEmpty("Comment2"), out Guid productId)) {
                                                                                                    productId = Guid.Empty;
                                                                                                }
                                                                                                return new NestedPart() {
                                                                                                    Name = part.Name,
                                                                                                    FileName = part.Variables.ContainsKey("Face5Filename") ? part.Variables.GetValueOrEmpty("Face5Filename") : part.Name,
                                                                                                    Width = Dimension.FromMillimeters(part.Width),
                                                                                                    Length = Dimension.FromMillimeters(part.Length),
                                                                                                    Center = new() {
                                                                                                        X = nestPart.location.Insert.X + (nestPart.location.IsRotated ? part.Width : part.Length) / 2,
                                                                                                        Y = nestPart.location.Insert.Y + (nestPart.location.IsRotated ? part.Length : part.Width) / 2
                                                                                                    },
                                                                                                    IsRotated = nestPart.location.IsRotated,
                                                                                                    Description = label.Fields.GetValueOrEmpty("Description"),
                                                                                                    ProductNumber = label.Fields.GetValueOrEmpty("Cabinet Number"),
                                                                                                    ProductId = productId,
                                                                                                    PartId = nestPart.partId,
                                                                                                    ImageData = label.Fields.GetValueOrEmpty("Machining Picture"),
                                                                                                    HasFace6 = part.Variables.ContainsKey("Face6FileName"),
                                                                                                    Face6FileName = part.Variables.ContainsKey("Face6FileName") ? part.Variables["Face6FileName"] : null,
                                                                                                    HasBackSideProgram = label.Fields.GetValueOrEmpty("HasBackSideProgram") == "Y",
                                                                                                    Note = label.Fields.GetValueOrEmpty("PEFinishedSide"),
                                                                                                    Length1EdgeBanding = label.Fields.GetValueOrDefault("Length Color 1"),
                                                                                                    Length2EdgeBanding = label.Fields.GetValueOrDefault("Length Color 2"),
                                                                                                    Width1EdgeBanding = label.Fields.GetValueOrDefault("WidthColor 1"),
                                                                                                    Width2EdgeBanding = label.Fields.GetValueOrDefault("WidthColor 2")
                                                                                                };
                                                                                            })
                                                                                            .ToList()
                                                                        };
                                                                    })
                                                }).ToList();

        var singleParts = report.Items
                                .Where(item => item.Note == "Single Program Code" || item.Description == "Single Part Programs")
                                .GroupBy(item => GetMachineName(item.Name))
                                .ToDictionary(
                                    group => group.Key,
                                    group => group.SelectMany(item => item.PartIds.Select(id => report.Parts[id]))
                                                    .Select(part => {
                                                        var label = report.PartLabels[part.LabelId];
                                                        return new SinglePartProgram() {
                                                            Name = part.Name,
                                                            FileName = part.Variables.ContainsKey("Face5Filename") ? part.Variables.GetValueOrEmpty("Face5Filename") : part.Name,
                                                            Width = Dimension.FromMillimeters(part.Width),
                                                            Length = Dimension.FromMillimeters(part.Length),
                                                            Description = label.Fields.GetValueOrEmpty("Description"),
                                                            PartId = part.Id,
                                                            ProductNumber = label.Fields.GetValueOrEmpty("Cabinet Number"),
                                                            HasBackSideProgram = label.Fields.GetValueOrEmpty("HasBackSideProgram") == "Y"
                                                        };
                                                    })
                                 );

        foreach (var machineRelease in releases) {
            if (singleParts.TryGetValue(machineRelease.MachineName, out IEnumerable<SinglePartProgram>? parts)) {
                machineRelease.SinglePrograms = parts;
            }
        }

        foreach (var machineRelease in releases) {

            var twoSidedPrograms = machineRelease.Programs
                                                .GroupBy(p => p.Name[1..])
                                                .Where(g => g.Count() == 2);

            List<ReleasedProgram> programs = new(machineRelease.Programs);

            foreach (var group in twoSidedPrograms) {

                var face5Program = group.Where(p => p.Name[0] != '6').FirstOrDefault();

                if (face5Program is null) continue;

                programs.Where(p => p.Name == face5Program.Name).First().HasFace6 = true;

            }

            programs.RemoveAll(p => p.Name[0] == '6');

            machineRelease.Programs = programs;

        }

        var releasedJob = new ReleasedJob() {
            JobName = report.JobName,
            OrderDate = orderDate,
            ReleaseDate = DateTime.Now,
            DueDate = dueDate,
            CustomerName = customerName,
            VendorName = vendorName,
            Releases = releases,
            TimeStamp = report.TimeStamp
        };

        return releasedJob;

    }

    private static IReadOnlyDictionary<int, string> CreateMachineToolTable(string machineName, IEnumerable<MachineToolMap> toolMaps, IEnumerable<string> usedToolNames) {

        var toolTable = new Dictionary<int, string>();

        var machineCarousel = toolMaps.FirstOrDefault(c => c.MachineName == machineName);

        if (machineCarousel is null) return toolTable;

        for (int i = 0; i < machineCarousel.ToolPositionCount; i++) {
            toolTable[i + 1] = "";
        }

        machineCarousel.Tools = machineCarousel.Tools.Select(t => new Tool() {
            Position = t.Position,
            Name = t.Name.Trim(),
            AlternativeNames = t.AlternativeNames.Select(t => t.Trim()).ToList()
        }).ToList();

        foreach (var toolName in usedToolNames.Select(t => t.Trim())) {

            foreach (var tool in machineCarousel.Tools) {

                if (string.Equals(toolName, tool.Name, StringComparison.OrdinalIgnoreCase)
                    || tool.AlternativeNames.Contains(toolName, StringComparer.OrdinalIgnoreCase)) {
                    toolTable[tool.Position] = tool.Name;
                    break;
                }

            }

        }

        return toolTable;

    }

    private static string GetMachineName(string scheduleName) {

        if (scheduleName.ToUpper().Contains("OMNITECH")) return "OMNITECH";
        if (scheduleName.ToUpper().Contains("ANDI STRATOS") || scheduleName.ToUpper().Contains("ANDI")) return "ANDI STRATOS";
        return "UNKNOWN";

    }

    private static TableOrientation GetTableOrientationFromMachineName(string machineName) => machineName switch {
        "OMNITECH" => TableOrientation.Rotated,
        _ => TableOrientation.Standard
    };

    private static string GetImageFileName(string patternName) {

        int idx = patternName.IndexOf('.');
        if (idx < 0) {
            return patternName;
        }

        return patternName[..idx];

    }

}
