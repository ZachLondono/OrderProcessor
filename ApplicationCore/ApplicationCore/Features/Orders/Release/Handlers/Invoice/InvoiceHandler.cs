using ApplicationCore.Features.Companies.Queries;
using ApplicationCore.Features.ExcelTemplates.Contracts;
using ApplicationCore.Features.ExcelTemplates.Domain;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Orders.Release.Handlers.Invoice.Models;
using ApplicationCore.Infrastructure;
using ApplicationCore.Shared;
using ClosedXML.Excel;
using Microsoft.Extensions.Logging;
using Company = ApplicationCore.Features.Companies.Domain.Company;

namespace ApplicationCore.Features.Orders.Release.Handlers.Invoice;

internal class InvoiceHandler : DomainListener<TriggerOrderReleaseNotification> {

    private readonly ILogger<InvoiceHandler> _logger;
    private readonly IBus _bus;
    private readonly IUIBus _uibus;
    private readonly IFileReader _reader;

    public InvoiceHandler(ILogger<InvoiceHandler> logger, IBus bus, IUIBus uibus, IFileReader reader) {
        _logger = logger;
        _bus = bus;
        _uibus = uibus;
        _reader = reader;
    }

    public override async Task Handle(TriggerOrderReleaseNotification notification) {

        if (!notification.ReleaseProfile.GenerateInvoice) {
            _uibus.Publish(new OrderReleaseInfoNotification("Not creating invoice, because option was disabled"));
            return;
        }

        var order = notification.Order;

        bool didError = false;

        Company? customer = null;
        var custQuery = await GetCompany(order.CustomerId);
        custQuery.Match(
            (company) => {
                customer = company;
            },
            (error) => {
                didError = true;
                _logger.LogError("Error loading customer {Error}", error);
            }
        );

        Company? vendor = null;
        var vendorQuery = await GetCompany(order.VendorId);
        custQuery.Match(
            (company) => {
                vendor = company;
            },
            (error) => {
                didError = true;
                _logger.LogError("Error loading vendor {Error}", error);
            }
        );

        if (didError || customer is null || vendor is null) {
            _uibus.Publish(new OrderReleaseErrorNotification("Could not load customer or vendor information for order"));
            return;
        }

        var config = new ClosedXMLTemplateConfiguration() { TemplateFilePath = notification.ReleaseProfile.InvoiceTemplatePath };
        var outputDir = notification.ReleaseProfile.InvoiceOutputDirectory;
        var doPrint = notification.ReleaseProfile.PrintInvoice;

        var custLine2Str = string.IsNullOrWhiteSpace(customer.Address.Country + customer.Address.State + customer.Address.Zip) ? "" : $"{customer.Address.City}, {customer.Address.State} {customer.Address.Zip}";
        var vendLine2Str = string.IsNullOrWhiteSpace(vendor.Address.Country + vendor.Address.State + vendor.Address.Zip) ? "" : $"{vendor.Address.City}, {vendor.Address.State} {vendor.Address.Zip}";

        int line = 1;
        var items = order.Products
                        .Where(p => p is DovetailDrawerBoxProduct)
                        .Cast<DovetailDrawerBoxProduct>()
                        .Select(b => new Item() {
                            Line = line++,
                            Qty = b.Qty,
                            Description = "Drawer Box",
                            Logo = b.Options.Logo == LogoPosition.None ? "N" : "Y",
                            Height = b.Height.AsInchFraction().ToString(),
                            Width = b.Width.AsInchFraction().ToString(),
                            Depth = b.Depth.AsInchFraction().ToString(),
                            Price = b.UnitPrice.ToString("$0.00"),
                            ExtPrice = (b.UnitPrice * b.Qty).ToString("$0.00")
                        }).ToList();

        var packinglist = new Models.PackingList() {
            Customer = new() {
                Name = customer.Name,
                Line1 = customer.Address.Line1,
                Line2 = custLine2Str,
                Line3 = customer.PhoneNumber
            },
            Vendor = new() {
                Name = vendor.Name,
                Line1 = vendor.Address.Line1,
                Line2 = vendLine2Str,
                Line3 = vendor.PhoneNumber
            },
            Date = order.OrderDate,
            ItemCount = order.Products.Sum(b => b.Qty),
            OrderName = order.Name,
            OrderNumber = order.Number,
            Total = order.Total.ToString("$0.00"), // TODO: use excel formatting instead of string formatting
            Discount = order.PriceAdjustment.ToString("0.00"),
            NetAmount = order.AdjustedSubTotal.ToString("0.00"),
            SalesTax = order.Tax.ToString("0.00"),
            SubTotal = order.SubTotal.ToString("0.00"),
            Items = items
        };

        var response = await _bus.Send(new FillTemplateRequest(packinglist, outputDir, $"{order.Number} - {order.Name} INVOICE", doPrint, config));

        FillTemplateResponse? invResponse = null;
        didError = false;
        response.Match(
            inv => invResponse = inv,
            error => {
                _logger.LogInformation("Error creating invoice {Error}", error);
                _uibus.Publish(new OrderReleaseErrorNotification($"Error creating nvoice {error.Details}"));
                didError = true;
            }
        );

        if (didError || invResponse is null) return;

        // TODO: check if order has a discount
        if (order.PriceAdjustment != 0M) {

            using var stream = _reader.OpenReadFileStream(invResponse.FilePath, FileAccess.ReadWrite);
            using var workbook = new XLWorkbook(stream);
            var sheet = workbook.Worksheet("Invoice");

            var rng = workbook.NamedRange("DiscountRows").Ranges.First();
            var first = rng.Rows().First().RowNumber();
            int last = rng.Rows().Last().RowNumber();
            sheet.Rows(first, last).Hide();

            workbook.Save();

        }

        _logger.LogInformation("Invoice created: {FilePath}", invResponse.FilePath);
        _uibus.Publish(new OrderReleaseSuccessNotification($"Invoice created {invResponse.FilePath}"));

    }

    private async Task<Response<Company>> GetCompany(Guid companyId) {
        return await _bus.Send(new GetCompanyById.Query(companyId));
    }

}
