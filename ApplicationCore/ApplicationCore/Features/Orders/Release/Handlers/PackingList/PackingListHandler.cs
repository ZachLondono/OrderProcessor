using ApplicationCore.Features.Companies.Queries;
using ApplicationCore.Features.Orders.Release.Handlers.PackingList.Models;
using ApplicationCore.Infrastructure;
using Microsoft.Extensions.Logging;
using Company = ApplicationCore.Features.Companies.Domain.Company;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.Entities;

namespace ApplicationCore.Features.Orders.Release.Handlers.PackingList;

internal class PackingListHandler : DomainListener<TriggerOrderReleaseNotification> {

    private readonly ILogger<PackingListHandler> _logger;
    private readonly IBus _bus;
    private readonly IUIBus _uibus;

    public PackingListHandler(ILogger<PackingListHandler> logger, IBus bus, IUIBus uibus) {
        _bus = bus;
        _uibus = uibus;
        _logger = logger;
    }

    public override async Task Handle(TriggerOrderReleaseNotification notification) {

        if (!notification.ReleaseProfile.GeneratePackingList) {
            _uibus.Publish(new OrderReleaseInfoNotification("Not creating packing list, because option was disabled"));
            return;
        }

        var order = notification.Order;

        if (!order.Products.Any()) {
            _uibus.Publish(new OrderReleaseInfoNotification("Not creating packing list, because there where no products found"));
            return;
        }

        Models.PackingList packinglist = await CreatePackingList(order);

        var outputDir = notification.ReleaseProfile.PackingListOutputDirectory;
        var doPrint = notification.ReleaseProfile.PrintPackingList;

        var service = new PackingListService();
        try {
            var wb = service.GeneratePackingList(packinglist);
            // TODO: add suffix to file name if it already exists
            string outputFile = Path.Combine(outputDir, $"{order.Number} - {order.Name} PACKING LIST.xlsx");
            wb.SaveAs(outputFile);
            _uibus.Publish(new OrderReleaseFileCreatedNotification("Packing List created", outputFile));
        } catch (Exception ex) {
            _uibus.Publish(new OrderReleaseErrorNotification($"Error creating Packing List {ex.Message}"));
        }

    }

    private async Task<Models.PackingList> CreatePackingList(Order order) {

        bool hasError = false;

        Company? vendor = null;
        var vendorQuery = await GetCompany(order.VendorId);
        vendorQuery.Match(
            (company) => {
                vendor = company;
            },
            (error) => {
                hasError = true;
                _logger.LogError("Error loading vendor {Error}", error);
            }
        );

        if (hasError || vendor is null) {
            _uibus.Publish(new OrderReleaseErrorNotification("Could not load customer or vendor information for order"));
        }

        var drawerBoxes = order.Products
                                .Where(p => p is DovetailDrawerBoxProduct)
                                .Cast<DovetailDrawerBoxProduct>()
                                .Select(b => new DrawerBoxItem() {
                                    Line = b.ProductNumber,
                                    Qty = b.Qty,
                                    Description = "Dovetail Drawer Box",
                                    Height = b.Height.AsInchFraction().ToString(),
                                    Width = b.Width.AsInchFraction().ToString(),
                                    Depth = b.Depth.AsInchFraction().ToString()
                                }).ToList();

        var cabinets = order.Products
                        .Where(p => p is Cabinet)
                        .Cast<Cabinet>()
                        .Select(b => new CabinetItem() {
                            Line = b.ProductNumber,
                            Qty = b.Qty,
                            Description = "Cabinet",
                            Height = b.Height.AsInchFraction().ToString(),
                            Width = b.Width.AsInchFraction().ToString(),
                            Depth = b.Depth.AsInchFraction().ToString()
                        }).ToList();

        var custLine2Str = string.IsNullOrWhiteSpace(order.Shipping.Address.Country + order.Shipping.Address.State + order.Shipping.Address.Zip) ? "" : $"{order.Shipping.Address.City}, {order.Shipping.Address.State} {order.Shipping.Address.Zip}";
        var vendLine2Str = string.IsNullOrWhiteSpace(vendor?.Address.Country ?? "" + vendor?.Address.State ?? "" + vendor?.Address.Zip ?? "") ? "" : $"{vendor?.Address.City ?? ""}, {vendor?.Address.State ?? ""} {vendor?.Address.Zip ?? ""}";

        var packinglist = new Models.PackingList() {
            Customer = new() {
                Name = order.Customer.Name,
                Line1 = order.Shipping.Address.Line1,
                Line2 = custLine2Str,
                Line3 = order.Shipping.PhoneNumber
            },
            Vendor = new() {
                Name = vendor?.Name ?? "",
                Line1 = vendor?.Address.Line1 ?? "",
                Line2 = vendLine2Str,
                Line3 = vendor?.PhoneNumber ?? ""
            },
            Date = order.OrderDate,
            OrderName = order.Name,
            OrderNumber = order.Number,
            DrawerBoxes = drawerBoxes,
            Cabinets = cabinets,
            Doors = new()
        };
        
        return packinglist;

    }

    private async Task<Response<Company?>> GetCompany(Guid companyId) {
        return await _bus.Send(new GetCompanyById.Query(companyId));
    }

}
