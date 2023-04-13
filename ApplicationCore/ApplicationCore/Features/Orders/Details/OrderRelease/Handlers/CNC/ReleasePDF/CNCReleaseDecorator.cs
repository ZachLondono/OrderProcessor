using ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.CNC.ReleasePDF.Contracts;
using ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.CNC.ReleasePDF.Services;
using ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.CNC.Domain;
using ApplicationCore.Features.Shared.Domain;
using System.Xml.Linq;
using QuestPDF.Infrastructure;
using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using ApplicationCore.Features.Companies.Contracts;

namespace ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.CNC.ReleasePDF;

internal class CNCReleaseDecorator : ICNCReleaseDecorator {

    private readonly IReleasePDFWriter _pdfService;
    private readonly CompanyDirectory.GetCustomerByIdAsync _getCustomerByIdAsync;
    private readonly CompanyDirectory.GetVendorByIdAsync _getVendorByIdAsync;

    public string ReportFilePath { get; set; } = string.Empty;

    public CNCReleaseDecorator(IReleasePDFWriter pdfService, CompanyDirectory.GetCustomerByIdAsync getCustomerByIdAsync, CompanyDirectory.GetVendorByIdAsync getVendorByIdAsync) {
        _pdfService = pdfService;
        _getCustomerByIdAsync = getCustomerByIdAsync;
        _getVendorByIdAsync = getVendorByIdAsync;
    }

    public async Task Decorate(Order order, IDocumentContainer container) {

        var vendor = await _getVendorByIdAsync(order.VendorId);
        var customer = await _getCustomerByIdAsync(order.CustomerId);

        ReleasedJob? releasedJob = CreateReleasedJob(ReportFilePath, order.OrderDate, customer?.Name ?? "", vendor?.Name ?? "");

        if (releasedJob is null) {

            return;

        }

        var decorators = _pdfService.GenerateDecorators(releasedJob);

        foreach (var decorator in decorators) {

            await decorator.Decorate(order, container);

        }

        return;

    }

    private static ReleasedJob? CreateReleasedJob(string reportFilePath, DateTime orderDate, string customerName, string vendorName) {
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

        var patternScheduleItems = job.Element("Manufacturing").Elements("PatternSchedule");

        var patternSchedules = patternScheduleItems.Select(PatternSchedule.FromXElement);

        var materials = job.Elements("Material").Select(MaterialRecord.FromXElement).ToDictionary(m => m.Id);

        var labels = job.Element("Manufacturing").Elements("Label").Select(PartLabels.FromXElement).ToDictionary(l => l.Id);

        List<MachineRelease> releases = patternSchedules
                                        .GroupBy(sched => GetMachineName(sched.Name))
                                        .Select(group => new MachineRelease() {
                                            MachineName = group.Key,
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
            JobName = job.ElementValue("Name"),
            OrderDate = orderDate,
            ReleaseDate = DateTime.Now,
            CustomerName = customerName,
            VendorName = vendorName,
            WorkOrderId = null,
            Releases = releases
        };

        return releasedJob;
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

    record Nest(string Id, string Name, IEnumerable<Part> Parts);

    record Part(string Id, string LabelId, string Name, double Width, double Length, IEnumerable<string> PatternScheduleIds) {
        public static Part FromXElemnt(XElement element) => new(element.AttributeValue("ID"), element.AttributeValue("LabelID"), element.ElementValue("Name"), element.ElementDouble("FinishedLength"), element.ElementDouble("FinishedWidth"), element.AttributeValue("PatSchID").Split(' '));
    }

    record PatternSchedule(string Id, string Name, IEnumerable<Pattern> Patterns) {
        public static PatternSchedule FromXElement(XElement element) => new(element.AttributeValue("ID"), element.ElementValue("Name"), element.Elements("Pattern").Select(Pattern.FromXElement));
    }

    record Pattern(string Id, string Name, string MaterialId, IEnumerable<PatternPart> Parts) {
        public static Pattern FromXElement(XElement element) => new(element.AttributeValue("ID"), element.ElementValue("Name"), element.AttributeValue("MatID"), PatternPart.FromXEment(element));
    }

    record PatternPart(string PartId, IEnumerable<PatternPartLocation> Locations) {
        public static IEnumerable<PatternPart> FromXEment(XElement patternElement) {

            return patternElement.Elements("NestPart")
                            .GroupBy(nestPart => nestPart.AttributeValue("PartID"))
                            .Select(group =>
                                new PatternPart(group.Key,
                                                group.Select(nestPart => {
                                                    var insert = nestPart.Element("Insert");
                                                    bool isRotated = nestPart.ElementValue("Rotation") == "90";

                                                    return new PatternPartLocation(new Point() {
                                                        X = insert.AttributeDouble("x"),
                                                        Y = insert.AttributeDouble("y")
                                                    }, isRotated);
                                                }))
                            );

        }
    }

    record PatternPartLocation(Point Insert, bool IsRotated);

    record MaterialRecord(string Id, string Name, double XDim, double YDim, double ZDim) {
        public static MaterialRecord FromXElement(XElement element) => new(element.AttributeValue("ID"), element.ElementValue("Name"), element.ElementDouble("XDim"), element.ElementDouble("YDim"), element.ElementDouble("ZDim"));
    }

    public record PartLabels(string Id, Dictionary<string, string> Fields) {

        public static PartLabels FromXElement(XElement element) {

            Dictionary<string, string> fields = new();

            string name = "";
            foreach (var subelement in element.Descendants()) {

                switch (subelement.Name.ToString()) {

                    case "Name":
                        name = subelement.Value;
                        break;

                    case "Value":
                        fields.TryAdd(name, subelement.Value);
                        name = "";
                        break;

                }

            }

            return new(element.AttributeValue("ID"), fields);

        }

    }

}