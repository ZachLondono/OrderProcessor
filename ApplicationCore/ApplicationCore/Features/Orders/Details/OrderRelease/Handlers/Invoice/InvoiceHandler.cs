using ApplicationCore.Features.Companies.Queries;
using Microsoft.Extensions.Logging;
using Company = ApplicationCore.Features.Companies.Domain.Company;
using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.Invoice.Models;
using ApplicationCore.Infrastructure.Bus;
using ApplicationCore.Features.Shared.Services;

namespace ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.Invoice;

internal class InvoiceHandler {

    private readonly ILogger<InvoiceHandler> _logger;
    private readonly IBus _bus;
    private readonly IFileReader _fileReader;

    public InvoiceHandler(ILogger<InvoiceHandler> logger, IBus bus, IFileReader fileReader) {
        _logger = logger;
        _bus = bus;
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

        /*if (hasError || vendor is null) {
            
        }*/

        var custLine2Str = string.IsNullOrWhiteSpace(order.Shipping.Address.Country + order.Shipping.Address.State + order.Shipping.Address.Zip) ? "" : $"{order.Shipping.Address.City}, {order.Shipping.Address.State} {order.Shipping.Address.Zip}";
        var vendLine2Str = string.IsNullOrWhiteSpace(vendor?.Address.Country + vendor?.Address.State + vendor?.Address.Zip) ? "" : $"{vendor?.Address.City}, {vendor?.Address.State} {vendor?.Address.Zip}";

        var drawerBoxes = order.Products
                        .OfType<DovetailDrawerBoxProduct>()
                        .Select(b => new DrawerBoxItem() {
                            Line = b.ProductNumber,
                            Qty = b.Qty,
                            Description = b.Description,
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
                            Description = b.Description,
                            Height = b.Height.AsInchFraction().ToString(),
                            Width = b.Width.AsInchFraction().ToString(),
                            Depth = b.Depth.AsInchFraction().ToString(),
                            Price = b.UnitPrice.ToString("$0.00"),
                            ExtPrice = (b.UnitPrice * b.Qty).ToString("$0.00")
                        }).ToList();

        var doors = new List<DoorItem>();
        var closetParts = new List<ClosetPartItem>();

        var invoice = new Models.Invoice() {
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

    private async Task<Response<Company?>> GetCompany(Guid companyId) {
        return await _bus.Send(new GetCompanyById.Query(companyId));
    }

}
