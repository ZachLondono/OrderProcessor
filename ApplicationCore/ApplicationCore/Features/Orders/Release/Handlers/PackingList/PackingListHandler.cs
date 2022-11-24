using ApplicationCore.Features.Companies.Queries;
using ApplicationCore.Features.ExcelTemplates.Contracts;
using ApplicationCore.Features.ExcelTemplates.Domain;
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

        var config = new ClosedXMLTemplateConfiguration() { TemplateFilePath = notification.ReleaseProfile.PackingListTemplatePath };
        var outputDir = notification.ReleaseProfile.PackingListOutputDirectory;
        var doPrint = notification.ReleaseProfile.PrintPackingList;

        var custLine2Str = string.IsNullOrWhiteSpace(customer.Address.Country + customer.Address.State + customer.Address.Zip) ? "" : $"{customer.Address.City}, {customer.Address.State} {customer.Address.Zip}";
        var vendLine2Str = string.IsNullOrWhiteSpace(vendor.Address.Country + vendor.Address.State + vendor.Address.Zip) ? "" : $"{vendor.Address.City}, {vendor.Address.State} {vendor.Address.Zip}";

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
            ItemCount = order.Boxes.Sum(b => b.Qty),
            OrderName = order.Name,
            OrderNumber = order.Number,
            Volume = "", // TODO calculate volume
            Weight = "", // TODO calculate weight
            Items = order.Boxes.Select(b => new Item() {
                Line = b.LineInOrder,
                Qty = b.Qty,
                Description = "Drawer Box",
                Logo = b.Options.Logo ? "Y" : "N",
                Height = b.Height.AsInchFraction().ToString(),
                Width = b.Width.AsInchFraction().ToString(),
                Depth = b.Depth.AsInchFraction().ToString()
            }).ToList()
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
