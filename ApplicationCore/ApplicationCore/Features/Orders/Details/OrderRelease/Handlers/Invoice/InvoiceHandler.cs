using Microsoft.Extensions.Logging;
using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.Invoice.Models;
using ApplicationCore.Features.Shared.Services;
using ApplicationCore.Features.Companies.Contracts;
using ApplicationCore.Features.Companies.Contracts.Entities;

namespace ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.Invoice;

internal class InvoiceHandler {

    private readonly ILogger<InvoiceHandler> _logger;
    private readonly CompanyDirectory.GetVendorByIdAsync _getVendorById;
    private readonly CompanyDirectory.GetCustomerByIdAsync _getCustomerById;
    private readonly IFileReader _fileReader;

    public InvoiceHandler(ILogger<InvoiceHandler> logger, CompanyDirectory.GetVendorByIdAsync getVendorById, CompanyDirectory.GetCustomerByIdAsync getCustomerById, IFileReader fileReader) {
        _logger = logger;
        _getVendorById = getVendorById;
        _getCustomerById = getCustomerById;
        _fileReader = fileReader;
    }

    public async Task Handle(Order order, string outputDir) {

        if (!order.Products.Any()) {
            return;
        }

        Models.Invoice invoice = await CreateInvoice(order);

        var service = new InvoiceService();
        try {
            var wb = service.GenerateInvoice(invoice);
            string outputFile = _fileReader.GetAvailableFileName(outputDir, $"{order.Number} - {order.Name} Invoice", "xlsx");
            wb.SaveAs(outputFile);
        } catch {
        }

    }

    private async Task<Models.Invoice> CreateInvoice(Order order) {

        Vendor? vendor = await _getVendorById(order.VendorId);
        if (vendor is null) {
            _logger.LogError("Could not find vendor {VendorId}", order.VendorId);
        }

        var custLine2Str = string.IsNullOrWhiteSpace(order.Shipping.Address.Country + order.Shipping.Address.State + order.Shipping.Address.Zip) ? "" : $"{order.Shipping.Address.City}, {order.Shipping.Address.State} {order.Shipping.Address.Zip}";
        var vendLine2Str = string.IsNullOrWhiteSpace(vendor?.Address.Country + vendor?.Address.State + vendor?.Address.Zip) ? "" : $"{vendor?.Address.City}, {vendor?.Address.State} {vendor?.Address.Zip}";

        var drawerBoxes = order.Products
                        .OfType<DovetailDrawerBoxProduct>()
                        .Select(b => new DrawerBoxItem() {
                            Line = b.ProductNumber,
                            Qty = b.Qty,
                            Description = b.GetDescription(),
                            Height = b.Height.AsInchFraction().ToString(),
                            Width = b.Width.AsInchFraction().ToString(),
                            Depth = b.Depth.AsInchFraction().ToString(),
                            Price = b.UnitPrice.ToString("$0.00"),
                            ExtPrice = (b.UnitPrice * b.Qty).ToString("$0.00")
                        }).ToList();

        var cabinets = order.Products
                        .OfType<Cabinet>()
                        .Select(b => new CabinetItem() {
                            Line = b.ProductNumber,
                            Qty = b.Qty,
                            Description = b.GetDescription(),
                            Height = b.Height.AsInchFraction().ToString(),
                            Width = b.Width.AsInchFraction().ToString(),
                            Depth = b.Depth.AsInchFraction().ToString(),
                            Price = b.UnitPrice.ToString("$0.00"),
                            ExtPrice = (b.UnitPrice * b.Qty).ToString("$0.00")
                        }).ToList();

        var doors = new List<DoorItem>();
        var closetParts = new List<ClosetPartItem>();

        var customer = await _getCustomerById(order.CustomerId);

        var invoice = new Models.Invoice() {
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
            Total = order.Total,
            Discount = order.PriceAdjustment,
            NetAmount = order.AdjustedSubTotal,
            SalesTax = order.Tax,
            Shipping = order.Shipping.Price,
            SubTotal = order.SubTotal,
            DrawerBoxes = drawerBoxes,
            Cabinets = cabinets,
            ClosetParts = closetParts,
            Doors = doors
        };

        return invoice;

    }

}
