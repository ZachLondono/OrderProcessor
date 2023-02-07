using ApplicationCore.Features.Companies.Queries;
using ApplicationCore.Features.ExcelTemplates.Domain;
using ApplicationCore.Features.Orders.Shared.Domain;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Orders.Release.Handlers.CutListing.Models;
using ApplicationCore.Infrastructure;
using Microsoft.Extensions.Logging;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Shared;

namespace ApplicationCore.Features.Orders.Release.Handlers.CutListing;

public class CutListHandler : DomainListener<TriggerOrderReleaseNotification> {

    private readonly ILogger<CutListHandler> _logger;
    private readonly IBus _bus;
    private readonly IUIBus _uibus;
    private readonly ConstructionValues _construction;
    private readonly ProductBuilderFactory _productBuilderFactory;
    private readonly IFileReader _fileReader;

    public CutListHandler(ILogger<CutListHandler> logger, IBus bus, IUIBus uibus, ConstructionValues construction, ProductBuilderFactory productBuilderFactory, IFileReader fileReader) {
        _logger = logger;
        _bus = bus;
        _uibus = uibus;
        _construction = construction;
        _productBuilderFactory = productBuilderFactory;
        _fileReader = fileReader;
    }

    public override async Task Handle(TriggerOrderReleaseNotification notification) {

        if (!notification.ReleaseProfile.GenerateCutList) {
            _uibus.Publish(new OrderReleaseInfoNotification("Not creating Cut Lists, because option was disabled"));
            return;
        }

        var order = notification.Order;

        var dovetailBoxes = order.Products
                                .Where(p => p is IDrawerBoxContainer)
                                .Cast<IDrawerBoxContainer>()
                                .SelectMany(c => {
                                    try { 
                                        return c.GetDrawerBoxes(_productBuilderFactory.CreateDovetailDrawerBoxBuilder);
                                    } catch (Exception ex) {
                                        _uibus.Publish(new OrderReleaseErrorNotification($"Error getting drawer boxes from product '{ex.Message}'"));
                                        return Enumerable.Empty<DovetailDrawerBox>();
                                    }
                                })
                                .ToList();

        if (!dovetailBoxes.Any()) {
            _uibus.Publish(new OrderReleaseInfoNotification("Not creating Cut Lists, because there where no drawer boxes found"));
            return;
        }

        var customerName = order.Customer.Name;
        var vendorName = await GetCompanyName(order.VendorId);

        CutList cutList = CreateStdCutList(order, dovetailBoxes, customerName, vendorName, _construction);
        CutList optimizedCutList = CreateOptimizedCutList(order, dovetailBoxes, customerName, vendorName, _construction);
        CutList bottomCutList = CreateBottomCutList(order, dovetailBoxes, customerName, vendorName, _construction);

        string outputDir = notification.ReleaseProfile.CutListOutputDirectory;
        bool doPrint = notification.ReleaseProfile.PrintCutList;
        string job = $"{order.Number} - {order.Name}";

        GenerateCutList(cutList, job, outputDir);
        GenerateCutList(optimizedCutList, job, outputDir);
        GenerateCutList(bottomCutList, job, outputDir);

    }

    private void GenerateCutList(CutList cutlist, string job, string outputDir) {

        var service = new CutListService();
        try {
            var wb = service.GenerateCutList(cutlist.Header, cutlist.Parts);
            string outputFile = GetAvailableFileName(outputDir, $"{job} {cutlist.Name} PACKING LIST");
            wb.SaveAs(outputFile);
            _uibus.Publish(new OrderReleaseFileCreatedNotification("Cut List created", outputFile));
        } catch (Exception ex) {
            _uibus.Publish(new OrderReleaseErrorNotification($"Error creating Cut List {ex.Message}"));
        }

    }

    private string GetAvailableFileName(string direcotry, string filename) {

        int index = 1;

        string filepath = Path.Combine(direcotry, $"{filename}.xlsx");

        while (_fileReader.DoesFileExist(filepath)) {

            filepath = Path.Combine(direcotry, $"{filename} ({index++}).xlsx");

        }

        return filepath;

    }

    private async Task<string> GetCompanyName(Guid companyId) {
        var response = await _bus.Send(new GetCompanyById.Query(companyId));

        string name = "";
        response.Match(
            company => {
                name = company?.Name ?? "Unknown";
            },
            error => {
                name = "Unknown";
            }
        );

        return name;
    }

    private static CutList CreateStdCutList(Order order, IEnumerable<DovetailDrawerBox> dovetailBoxes, string customerName, string vendorName, ConstructionValues construction) {
        
        int lineNum = 1;
        var cutlistItems = dovetailBoxes
                                .SelectMany(b => {

                                    var items = new List<PartRow>();
                                    foreach (var part in b.GetParts(construction)) {

                                        items.Add(new() {
                                            CabNumbers = part.ProductNumber.ToString(),
                                            Line = lineNum++,
                                            Name = part.Type.ToString(),
                                            Qty = part.Qty,
                                            Width = part.Width.AsMillimeters(),
                                            Length = part.Length.AsMillimeters(),
                                            Comment = part.Comment,
                                            Material = part.Material,
                                            PartSize = $"{part.Width.AsInchFraction()}\"W x {part.Length.AsInchFraction()}\"L",
                                        });
                                    }

                                    return items;
                                })
                                .ToList();

        return CreateCutList(order, dovetailBoxes, customerName, vendorName, cutlistItems, "CUTLIST");

    }

    private static CutList CreateOptimizedCutList(Order order, IEnumerable<DovetailDrawerBox> dovetailBoxes, string customerName, string vendorName, ConstructionValues construction) {
        
        int lineNum = 1;
        var cutlistItems = dovetailBoxes
                                .SelectMany(b => b.GetParts(construction).Where(p => p.Type != DrawerBoxPartType.Bottom))
                                .GroupBy(p => (p.Material, p.Width, p.Length)) // TODO: if there are multiple scoop fronts they should be grouped together
                                .Select(g => {
                                    var qty = g.Sum(b => b.Qty);
                                    string cabNumbers = string.Join(',', g.Select(p => p.ProductNumber).Distinct());
                                    return new PartRow() {
                                        CabNumbers = cabNumbers,
                                        Line = lineNum++,
                                        Name = DrawerBoxPartType.Unknown.ToString(),
                                        Qty = qty,
                                        Width = g.Key.Width.AsMillimeters(),
                                        Length = g.Key.Length.AsMillimeters(),
                                        Comment = "",
                                        Material = g.Key.Material,
                                        PartSize = $"{g.Key.Width.AsInchFraction()}\"W x {g.Key.Length.AsInchFraction()}\"L",
                                    };
                                })
                                .ToList();

        return CreateCutList(order, dovetailBoxes, customerName, vendorName, cutlistItems, "OPTIMIZED");

    }

    private static CutList CreateBottomCutList(Order order, IEnumerable<DovetailDrawerBox> dovetailBoxes, string customerName, string vendorName, ConstructionValues construction) {
        
        int lineNum = 1;
        var cutlistItems = dovetailBoxes
                                .SelectMany(b => {
                                    var items = new List<PartRow>();
                                    foreach (var part in b.GetParts(construction).Where(p => p.Type == DrawerBoxPartType.Bottom)) {
                                        items.Add(new() {
                                            CabNumbers = part.ProductNumber.ToString(),
                                            Line = lineNum++,
                                            Name = part.Type.ToString(),
                                            Qty = part.Qty,
                                            Width = Math.Round(part.Width.AsMillimeters(), 2),
                                            Length = Math.Round(part.Length.AsMillimeters(), 2),
                                            Comment = part.Comment,
                                            Material = part.Material,
                                            PartSize = $"{part.Width.AsInchFraction()}\"W x {part.Length.AsInchFraction()}\"L"
                                        });
                                    }

                                    return items;
                                })
                                .ToList();

        return CreateCutList(order, dovetailBoxes, customerName, vendorName, cutlistItems, "BOTTOMS");

    }

    private static CutList CreateCutList(Order order, IEnumerable<DovetailDrawerBox> dovetailBoxes, string customerName, string vendorName, List<PartRow> parts, string name) {

        // TODO: if a drawerbox has a different option then the most common option than it should be shown in a part comment

        var clips = dovetailBoxes.Select(b => b.Options.Clips)
                        .GroupBy(c => c)
                        .OrderByDescending(g => g.Count())
                        .First()
                        .First();

        var notch = dovetailBoxes.Select(b => b.Options.Notches)
                        .GroupBy(n => n)
                        .OrderByDescending(g => g.Count())
                        .First()
                        .First();

        var postFin = dovetailBoxes.Select(b => b.Options.PostFinish)
                        .GroupBy(c => c)
                        .OrderByDescending(g => g.Count())
                        .First()
                        .First();

        var mountingHoles = dovetailBoxes.Select(b => b.Options.FaceMountingHoles)
                                .GroupBy(c => c)
                                .OrderByDescending(g => g.Count())
                                .First()
                                .First();

        var assembled = dovetailBoxes.Select(b => b.Options.Assembled)
                                .GroupBy(c => c)
                                .OrderByDescending(g => g.Count())
                                .First()
                                .First();

        var cutlist = new CutList() {
            Name = name,
            Header = new Header() {
                OrderNumber = order.Number,
                JobName = order.Name,
                BoxCount = order.Products.Sum(b => b.Qty),
                Clips = clips,
                MountingHoles = mountingHoles,
                Notches = notch,
                Finish = postFin,
                CustomerName = customerName,
                VendorName = vendorName,
                Note = order.CustomerComment,
                OrderDate = order.OrderDate,
                Assembly = assembled,
            },
            Parts = parts
        };
        return cutlist;
    }

}