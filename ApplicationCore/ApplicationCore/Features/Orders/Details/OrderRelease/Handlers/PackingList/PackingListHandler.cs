using ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.PackingList.Models;
using Microsoft.Extensions.Logging;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using ApplicationCore.Features.Shared.Services;
using ApplicationCore.Features.Companies.Contracts;
using ApplicationCore.Features.Companies.Contracts.Entities;
using ApplicationCore.Pages.CustomerDetails;

namespace ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.PackingList;

internal class PackingListHandler {

    private readonly ILogger<PackingListHandler> _logger;
    private readonly CompanyDirectory.GetVendorByIdAsync _getVendorById;
    private readonly CompanyDirectory.GetCustomerByIdAsync _getCustomerById;
    private readonly IFileReader _fileReader;

    public PackingListHandler(ILogger<PackingListHandler> logger, CompanyDirectory.GetVendorByIdAsync getVendorById, CompanyDirectory.GetCustomerByIdAsync getCustomerById, IFileReader fileReader) {
        _getVendorById = getVendorById;
        _getCustomerById = getCustomerById;
        _logger = logger;
        _fileReader = fileReader;
    }

    public async Task Handle(Order order, string outputDir) {

        if (!order.Products.Any()) {
            return;
        }

        Models.PackingList packinglist = await CreatePackingList(order);

        var service = new PackingListService();
        try {
            var wb = service.GeneratePackingList(packinglist);
            string outputFile = _fileReader.GetAvailableFileName(outputDir, $"{order.Number} - {order.Name} PACKING LIST", ".xlsx");
            wb.SaveAs(outputFile);
        } catch {
        }

    }

    private async Task<Models.PackingList> CreatePackingList(Order order) {

        Vendor? vendor = await _getVendorById(order.VendorId);
        if (vendor is null) {
            _logger.LogError("Could not find vendor {VendorId}", order.VendorId);
        }

        var drawerBoxes = order.Products
                                .OfType<DovetailDrawerBoxProduct>()
                                .Select(b => new DrawerBoxItem() {
                                    Line = b.ProductNumber,
                                    Qty = b.Qty,
                                    Description = b.GetDescription(),
                                    Height = b.Height.AsInchFraction().ToString(),
                                    Width = b.Width.AsInchFraction().ToString(),
                                    Depth = b.Depth.AsInchFraction().ToString()
                                }).ToList();

        var cabinets = order.Products
                        .OfType<Cabinet>()
                        .Select(b => new CabinetItem() {
                            Line = b.ProductNumber,
                            Qty = b.Qty,
                            Description = b.GetDescription(),
                            Height = b.Height.AsInchFraction().ToString(),
                            Width = b.Width.AsInchFraction().ToString(),
                            Depth = b.Depth.AsInchFraction().ToString()
                        }).ToList();

        var closetParts = order.Products
                            .OfType<ClosetPart>()
                            .Select(b => new ClosetPartItem() {
                                Line = b.ProductNumber,
                                Qty = b.Qty,
                                Description = b.GetDescription(),
                                Length = b.Length.AsInchFraction().ToString(),
                                Width = b.Width.AsInchFraction().ToString()
                            })
                            .ToList();

        var doors = order.Products
                        .OfType<MDFDoorProduct>()
                        .Select(b => new DoorItem() {
                            Line = b.ProductNumber,
                            Qty = b.Qty,
                            Description = b.GetDescription(),
                            Height = b.Height.AsInchFraction().ToString(),
                            Width = b.Width.AsInchFraction().ToString()
                        })
                        .ToList();


        var custLine2Str = string.IsNullOrWhiteSpace(order.Shipping.Address.Country + order.Shipping.Address.State + order.Shipping.Address.Zip) ? "" : $"{order.Shipping.Address.City}, {order.Shipping.Address.State} {order.Shipping.Address.Zip}";
        var vendLine2Str = string.IsNullOrWhiteSpace(vendor?.Address.Country ?? "" + vendor?.Address.State ?? "" + vendor?.Address.Zip ?? "") ? "" : $"{vendor?.Address.City ?? ""}, {vendor?.Address.State ?? ""} {vendor?.Address.Zip ?? ""}";

        var customer = await _getCustomerById(order.CustomerId);

        var packinglist = new Models.PackingList() {
            Customer = new() {
                Name = customer?.Name ?? "",
                Line1 = order.Shipping.Address.Line1,
                Line2 = custLine2Str,
                Line3 = order.Shipping.PhoneNumber
            },
            Vendor = new() {
                Name = vendor?.Name ?? "",
                Line1 = vendor?.Address.Line1 ?? "",
                Line2 = vendLine2Str,
                Line3 = vendor?.Phone ?? ""
            },
            Date = order.OrderDate,
            OrderName = order.Name,
            OrderNumber = order.Number,
            DrawerBoxes = drawerBoxes,
            Cabinets = cabinets,
            ClosetParts = closetParts,
            Doors = doors
        };

        return packinglist;

    }

}
