using ApplicationCore.Features.Companies.Queries;
using ApplicationCore.Infrastructure;
using ApplicationCore.Features.Shared;
using Microsoft.Extensions.Logging;
using Company = ApplicationCore.Features.Companies.Domain.Company;
using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using ApplicationCore.Features.Orders.Shared.Domain.Products;

namespace ApplicationCore.Features.Orders.Release.Handlers.Invoice;

internal class InvoiceHandler : DomainListener<TriggerOrderReleaseNotification> {

    private readonly ILogger<InvoiceHandler> _logger;
    private readonly IBus _bus;
    private readonly IUIBus _uibus;
    private readonly IFileReader _fileReader;

    public InvoiceHandler(ILogger<InvoiceHandler> logger, IBus bus, IUIBus uibus, IFileReader fileReader) {
        _logger = logger;
        _bus = bus;
        _uibus = uibus;
        _fileReader = fileReader;
    }

    public override async Task Handle(TriggerOrderReleaseNotification notification) {

        if (!notification.ReleaseProfile.GenerateInvoice) {
            _uibus.Publish(new OrderReleaseInfoNotification("Not creating invoice, because option was disabled"));
            return;
        }

        var order = notification.Order;

        if (!order.Products.Any()) {
            _uibus.Publish(new OrderReleaseInfoNotification("Not creating invoice, because there where no products found"));
            return;
        }

        Models.Invoice invoice = await CreateInvoice(order);

        var outputDir = notification.ReleaseProfile.InvoiceOutputDirectory;
        var doPrint = notification.ReleaseProfile.PrintInvoice;

        var service = new InvoiceService();
        try {
            var wb = service.GenerateInvoice(invoice);
            string outputFile = _fileReader.GetAvailableFileName(outputDir, $"{order.Number} - {order.Name} Invoice", "xlsx");
            wb.SaveAs(outputFile);
            _uibus.Publish(new OrderReleaseFileCreatedNotification("Packing List created", outputFile));
        } catch (Exception ex) {
            _uibus.Publish(new OrderReleaseErrorNotification($"Error creating invoice {ex.Message}"));
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

        if (hasError || vendor is null) {
            _uibus.Publish(new OrderReleaseErrorNotification("Could not load customer or vendor information for order"));
        }

        var custLine2Str = string.IsNullOrWhiteSpace(order.Shipping.Address.Country + order.Shipping.Address.State + order.Shipping.Address.Zip) ? "" : $"{order.Shipping.Address.City}, {order.Shipping.Address.State} {order.Shipping.Address.Zip}";
        var vendLine2Str = string.IsNullOrWhiteSpace(vendor?.Address.Country + vendor?.Address.State + vendor?.Address.Zip) ? "" : $"{vendor?.Address.City}, {vendor?.Address.State} {vendor?.Address.Zip}";

        var drawerBoxes = order.Products
                        .OfType<DovetailDrawerBoxProduct>()
                        .Select(b => new Models.DrawerBoxItem() {
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
                        .Select(b => new Models.CabinetItem() {
                            Line = b.ProductNumber,
                            Qty = b.Qty,
                            Description = b.Description,
                            Height = b.Height.AsInchFraction().ToString(),
                            Width = b.Width.AsInchFraction().ToString(),
                            Depth = b.Depth.AsInchFraction().ToString(),
                            Price = b.UnitPrice.ToString("$0.00"),
                            ExtPrice = (b.UnitPrice * b.Qty).ToString("$0.00")
                        }).ToList();

        var doors = new List<Models.DoorItem>();
        var closetParts = new List<Models.ClosetPartItem>();

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
