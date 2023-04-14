using ApplicationCore.Features.Companies.Contracts;
using ApplicationCore.Features.Companies.Contracts.Entities;
using ApplicationCore.Features.Companies.Contracts.ValueObjects;
using ApplicationCore.Features.Orders.Loader.Dialog;
using ApplicationCore.Features.Orders.Loader.Providers.DoorOrderModels;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Shared.Domain;
using ApplicationCore.Features.Shared.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Office.Interop.Excel;
using CustomerAddress = ApplicationCore.Features.Companies.Contracts.ValueObjects.Address;
using OrderAddress = ApplicationCore.Features.Orders.Shared.Domain.ValueObjects.Address;

namespace ApplicationCore.Features.Orders.Loader.Providers;

internal class DoorSpreadsheetOrderProvider : IOrderProvider {

    public IOrderLoadingViewModel? OrderLoadingViewModel { get; set; }

    private readonly IFileReader _fileReader;
    private readonly DoorOrderProviderOptions _options;
    private readonly ILogger<DoorSpreadsheetOrderProvider> _logger;
    private readonly CompanyDirectory.InsertCustomerAsync _insertCustomerAsync;
    private readonly CompanyDirectory.GetCustomerIdByNameAsync _getCustomerByNamAsync;

    public DoorSpreadsheetOrderProvider(IFileReader fileReader, IOptions<DoorOrderProviderOptions> options, ILogger<DoorSpreadsheetOrderProvider> logger, CompanyDirectory.InsertCustomerAsync insertCustomerAsync, CompanyDirectory.GetCustomerIdByNameAsync getCustomerByNamAsync) {
        _fileReader = fileReader;
        _options = options.Value;
        _logger = logger;
        _insertCustomerAsync = insertCustomerAsync;
        _getCustomerByNamAsync = getCustomerByNamAsync;
    }

    public async Task<OrderData?> LoadOrderData(string source) {

        if (!_fileReader.DoesFileExist(source)) {
            OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, "Could not access given filepath");
            return null;
        }

        var extension = Path.GetExtension(source);
        if (extension is null || (extension != ".xlsx" && extension != ".xlsm")) {
            OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, "Given filepath is not an excel document");
            return null;
        }

        Application? app = null;
        Workbook? workbook = null;

        try {

            app = new() {
                DisplayAlerts = false,
                Visible = false
            };

            workbook = app.Workbooks.Open(source, ReadOnly: true);
            Worksheet? orderSheet = workbook.Sheets["MDF"];

            if (orderSheet is null) {
                OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, "Could not find MDF sheet in workbook");
                return null;
            }

            var header = OrderHeader.ReadFromWorksheet(orderSheet);

            List<LineItem> lines = new();
            int line = 0;
            while (true) {

                line++;

                var lineVal = orderSheet.Range["LineNumStart"].Offset[line].Value2;
                if (lineVal is null || lineVal.ToString() == "") {
                    break;
                }

                try { 
                    lines.Add(LineItem.ReadFromWorksheet(orderSheet, line));
                } catch (Exception ex) {

                    OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, $"Error reading line item at line {line}");
                    _logger.LogError(ex, "Exception thrown while reading line item from workbook");

                }

            }

            OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Info, $"{lines.Count} line items read from workbook");

            var vendorId = Guid.Parse(_options.VendorIds[header.VendorName]);
            var customerId = await GetCustomerId(header);

            var data = MapWorkbookData(header, lines, vendorId, customerId ?? Guid.Empty);

            return data;

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

    private async Task<Guid?> GetCustomerId(OrderHeader header) {

        Guid? result = await _getCustomerByNamAsync(header.CompanyName);

        if (result is not null) {
            return (Guid) result;
        }

        var address = ParseCompanyAddress(header.Address1, header.Address2);

        var billingContact = new Contact() {
            Name = header.InvoiceFirstName,
            Phone = header.Phone, 
            Email = header.InvoiceEmail 
        };
        
        var shippingContact = new Contact() {
            Name = header.ConfirmationFirstName,
            Phone = header.Phone,
            Email = header.ConfirmationEmail 
        };

        var customer = Customer.Create(header.CompanyName, string.Empty, shippingContact, address, billingContact, address);
        await _insertCustomerAsync(customer);

        return customer.Id;

    }

    public static OrderData MapWorkbookData(OrderHeader header, List<LineItem> items, Guid vendorId, Guid customerId) {

        OrderAddress address = ParseOrderAddress(header.Address1, header.Address2);

        var units = header.GetUnitType();

        return new OrderData() {

            Name = header.JobName,
            Number = header.TrackingNumber,
            OrderDate = header.OrderDate,

            VendorId = vendorId,
            CustomerId = customerId,

            Comment = string.Empty,
            AdditionalItems = new(),
            Info = new(),
            Rush = false,

            Tax = 0,
            PriceAdjustment = 0,
            Billing = new() {
                Address = address,
                InvoiceEmail = header.InvoiceEmail,
                PhoneNumber = header.Phone
            },

            Shipping = new() {
                Address = address,
                Contact = header.ConfirmationFirstName,
                Method = "",
                PhoneNumber = header.Phone,
                Price = header.Freight
            },

            Products = items.Select(i => MapLineItem(i, header, units)).ToList()

        };

    }

    private static CustomerAddress ParseCompanyAddress(string address1, string address2) {
        
        string city = string.Empty;
        string state = string.Empty;
        string zip = string.Empty;

        // This method will not work if there is no ',' between the city and state or if there is a ',' between the state and zip code

        var splitA = address2.Split(',');
        if (splitA.Any()) {

            city = splitA.First().Trim();

            var splitB = splitA.Skip(1)
                                .FirstOrDefault()?
                                .Trim()
                                .Split(' ')
                                .Where(s => !string.IsNullOrWhiteSpace(s));

            if (splitB is not null && splitB.Any()) {
                state = splitB.First();
                zip = splitB.Skip(1).FirstOrDefault() ?? string.Empty;
            }

        }

        return new CustomerAddress() {
            Line1 = address1,
            City = city,
            State = state,
            Zip = zip
        };

    }

    private static OrderAddress ParseOrderAddress(string address1, string address2) {
        
        string city = string.Empty;
        string state = string.Empty;
        string zip = string.Empty;

        // This method will not work if there is no ',' between the city and state or if there is a ',' between the state and zip code

        var splitA = address2.Split(',');
        if (splitA.Any()) {

            city = splitA.First().Trim();

            var splitB = splitA.Skip(1)
                                .FirstOrDefault()?
                                .Trim()
                                .Split(' ')
                                .Where(s => !string.IsNullOrWhiteSpace(s));

            if (splitB is not null && splitB.Any()) {
                state = splitB.First();
                zip = splitB.Skip(1).FirstOrDefault() ?? string.Empty;
            }

        }

        return new OrderAddress() {
            Line1 = address1,
            City = city,
            State = state,
            Zip = zip
        };

    }

    public static IProduct MapLineItem(LineItem lineItem, OrderHeader header, OrderHeader.UnitType units) {

        DoorType type = DoorType.Door;
        if (lineItem.Description.Contains("Drawer") || lineItem.Description.Contains("Dwr")) {
            type = DoorType.DrawerFront;
        }

        DoorFrame frame;
        Dimension height, width, thickness, panelDrop;
        switch (units) {

            case OrderHeader.UnitType.Inches:
                height = Dimension.FromInches(lineItem.Height);
                width = Dimension.FromInches(lineItem.Width);
                thickness = Dimension.FromInches(lineItem.Thickness);
                panelDrop = Dimension.FromInches(header.PanelDrop);
                frame = new() {
                    LeftStile = Dimension.FromInches(lineItem.LeftStile),
                    RightStile = Dimension.FromInches(lineItem.RightStile),
                    TopRail = Dimension.FromInches(lineItem.TopRail),
                    BottomRail = Dimension.FromInches(lineItem.BottomRail),
                };
                break;

            case OrderHeader.UnitType.Millimeters:
                height = Dimension.FromMillimeters(lineItem.Height);
                width = Dimension.FromMillimeters(lineItem.Width);
                thickness = Dimension.FromMillimeters(lineItem.Thickness);
                panelDrop = Dimension.FromMillimeters(header.PanelDrop);
                frame = new() {
                    LeftStile = Dimension.FromMillimeters(lineItem.LeftStile),
                    RightStile = Dimension.FromMillimeters(lineItem.RightStile),
                    TopRail = Dimension.FromMillimeters(lineItem.TopRail),
                    BottomRail = Dimension.FromMillimeters(lineItem.BottomRail),
                };
                break;

            default:
                height = Dimension.Zero;
                width = Dimension.Zero;
                thickness = Dimension.Zero;
                panelDrop = Dimension.Zero;
                frame = new() {
                    LeftStile = Dimension.Zero,
                    RightStile = Dimension.Zero,
                    TopRail = Dimension.Zero,
                    BottomRail = Dimension.Zero,
                };
                break;

        }


        return MDFDoorProduct.Create(lineItem.UnitPrice, "", lineItem.Qty, lineItem.PartNumber, type, height, width, lineItem.Note, frame, lineItem.Material, thickness, header.Style, header.EdgeProfile, header.PanelDetail, panelDrop, header.Color);

    }



}
