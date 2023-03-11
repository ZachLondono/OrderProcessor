using ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.CNC.ReleasePDF.Contracts;
using ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.CNC.ReleasePDF.Services;
using ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.CNC.Domain;
using ApplicationCore.Features.Shared.Domain;
using ApplicationCore.Infrastructure.Bus;
using MoreLinq;
using System.Xml.Linq;
using ApplicationCore.Features.WorkOrders.Contracts;

namespace ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.CNC.ReleasePDF;

internal class GenerateReleaseForSelectedJobs {

    public record Command(Guid OrderId, string Title, string CustomerName, string VendorName, DateTime OrderDate, string ReportFilePath);

    public class Handler {

        private readonly Manufacturing.CreateWorkOrder _createWorkOrder;
        private readonly IReleasePDFWriter _pdfService;

        public Handler(Manufacturing.CreateWorkOrder createWorkOrder, IReleasePDFWriter pdfService) {
            _createWorkOrder = createWorkOrder;
            _pdfService = pdfService;
        }

        public Task<Response<ReleaseGenerationResult>> Handle(Command command) {

            ReleasedJob? releasedJob = CreateReleasedJob(command);

            if (releasedJob is null) {

                return Task.FromResult(Response<ReleaseGenerationResult>.Error(new() {
                    Title = "Could not generate release",
                    Details = "Unable to load data from report"
                }));

            }

            Guid? workOrderId = null; //await GenerateWorkOrder(command, productIds, partClasses);
            releasedJob.WorkOrderId = workOrderId;
            var decortors = _pdfService.GenerateDecorators(releasedJob);

            return Task.FromResult(Response<ReleaseGenerationResult>.Success(new() {
                WorkOrderId = workOrderId,
                Decorators = decortors
            }));

        }

        private static ReleasedJob? CreateReleasedJob(Command command) {
            XDocument xdoc = XDocument.Load(command.ReportFilePath);

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

            var releasedJob = new ReleasedJob() {
                JobName = job.ElementValue("Name"),
                OrderDate = command.OrderDate,
                ReleaseDate = DateTime.Now,
                CustomerName = command.CustomerName,
                VendorName = command.VendorName,
                WorkOrderId = null,
                Releases = patternSchedules.GroupBy(sched => GetMachineName(sched.Name))
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
                                                                                            return new NestedPart() {
                                                                                                Name = part.Name,
                                                                                                Width = Dimension.FromMillimeters(part.Width),
                                                                                                Length = Dimension.FromMillimeters(part.Length),
                                                                                                Center = new() {
                                                                                                    X = nestPart.location.Insert.X + (nestPart.location.IsRotated ? part.Length : part.Width) / 2,
                                                                                                    Y = nestPart.location.Insert.Y + (nestPart.location.IsRotated ? part.Width : part.Length) / 2
                                                                                                },
                                                                                                IsRotated = nestPart.location.IsRotated,
                                                                                                Description = label.Fields.GetValueOrEmpty("Description"),
                                                                                                ProductNumber = label.Fields.GetValueOrEmpty("ProductNumber"),
                                                                                                ImageData = label.Fields.GetValueOrEmpty("Machining Picture"),
                                                                                                HasFace6 = false
                                                                                            };
                                                                                        })
                                                                                        .ToList()
                                                                    };
                                                                })
                                            })
            };

            return releasedJob;
        }

        private async Task<Guid?> GenerateWorkOrder(Command command, HashSet<string> productIds, HashSet<string> partClasses) {

            Guid? workOrderId = null;

            if (productIds.Any()) {

                string workOrderName = partClasses.Distinct()
                                                    .Select(partClass => partClass.ToLower() switch {
                                                        "royal2" => "Cabinets",
                                                        "royal_c" => "Closets",
                                                        "mdfdoor" => "MDF Doors",
                                                        _ => "CNC"
                                                    })
                                                    .Aggregate((l, r) => $"{l},{r}");

                var ids = productIds.Select(str => {

                    if (Guid.TryParse(str, out Guid id)) {
                        return id;
                    }

                    return Guid.Empty;

                })
                //.Where(id => id != Guid.Empty && order.Products.Any(p => p.Id == id)) // TODO: maybe add a delegate 'GetProductIdsInOrder'
                .Distinct()
                .ToList();

                if (ids.Any()) {

                    workOrderId = await _createWorkOrder(command.OrderId, workOrderName, ids);

                }

            }

            return workOrderId;
        }

    }

    public class ReleaseGenerationResult {

        public Guid? WorkOrderId { get; set; }
        public IEnumerable<IDocumentDecorator> Decorators { get; set; } = Enumerable.Empty<IDocumentDecorator>();

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

        return patternName.Substring(0, idx);

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
