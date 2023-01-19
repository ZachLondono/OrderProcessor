using ApplicationCore.Features.Companies.Queries;
using ApplicationCore.Features.ExcelTemplates.Contracts;
using ApplicationCore.Features.ExcelTemplates.Domain;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Orders.Release.Handlers.PackingList.Models;
using ApplicationCore.Infrastructure;
using Microsoft.Extensions.Logging;
using Company = ApplicationCore.Features.Companies.Domain.Company;

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

        bool didError = false;

        Company? vendor = null;
        var vendorQuery = await GetCompany(order.VendorId);
        vendorQuery.Match(
            (company) => {
                vendor = company;
            },
            (error) => {
                didError = true;
                _logger.LogError("Error loading vendor {Error}", error);
            }
        );

        if (didError || vendor is null) {
            _uibus.Publish(new OrderReleaseErrorNotification("Could not load customer or vendor information for order"));
            return;
        }

        var config = new ClosedXMLTemplateConfiguration() { TemplateFilePath = notification.ReleaseProfile.PackingListTemplatePath };
        var outputDir = notification.ReleaseProfile.PackingListOutputDirectory;
        var doPrint = notification.ReleaseProfile.PrintPackingList;

        var custLine2Str = string.IsNullOrWhiteSpace(order.Shipping.Address.Country + order.Shipping.Address.State + order.Shipping.Address.Zip) ? "" : $"{order.Shipping.Address.City}, {order.Shipping.Address.State} {order.Shipping.Address.Zip}";
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
                            Depth = b.Depth.AsInchFraction().ToString()
                        }).ToList();

        var packinglist = new Models.PackingList() {
            Customer = new() {
                Name = order.Customer.Name,
                Line1 = order.Shipping.Address.Line1,
                Line2 = custLine2Str,
                Line3 = order.Shipping.PhoneNumber
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
            Volume = "", // TODO calculate volume
            Weight = "", // TODO calculate weight
            Items = items
        };

        var plResponse = await _bus.Send(new FillTemplateRequest(packinglist, outputDir, $"{order.Number} - {order.Name} PACKING LIST", doPrint, config));

        plResponse.Match(
            (response) => {

                _logger.LogInformation("Packing list created: {FilePath}", response.FilePath);
                _uibus.Publish(new OrderReleaseSuccessNotification($"Packing list created {response.FilePath}"));

            },
            (error) => {

                _logger.LogInformation("Error creating packing list {Error}", error);
                _uibus.Publish(new OrderReleaseErrorNotification($"Packing list was not created\n{error.Details}"));

            }
        );

    }

    private async Task<Response<Company?>> GetCompany(Guid companyId) {
        return await _bus.Send(new GetCompanyById.Query(companyId));
    }

}
