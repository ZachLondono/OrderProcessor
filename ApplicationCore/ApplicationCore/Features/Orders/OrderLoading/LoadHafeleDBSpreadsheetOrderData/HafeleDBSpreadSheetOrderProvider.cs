using ApplicationCore.Features.Companies.Contracts.Entities;
using ApplicationCore.Features.Companies.Contracts.ValueObjects;
using ApplicationCore.Features.Companies.Customers.Commands;
using ApplicationCore.Features.Companies.Customers.Queries;
using ApplicationCore.Features.Orders.OrderLoading.Dialog;
using ApplicationCore.Features.Orders.OrderLoading.Models;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.Products.DrawerBoxes;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Infrastructure.Bus;
using ApplicationCore.Shared.Domain;
using ApplicationCore.Shared.Services;
using ApplicationCore.Shared.Settings;
using Microsoft.Extensions.Options;
using Microsoft.Office.Interop.Excel;
using ExcelApp = Microsoft.Office.Interop.Excel.Application;

namespace ApplicationCore.Features.Orders.OrderLoading.LoadHafeleDBSpreadsheetOrderData;

internal class HafeleDBSpreadSheetOrderProvider : IOrderProvider {

    public IOrderLoadWidgetViewModel? OrderLoadingViewModel { get; set; }

    private readonly HafeleDBOrderProviderSettings _settings;
    private readonly IFileReader _fileReader;
    private readonly IBus _bus;

    public HafeleDBSpreadSheetOrderProvider(IOptions<HafeleDBOrderProviderSettings> options, IFileReader fileReader, IBus bus) {
        _settings = options.Value;
        _fileReader = fileReader;
        _bus = bus;
    }

    public async Task<OrderData?> LoadOrderData(string source) {

        if (!_fileReader.DoesFileExist(source)) {
            OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, "Could not access given filepath");
            return null;
        }

        var extension = Path.GetExtension(source);
        if (extension is null || extension != ".xlsx") {
            OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, "Given filepath is not an excel document");
            return null;
        }

        ExcelApp? app = null;
        Workbook? workbook = null;

        try {

            app = new() {
                DisplayAlerts = false,
                Visible = false
            };

            workbook = app.Workbooks.Open(source, ReadOnly: true);

            var data = WorkbookOrderData.ReadWorkbook(workbook);
            if (data is null) {
                OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, "Could not load order data from workbook");
                return null;
            }

            return await MapWorkbookDataToOrderData(data);

        } catch (Exception ex) { 

            OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, $"Error occurred while reading order from workbook {ex}");

        } finally {

            workbook?.Close(SaveChanges: false);
            app?.Quit();

            // Clean up COM objects, calling these twice ensures it is fully cleaned up.
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();

        }

        return null;

    }

    public async Task<OrderData> MapWorkbookDataToOrderData(WorkbookOrderData workbookData) {

        bool metric = workbookData.OrderDetails.UnitFormat.Equals("metric", StringComparison.InvariantCultureIgnoreCase);

        var products = MapLineItemsToProduct(workbookData.Items, workbookData.GlobalDrawerSpecs.Material, metric).ToList();
        var shipping = new ShippingInfo() {
            Contact = workbookData.ContactInformation.Contact,
            Method = "Delivery",
            PhoneNumber = workbookData.ContactInformation.Phone,
            Price = 0M,
            Address = new() {
                Line1 = workbookData.ContactInformation.Address1,
                Line2 = workbookData.ContactInformation.Address2,
                Line3 = "",
                City = workbookData.ContactInformation.City,
                State = workbookData.ContactInformation.State,
                Zip = workbookData.ContactInformation.Zip,
                Country = "USA"
            }
        };

        // TODO: get billing info from vendor
        var billing = new BillingInfo() {
            InvoiceEmail = null,
            PhoneNumber = "",
            Address = new() {
                Line1 = "390 Cheyenne Drive",
                Line2 = "P.O. Box 4000",
                Line3 = "",
                City = "Archdale",
                State = "NC",
                Zip = "27263"
            }
        };

        string workingDirectory = Path.Combine(@"R:\DB ORDERS\Hafele\Confirmations\", $"{workbookData.OrderDetails.HafelePO} - {workbookData.OrderDetails.JobName} - {workbookData.ContactInformation.Company}");

        bool rush = workbookData.OrderDetails.ProductionTime.Contains("rush", StringComparison.InvariantCultureIgnoreCase);

        var customerId = await GetCustomerId(workbookData.ContactInformation);

        return new() {
            Number = workbookData.OrderDetails.HafelePO,
            Name = workbookData.OrderDetails.JobName,
            Products = products,
            Shipping = shipping,
            Billing = billing,
            DueDate = null,
            CustomerId = customerId ?? Guid.Empty,
            Rush = rush,
            WorkingDirectory = workingDirectory,
            OrderDate = workbookData.OrderDetails.OrderDate,
            VendorId = _settings.VendorId,
            Comment = workbookData.OrderComments,
            Info = new() {
                { "Hafele PO", workbookData.OrderDetails.HafelePO },
                { "Hafele Order Number", workbookData.OrderDetails.HafeleOrderNumber },
                { "Hafele Account Number", workbookData.OrderDetails.AccountNumber },
                { "Customer PO", workbookData.OrderDetails.PurchaseOrder }
            },
            AdditionalItems = new(),
            Tax = 0M,
            PriceAdjustment = 0M,
        };

    }

    public IEnumerable<IProduct> MapLineItemsToProduct(IEnumerable<LineItem> lineItems, string boxMaterialName, bool metric) {

        var frontBackThickness = Dimension.FromMillimeters(_settings.FrontBackThicknessMM);
        var sideThickness = Dimension.FromMillimeters(_settings.SideThicknessMM);
        var bottomThickness = Dimension.FromMillimeters(_settings.BottomThicknessMM);
        var frontBackHeightAdj = Dimension.FromMillimeters(_settings.FrontBackHeightAdjMM);

        return lineItems.Select(
            item => {

                Func<double, Dimension> dimConvert = metric ? Dimension.FromMillimeters : Dimension.FromInches;

                var frontBack = new DoweledDrawerBoxMaterial(boxMaterialName, frontBackThickness, true);
                var sides = new DoweledDrawerBoxMaterial(boxMaterialName, sideThickness, true);
                var bottom = new DoweledDrawerBoxMaterial(item.BottomMaterial, bottomThickness, true);

                return new DoweledDrawerBoxProduct(
                            Guid.NewGuid(),
                            item.UnitPrice,
                            item.Qty,
                            string.Empty,
                            item.Line,
                            dimConvert(item.Height),
                            dimConvert(item.Width),
                            dimConvert(item.Depth),
                            frontBack,
                            frontBack,
                            sides,
                            bottom,
                            false,
                            frontBackHeightAdj);

            });

    }

    private async Task<Guid?> GetCustomerId(ContactInformation contactInfo) {

        var result = await _bus.Send(new GetCustomerIdByName.Query(contactInfo.Company));

        var customerId = result.Match(
            id => id,
            error => null);

        if (customerId is not null) {
            return (Guid)customerId;
        }

        var billingContact = new Contact() {
            Name = contactInfo.Contact,
            Phone = contactInfo.Phone,
            Email = contactInfo.Email
        };

        var shippingContact = new Contact() {
            Name = contactInfo.Contact,
            Phone = contactInfo.Phone,
            Email = contactInfo.Email
        };

        Companies.Contracts.ValueObjects.Address address = new() {

        };

        var customer = Customer.Create(contactInfo.Company, string.Empty, shippingContact, address, billingContact, address);

        _ = await _bus.Send(new InsertCustomer.Command(customer, null));

        return customer.Id;

    }

}