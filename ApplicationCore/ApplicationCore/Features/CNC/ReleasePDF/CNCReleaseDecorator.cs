using QuestPDF.Infrastructure;
using ApplicationCore.Features.CNC.Tools.Contracts;
using ApplicationCore.Features.CNC.ReleasePDF.Services;
using ApplicationCore.Features.CNC.ReleasePDF.WSXML;
using ApplicationCore.Features.CNC.Contracts;
using ApplicationCore.Features.CNC.Domain;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.CNC.ReleasePDF;

internal class CNCReleaseDecorator : ICNCReleaseDecorator {

    private ReleasedJob? _jobData = null;

    private readonly ReleasePDFDecoratorFactory _pdfService;
    private readonly CNCToolBox.GetToolCarousels _getToolCarousels;

    public CNCReleaseDecorator(ReleasePDFDecoratorFactory pdfService, CNCToolBox.GetToolCarousels getToolCarousels) {
        _pdfService = pdfService;
        _getToolCarousels = getToolCarousels;
    }

    public async Task<ReleasedJob?> LoadDataFromFile(string reportFilePath, DateTime orderDate, string customerName, string vendorName) {

        var toolCarousels = await _getToolCarousels();
        var report = WSXMLParser.ParseWSXMLReport(reportFilePath);
        if (report is null) {
            return null;
        }
        _jobData = MapDataToReleasedJob(report, orderDate, customerName, vendorName, toolCarousels);
        return _jobData;

    }

    public void Decorate(IDocumentContainer container) {

        if (_jobData is null) {
            return;
        }

        var decorators = _pdfService.GenerateDecorators(_jobData);

        foreach (var decorator in decorators) {

            decorator.Decorate(container);

        }

        return;

    }

    private static ReleasedJob MapDataToReleasedJob(WSXMLReport report, DateTime orderDate, string customerName, string vendorName, IEnumerable<ToolCarousel> toolCarousels) {

        var allToolNames = report.OperationGroups.Where(g => g.PartId is not null)
                                        .SelectMany(g => g.ToolName)
                                        .Distinct();

        List<MachineRelease> releases = report.PatternSchedules
                                                .GroupBy(sched => GetMachineName(sched.Name))
                                                .Select(group => new MachineRelease() {
                                                    MachineName = group.Key,
                                                    ToolTable = CreateMachineToolTable(group.Key, toolCarousels, allToolNames),
                                                    MachineTableOrientation = GetTableOrientationFromMachineName(group.Key),
                                                    Programs = group.SelectMany(group => group.Patterns)
                                                                    .Select(pattern => {
                                                                        var material = report.Materials[pattern.MaterialId];
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
                                                                                                var part = report.Parts[nestPart.partId];
                                                                                                var label = report.PartLabels[part.LabelId];
                                                                                                if (!Guid.TryParse(label.Fields.GetValueOrEmpty("Comment2"), out Guid productId)) {
                                                                                                    productId = Guid.Empty;
                                                                                                }
                                                                                                return new NestedPart() {
                                                                                                    Name = part.Name,
                                                                                                    FileName = part.Variables["Face5Filename"],
                                                                                                    Width = Dimension.FromMillimeters(part.Width),
                                                                                                    Length = Dimension.FromMillimeters(part.Length),
                                                                                                    Center = new() {
                                                                                                        X = nestPart.location.Insert.X + (nestPart.location.IsRotated ? part.Length : part.Width) / 2,
                                                                                                        Y = nestPart.location.Insert.Y + (nestPart.location.IsRotated ? part.Width : part.Length) / 2
                                                                                                    },
                                                                                                    IsRotated = nestPart.location.IsRotated,
                                                                                                    Description = part.Description,
                                                                                                    ProductNumber = label.Fields.GetValueOrEmpty("Cabinet Number"),
                                                                                                    ProductId = productId,
                                                                                                    PartId = nestPart.partId,
                                                                                                    ImageData = label.Fields.GetValueOrEmpty("Machining Picture"),
                                                                                                    HasFace6 = part.Variables.ContainsKey("Face6FileName"),
                                                                                                    Face6FileName = part.Variables.ContainsKey("Face6FileName") ? part.Variables["Face6FileName"] : null,
                                                                                                    HasBackSideProgram = label.Fields.GetValueOrEmpty("HasBackSideProgram") == "Y"
                                                                                                };
                                                                                            })
                                                                                            .ToList()
                                                                        };
                                                                    })
                                                }).ToList();

        var singleParts = report.Items
                                .Where(item => item.Note == "Single Program Code")
                                .GroupBy(item => GetMachineName(item.Name))
                                .ToDictionary(
                                    group => group.Key,
                                    group => group.SelectMany(item => item.PartIds.Select(id => report.Parts[id]))
                                                    .Select(part => {
                                                        var label = report.PartLabels[part.LabelId];
                                                        return new SinglePartProgram() {
                                                            Name = part.Name,
                                                            FileName = part.Variables["Face5Filename"],
                                                            Width = Dimension.FromMillimeters(part.Width),
                                                            Length = Dimension.FromMillimeters(part.Length),
                                                            Description = part.Description,
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
            CustomerName = customerName,
            VendorName = vendorName,
            Releases = releases,
            TimeStamp = report.TimeStamp
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

                if (string.Equals(toolName, tool.Name, StringComparison.OrdinalIgnoreCase) || tool.AlternativeNames.Contains(toolName, StringComparer.OrdinalIgnoreCase)) {
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