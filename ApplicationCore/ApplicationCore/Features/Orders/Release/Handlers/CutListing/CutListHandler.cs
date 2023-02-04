using ApplicationCore.Features.Companies.Queries;
using ApplicationCore.Features.ExcelTemplates.Contracts;
using ApplicationCore.Features.ExcelTemplates.Domain;
using ApplicationCore.Features.Orders.Shared.Domain;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Orders.Release.Handlers.CutListing.Models;
using ApplicationCore.Infrastructure;
using Microsoft.Extensions.Logging;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using ApplicationCore.Features.Orders.Shared.Domain.Builders;

namespace ApplicationCore.Features.Orders.Release.Handlers.CutListing;

public class CutListHandler : DomainListener<TriggerOrderReleaseNotification> {

    private readonly ILogger<CutListHandler> _logger;
    private readonly IBus _bus;
    private readonly IUIBus _uibus;
    private readonly ConstructionValues _construction;
    private readonly ProductBuilderFactory _productBuilderFactory;

    public CutListHandler(ILogger<CutListHandler> logger, IBus bus, IUIBus uibus, ConstructionValues construction, ProductBuilderFactory productBuilderFactory) {
        _logger = logger;
        _bus = bus;
        _uibus = uibus;
        _construction = construction;
        _productBuilderFactory = productBuilderFactory;
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

        var config = new ClosedXMLTemplateConfiguration() { TemplateFilePath = notification.ReleaseProfile.CutListTemplatePath };
        string outputDir = notification.ReleaseProfile.CutListOutputDirectory;
        bool doPrint = notification.ReleaseProfile.PrintCutList;
        string prefix = $"{order.Number} - {order.Name}";

        var responseStd = await _bus.Send(new FillTemplateRequest(cutList, outputDir, $"{prefix} CUTLIST", doPrint, config));
        responseStd.Match(
            r => {
                _logger.LogInformation("Drawer Box Cut List Created : {FilePath}", r.FilePath);
                _uibus.Publish(new OrderReleaseSuccessNotification($"Cut List created {r.FilePath}"));
            },
            error => {
                _logger.LogError("Error creating Cut List : {Error}", error);
                _uibus.Publish(new OrderReleaseErrorNotification($"Error creating Cut List {error.Details}"));
            }
        );

        var responseOptimized = await _bus.Send(new FillTemplateRequest(optimizedCutList, outputDir, $"{prefix} OPTIMIZED CUTLIST", doPrint, config));
        responseOptimized.Match(
            r => {
                _logger.LogInformation("Drawer Box Cut List Created : {FilePath}", r.FilePath);
                _uibus.Publish(new OrderReleaseSuccessNotification($"Cut List created {r.FilePath}"));
            },
            error => {
                _logger.LogError("Error creating Cut List : {Error}", error);
                _uibus.Publish(new OrderReleaseErrorNotification($"Error creating Cut List {error.Details}"));
            }
        );

        var responseBottom = await _bus.Send(new FillTemplateRequest(bottomCutList, outputDir, $"{prefix} BOTTOM CUTLIST", doPrint, config));
        responseBottom.Match(
            r => {
                _logger.LogInformation("Drawer Box Cut List Created : {FilePath}", r.FilePath);
                _uibus.Publish(new OrderReleaseSuccessNotification($"Cut List created {r.FilePath}"));
            },
            error => {
                _logger.LogError("Error creating Cut List : {Error}", error);
                _uibus.Publish(new OrderReleaseErrorNotification($"Error creating Cut List {error.Details}"));
            }
        );

        // TODO: if necessary, format the generated cutlist note field when the note is really long by writing the note in the left cell, autofit the row then merge the cells


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
        int groupNum = 0;
        int lineNum = 1;
        var cutlistItems = dovetailBoxes
                                .SelectMany(b => {
                                    groupNum++;

                                    var items = new List<Item>();
                                    foreach (var part in b.GetParts(construction)) {

                                        items.Add(new() {
                                            GroupNumber = groupNum,
                                            CabNumber = part.ProductNumber.ToString(),
                                            LineNumber = lineNum++,
                                            PartName = part.Type.ToString(),
                                            Qty = part.Qty,
                                            Width = part.Width.AsMillimeters(),
                                            Length = part.Length.AsMillimeters(),
                                            Comment = part.Comment,
                                            Material = part.Material,
                                            Size = $"{part.Width.AsInchFraction()}\"W x {part.Length.AsInchFraction()}\"L",
                                        });
                                    }

                                    return items;
                                })
                                .ToList();

        return CreateCutList(order, dovetailBoxes, customerName, vendorName, cutlistItems);

    }

    private static CutList CreateOptimizedCutList(Order order, IEnumerable<DovetailDrawerBox> dovetailBoxes, string customerName, string vendorName, ConstructionValues construction) {
        int groupNum = 0;
        int lineNum = 1;
        var cutlistItems = dovetailBoxes
                                .SelectMany(b => b.GetParts(construction).Where(p => p.Type != DrawerBoxPartType.Bottom))
                                .GroupBy(p => (p.Material, p.Width, p.Length)) // TODO: if there are multiple scoop fronts they should be grouped together
                                .Select(g => {
                                    var qty = g.Sum(b => b.Qty);
                                    string cabNumbers = string.Join(',', g.Select(p => p.ProductNumber).Distinct());
                                    groupNum++;
                                    return new Item() {
                                        GroupNumber = groupNum,
                                        CabNumber = cabNumbers,
                                        LineNumber = lineNum++,
                                        PartName = DrawerBoxPartType.Unknown.ToString(),
                                        Qty = qty,
                                        Width = g.Key.Width.AsMillimeters(),
                                        Length = g.Key.Length.AsMillimeters(),
                                        Comment = "",
                                        Material = g.Key.Material,
                                        Size = $"{g.Key.Width.AsInchFraction()}\"W x {g.Key.Length.AsInchFraction()}\"L",
                                    };
                                })
                                .ToList();

        return CreateCutList(order, dovetailBoxes, customerName, vendorName, cutlistItems);

    }

    private static CutList CreateBottomCutList(Order order, IEnumerable<DovetailDrawerBox> dovetailBoxes, string customerName, string vendorName, ConstructionValues construction) {
        int groupNum = 0;
        int lineNum = 1;

        var cutlistItems = dovetailBoxes
                                .SelectMany(b => {
                                    groupNum++;

                                    var items = new List<Item>();
                                    foreach (var part in b.GetParts(construction).Where(p => p.Type == DrawerBoxPartType.Bottom)) {
                                        items.Add(new() {
                                            GroupNumber = groupNum,
                                            CabNumber = part.ProductNumber.ToString(),
                                            LineNumber = lineNum++,
                                            PartName = part.Type.ToString(),
                                            Qty = part.Qty,
                                            Width = Math.Round(part.Width.AsMillimeters(), 2),
                                            Length = Math.Round(part.Length.AsMillimeters(), 2),
                                            Comment = part.Comment,
                                            Material = part.Material,
                                            Size = $"{part.Width.AsInchFraction()}\"W x {part.Length.AsInchFraction()}\"L",
                                        });
                                    }

                                    return items;
                                })
                                .ToList();

        return CreateCutList(order, dovetailBoxes, customerName, vendorName, cutlistItems);

    }

    private static CutList CreateCutList(Order order, IEnumerable<DovetailDrawerBox> dovetailBoxes, string customerName, string vendorName, List<Item> cutlistItems) {

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

        var cutlist = new CutList() {
            Header = new Header() {
                OrderNumber = order.Number,
                OrderName = order.Name,
                BoxCount = order.Products.Sum(b => b.Qty),
                Clips = $"Clips: {clips}",
                MountingHoles = $"Mounting Holes : {(mountingHoles ? "Yes" : "No")}", // Get the most common mounting holes option
                Notch = $"Notch: {notch}",
                PostFinish = $"Post Finish: {(postFin ? "Yes" : "No")}",
                CustomerName = customerName,
                VendorName = vendorName,
                Note = order.CustomerComment,
                OrderDate = order.OrderDate.ToShortDateString(),
            },
            Items = cutlistItems
        };
        return cutlist;
    }


}