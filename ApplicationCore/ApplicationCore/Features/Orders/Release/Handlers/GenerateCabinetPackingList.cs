using ApplicationCore.Features.ExcelTemplates.Domain;
using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Shared;
using ApplicationCore.Infrastructure;

namespace ApplicationCore.Features.Orders.Release.Handlers;

internal class GenerateCabinetPackingList : DomainListener<TriggerOrderReleaseNotification> {

    private readonly IUIBus _uibus;
    private readonly IFileReader _fileReader;
    private readonly VendorInfo.GetVendorInfoById _getVendorInfo;

    public GenerateCabinetPackingList(IUIBus uibus, IFileReader fileReader, VendorInfo.GetVendorInfoById getVendorInfo) {
        _uibus = uibus;
        _fileReader = fileReader;
        _getVendorInfo = getVendorInfo;
    }

    public override async Task Handle(TriggerOrderReleaseNotification notification) {
        
        if (!notification.ReleaseProfile.GenerateCabinetPackingList) {
            _uibus.Publish(new OrderReleaseInfoNotification("Not generating cabinet packing list, because option was disabled"));
            return;
        }

        var order = notification.Order;
        string templatePath = notification.ReleaseProfile.CabinetPackingListTemplateFilePath;
        string outputDirectory = notification.ReleaseProfile.CabinetPackingListOutputDirectory;

        if (!File.Exists(templatePath)) {
            _uibus.Publish(new OrderReleaseErrorNotification($"Cabinet packing list template file cannot be found '{templatePath}'"));
            return;
        }

        if (!Directory.Exists(outputDirectory)) {
            _uibus.Publish(new OrderReleaseErrorNotification($"Cabinet packing list output directory cannot be found '{outputDirectory}'"));
            return;
        }

        try {

            await FillTemplate(order, templatePath, outputDirectory);

        } catch (Exception ex) {

            _uibus.Publish(new OrderReleaseErrorNotification($"Exception thrown while generating job summary. {ex.Message}"));

        }

    }

    public async Task FillTemplate(Order order, string templateFilePath, string outputDirectory) {

        IEnumerable<Model.Item> items = order.Products
                                                .Where(p => p is Cabinet)
                                                .Cast<Cabinet>()
                                                .Select(c => new Model.Item() {
                                                    CabNum = c.ProductNumber,
                                                    Description = c.GetType().Name,
                                                    Qty = c.Qty,
                                                    Width = c.Width.AsInches(),
                                                    Height = c.Height.AsInches(),
                                                    Depth = c.Depth.AsInches()
                                                });

        var vendor = (await _getVendorInfo(order.VendorId)) ?? new();

        var model = new Model() {
            OrderName = order.Name,
            OrderNumber = order.Number,
            Items = items,
            ItemCount = items.Count(),
            Date = order.OrderDate.ToShortDateString(),
            Vendor = new() {
                Name = vendor.Name,
                Line1 = vendor.Address.Line1,
                Line2 = vendor.Address.Line2,
                Line3 = GetAddressLine3(vendor.Address)
            },
            Customer = new() {
                Name = order.Customer.Name,
                Line1 = order.Shipping.Address.Line1,
                Line2 = order.Shipping.Address.Line2,
                Line3 = GetAddressLine3(order.Shipping.Address)
            }
        };

        using var stream = _fileReader.OpenReadFileStream(templateFilePath);
        var template = new ClosedXMLTemplate(stream);
        template.AddVariable(model);
        var result = template.Generate();

        if (result.HasErrors) {

            foreach (var error in result.ParsingErrors) {
                _uibus.Publish(new OrderReleaseErrorNotification($"Parsing error [{error.Range}] - {error.Message}"));
            }

        }

        string outputFile = GetAvailableFileName(outputDirectory, $"{order.Number} - Cabinet Packing List");

        template.SaveAs(outputFile);

    }

    public static string GetAddressLine3(Address address) {

        if (string.IsNullOrWhiteSpace(address.City) && string.IsNullOrWhiteSpace(address.State) && string.IsNullOrWhiteSpace(address.Zip)) {
            return string.Empty;
        }

        return $"{address.City} {address.State}, {address.Zip}";

    }

    public string GetAvailableFileName(string direcotry, string filename, string fileExtension = "xlsx") {

        int index = 1;

        string filepath = Path.Combine(direcotry, $"{filename}.{fileExtension}");

        while (_fileReader.DoesFileExist(filepath)) {

            filepath = Path.Combine(direcotry, $"{filename} ({index++}).{fileExtension}");

        }

        return filepath;

    }

    public class Model {

        public required Company Vendor { get; set; }
        public required Company Customer { get; set; }
        public required string Date { get; set; }
        public required string OrderNumber { get; set; }
        public required string OrderName { get; set; }
        public required int ItemCount { get; set; }
        public required IEnumerable<Item> Items { get; set; }

        public class Company {
            public required string Name { get; set; }
            public required string Line1 { get; set; }
            public required string Line2 { get; set; }
            public required string Line3 { get; set; }
        }

        public class Item {
            public required int CabNum { get; set; }
            public required int Qty { get; set; }
            public required string Description { get; set; }
            public required double Height { get; set; }
            public required double Width { get; set; }
            public required double Depth { get; set; }
        }

    }

    public class Vendor {

        public string Name { get; set; } = string.Empty;

        public Address Address { get; set; } = new();

    }

    public static class VendorInfo {

        public delegate Task<Vendor?> GetVendorInfoById(Guid vendorId);

    }

}
