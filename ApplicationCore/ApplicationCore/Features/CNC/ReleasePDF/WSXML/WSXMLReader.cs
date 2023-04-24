using ApplicationCore.Features.CNC.Contracts;
using ApplicationCore.Features.CNC.Domain;
using ApplicationCore.Features.Shared.Domain;
using ApplicationCore.Features.Tools.Contracts;
using System.Xml.Linq;

namespace ApplicationCore.Features.CNC.ReleasePDF.WSXML;

internal partial class WSXMLParser {

    public static ReleasedJob? ParseWSXMLReport(string reportFilePath, DateTime orderDate, string customerName, string vendorName, IEnumerable<ToolCarousel> toolCarousels) {

        XDocument xdoc = XDocument.Load(reportFilePath);

        if (xdoc.Root is null) {
            Console.WriteLine("No root found");
            return null;
        }

        var job = xdoc.Root.Element("Job");

        if (job is null) {
            Console.WriteLine("No job found");
            return null;
        }

        var nestItems = job.Elements("Item").Where(item => item.ElementValue("Note") == "Nested blocknest").Where(nest => nest.Elements("Part").Any());

        Dictionary<string, Part> allParts = new();

        var nests = nestItems.Select(item => {
            var parts = item.Elements("Part").Select(Part.FromXElemnt).ToList();
            parts.ForEach(part => allParts.TryAdd(part.Id, part));
            return new Nest(item.AttributeValue("ID"), item.ElementValue("Name"), parts);
        }).ToList();

        var manufacturing = job.Element("Manufacturing");

        if (manufacturing is null) {
            Console.WriteLine("No manufacturing data found");
            return null;
        }

        var patternScheduleItems = manufacturing.Elements("PatternSchedule");

        var patternSchedules = patternScheduleItems.Select(PatternSchedule.FromXElement);

        var materials = job.Elements("Material").Select(MaterialRecord.FromXElement).ToDictionary(m => m.Id);

        var labels = manufacturing.Elements("Label").Select(PartLabels.FromXElement).ToDictionary(l => l.Id);

        var operationGroups = manufacturing.Elements("OperationGroups").Select(OperationGroups.FromXElement);

        var allToolNames = operationGroups.Where(g => g.MfgOrientationId is not null)
                                        .SelectMany(g => g.ToolName)
                                        .Distinct();

        // TODO: get tools used by machine, so that they can be displayed individually if different
        List<Tool> usedTools = new();
        var allTools = toolCarousels.SelectMany(c => c.Tools).ToList();
        foreach (var toolName in allToolNames) {

            foreach (var tool in allTools) {
                if (tool.Name == toolName || tool.AlternativeNames.Contains(toolName)) {
                    usedTools.Add(tool);
                    break;
                }
            }

        }

        string jobName = job.ElementValue("Name");

        ReleasedJob releasedJob = MapDataToReleasedJob(orderDate, customerName, vendorName, toolCarousels, jobName, allParts, patternSchedules, materials, labels, allToolNames);

        return releasedJob;

    }

    private static ReleasedJob MapDataToReleasedJob(DateTime orderDate, string customerName, string vendorName, IEnumerable<ToolCarousel> toolCarousels, string jobName, Dictionary<string, Part> allParts, IEnumerable<PatternSchedule> patternSchedules, Dictionary<string, MaterialRecord> materials, Dictionary<string, PartLabels> labels, IEnumerable<string> allToolNames) {
        List<MachineRelease> releases = patternSchedules
                                                .GroupBy(sched => GetMachineName(sched.Name))
                                                .Select(group => new MachineRelease() {
                                                    MachineName = group.Key,
                                                    ToolTable = CreateMachineToolTable(group.Key, toolCarousels, allToolNames),
                                                    MachineTableOrientation = GetTableOrientationFromMachineName(group.Key),
                                                    Programs = group.SelectMany(group => group.Patterns)
                                                                    .Select(pattern => {
                                                                        var material = materials[pattern.MaterialId];
                                                                        return new ReleasedProgram() {
                                                                            Name = pattern.Name,
                                                                            HasFace6 = false,
                                                                            ImagePath = $"y:\\CADCode\\pix\\{GetImageFileName(pattern.Name)}.wmf",
                                                                            Material = new() {
                                                                                IsGrained = false,
                                                                                Yield = 0,
                                                                                Name = material.Name,
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
                                                                                                var part = allParts[nestPart.partId];
                                                                                                var label = labels[part.LabelId];
                                                                                                if (!Guid.TryParse(label.Fields.GetValueOrEmpty("Comment2"), out Guid productId)) {
                                                                                                    productId = Guid.Empty;
                                                                                                }
                                                                                                return new NestedPart() {
                                                                                                    Name = part.Name,
                                                                                                    FileName = GetFileNameFromPartLabels(label),
                                                                                                    Width = Dimension.FromMillimeters(part.Width),
                                                                                                    Length = Dimension.FromMillimeters(part.Length),
                                                                                                    Center = new() {
                                                                                                        X = nestPart.location.Insert.X + (nestPart.location.IsRotated ? part.Length : part.Width) / 2,
                                                                                                        Y = nestPart.location.Insert.Y + (nestPart.location.IsRotated ? part.Width : part.Length) / 2
                                                                                                    },
                                                                                                    IsRotated = nestPart.location.IsRotated,
                                                                                                    Description = label.Fields.GetValueOrEmpty("Description"),
                                                                                                    ProductNumber = label.Fields.GetValueOrEmpty("Cabinet Number"),
                                                                                                    ProductId = productId,
                                                                                                    PartId = nestPart.partId,
                                                                                                    ImageData = label.Fields.GetValueOrEmpty("Machining Picture"),
                                                                                                    HasFace6 = false
                                                                                                };
                                                                                            })
                                                                                            .ToList()
                                                                        };
                                                                    })
                                                }).ToList();

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
            JobName = jobName,
            OrderDate = orderDate,
            ReleaseDate = DateTime.Now,
            CustomerName = customerName,
            VendorName = vendorName,
            WorkOrderId = null,
            Releases = releases
        };
        return releasedJob;
    }

    private static IReadOnlyDictionary<int, string> CreateMachineToolTable(string machineName, IEnumerable<ToolCarousel> toolCarousels, IEnumerable<string> usedToolNames) {

        var toolTable = new Dictionary<int, string>();

        var machineCarousel = toolCarousels.FirstOrDefault(c => c.MachineName == machineName);

        if (machineCarousel is null) return toolTable;

        for (int i = 0; i < machineCarousel.PositionCount; i++) {
            toolTable[i + 1] = "";
        }

        foreach (var toolName in usedToolNames) {

            foreach (var tool in machineCarousel.Tools) {

                if (toolName == tool.Name || tool.AlternativeNames.Contains(toolName)) {
                    toolTable[tool.Position] = tool.Name;
                    break;
                }

            }

        }

        return toolTable;

    }

    private static string GetFileNameFromPartLabels(PartLabels? labels) {

        if (labels is null) return string.Empty;

        if (labels.Fields.TryGetValue("FileName", out string? fileName)) {
            return fileName ?? string.Empty;
        }
        if (labels.Fields.TryGetValue("Filename", out fileName)) {
            return fileName ?? string.Empty;
        }

        return fileName ?? string.Empty;

    }

    private static string GetMachineName(string scheduleName) {

        if (scheduleName.Contains("OMNITECH")) return "OMNITECH";
        if (scheduleName.Contains("ANDI STRATOS")) return "ANDI STRATOS";
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
