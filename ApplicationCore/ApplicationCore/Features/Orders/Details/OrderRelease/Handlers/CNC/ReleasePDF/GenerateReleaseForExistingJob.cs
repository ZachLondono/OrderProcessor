using ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.CNC.LabelDB.Contracts;
using ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.CNC.LabelDB.Services;
using ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.CNC.ReleasePDF.Contracts;
using ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.CNC.ReleasePDF.Services;
using ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.CNC.Domain;
using ApplicationCore.Features.Shared.Contracts;
using ApplicationCore.Features.Shared.Domain;
using ApplicationCore.Infrastructure.Bus;
using MoreLinq;
using ApplicationCore.Features.Orders.Details.Shared;

namespace ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.CNC.ReleasePDF;

internal class GenerateReleaseForSelectedJobs {

    public record Command(Guid OrderId, string Title, string CustomerName, string VendorName, DateTime OrderDate, string LabelFilePath, IEnumerable<AvailableJob> SelectedJobs);

    public class Handler {

        private readonly IExistingJobProvider _existingJobProvider;
        private readonly Manufacturing.CreateWorkOrder _createWorkOrder;
        private readonly IReleasePDFWriter _pdfService;

        public Handler(IExistingJobProvider existingJobProvider, Manufacturing.CreateWorkOrder createWorkOrder, IReleasePDFWriter pdfService) {
            _existingJobProvider = existingJobProvider;
            _createWorkOrder = createWorkOrder;
            _pdfService = pdfService;
        }

        public async Task<Response<ReleaseGenerationResult>> Handle(Command command) {

            var jobsByMachine = command.SelectedJobs.GroupBy(job => job.MachineName).ToList();

            HashSet<string> productIds = new();
            HashSet<string> partClasses = new();
            List<ReleasedJob> jobsToRelease = new();

            foreach (var selectedJobsGroup in jobsByMachine) {

                List<ExistingJob> existingJobs = new();

                foreach (var selectedJob in selectedJobsGroup) {

                    ExistingJob? existingJob = await _existingJobProvider.LoadExistingJobAsync(command.LabelFilePath, selectedJob.Name);

                    if (existingJob is null) {
                        // TODO: log warning
                        continue;
                    }

                    existingJobs.Add(existingJob);

                }


                existingJobs.SelectMany(j => j.Parts)
                            .Select(part => part.ProductId)
                            .Except(productIds)
                            .Distinct()
                            .ForEach(id => productIds.Add(id));

                existingJobs.SelectMany(j => j.Parts)
                            .Select(part => part.PartClass)
                            .Except(partClasses)
                            .Distinct()
                            .ForEach(cl => partClasses.Add(cl));


                ReleasedJob releasedJob = CreateReleasedJob(command, existingJobs, selectedJobsGroup.Key);

                jobsToRelease.Add(releasedJob);

            }

            Guid? workOrderId = await GenerateWorkOrder(command, productIds, partClasses);

            List<IDocumentDecorator> documentDecorators = new();
            foreach (var job in jobsToRelease) {

                job.WorkOrderId = workOrderId;

                var decortors = _pdfService.GenerateDecorators(job);

                documentDecorators.AddRange(decortors);

            }

            return Response<ReleaseGenerationResult>.Success(new() {
                WorkOrderId = workOrderId,
                Decorators = documentDecorators
            });

        }

        private static ReleasedJob CreateReleasedJob(Command command, List<ExistingJob> existingJobs, string machineName) {
            var nestedPartByProgram = existingJobs.SelectMany(existingJob => existingJob.Parts
                                                                                        .GroupBy(part => part.PatternNumber)
                                                                                        .ToDictionary(group => existingJob.Patterns.Skip(group.Key - 1).FirstOrDefault()?.Name ?? "", group => group.ToList())
                                                    )
                                                    .GroupBy(kvp => kvp.Key, kvp => kvp.Value)
                                                    .ToDictionary(g => g.Key, g => g.Last());


            var programs = existingJobs.SelectMany(existingJob => existingJob.Patterns.Select(pattern => {
                var parts = GetParts(nestedPartByProgram, pattern.Name);
                return new ReleasedProgram() {
                    Name = pattern.Name,
                    ImagePath = $"Y:\\CADCode\\pix\\{pattern.ImagePath}.wmf",
                    HasFace6 = parts.Any(p => p.HasFace6),
                    Material = new ProgramMaterial() {
                        Name = pattern.MaterialName,
                        Width = pattern.MaterialWidth,
                        Length = pattern.MaterialLength,
                        Thickness = pattern.MaterialThickness,
                        IsGrained = existingJob.Inventory
                                            .Where(inv => inv.Name == pattern.MaterialName)
                                            .Select(inv => inv.Grained)
                                            .FirstOrDefault()?.Equals("1") ?? false,
                        Yield = 0 // TODO: calculate yield
                    },
                    Parts = parts
                };
            }));

            ReleasedJob releasedJob = new() {
                JobName = command.Title,
                CustomerName = command.CustomerName,
                VendorName = command.VendorName,
                OrderDate = command.OrderDate,
                ReleaseDate = DateTime.Now,
                Releases = new List<MachineRelease>() {
                        new() {
                            MachineName = machineName,
                            MachineTableOrientation = (machineName == "Omnitech" ? TableOrientation.Rotated : TableOrientation.Standard),
                            Programs = programs
                        }
                    }
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

        private static List<NestedPart> GetParts(IDictionary<string, List<ManufacturedPart>> nestPartsByPatternName, string patternName) {

            if (nestPartsByPatternName.TryGetValue(patternName, out var parts)) {

                return parts.Select(part => new NestedPart() {
                    Name = part.Name,
                    Width = Dimension.FromMillimeters(part.Width),
                    Length = Dimension.FromMillimeters(part.Length),
                    Description = part.Description,
                    Center = new Point(part.InsertX + part.Length / 2, part.InsertY + part.Width / 2),
                    ProductNumber = part.ProductNumber,
                    HasFace6 = part.HasFace6 == 0
                }).ToList();

            }

            return new();

        }

    }

    public class ReleaseGenerationResult {

        public Guid? WorkOrderId { get; set; }
        public IEnumerable<IDocumentDecorator> Decorators { get; set; } = Enumerable.Empty<IDocumentDecorator>();

    }

}
