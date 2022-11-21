using ApplicationCore.Features.Companies.Queries;
using ApplicationCore.Features.Orders.Domain.ValueObjects;
using ApplicationCore.Features.Orders.Loader.Providers.DTO;
using ApplicationCore.Infrastructure;
using ApplicationCore.Shared;
using ClosedXML.Excel;
using ApplicationCore.Features.Companies.Commands;
using ApplicationCore.Features.Companies.Domain.ValueObjects;
using ApplicationCore.Features.Companies.Domain;
using ApplicationCore.Features.Orders.Loader.Providers.Results;

namespace ApplicationCore.Features.Orders.Loader.Providers;

internal class OTExcelProvider : OrderProvider {

    private readonly IFileReader _fileReader;
    private readonly IBus _bus;

    public OTExcelProvider(IFileReader fileReader, IBus bus) {
        _fileReader = fileReader;
        _bus = bus;
    }

    public override async Task<OrderData?> LoadOrderData(string source) {

        using var stream = _fileReader.OpenReadFileStream(source);
        using var wb = new XLWorkbook(stream);

        var sheet = wb.Worksheet("Order");

        if (sheet is null) return null; //new Response<Order>(new Error() { Message = "Order sheet could not be found" });

        string number = sheet.Cell("OrderNumber").GetValue<string>();
        string name = sheet.Cell("OrderName").GetValue<string>();
        string dateStr = sheet.Cell("OrderDate").GetValue<string>();
        if (!DateTime.TryParse(dateStr, out DateTime orderdate)) orderdate = DateTime.Now;

        string customerName = sheet.Cell("CustomerName").GetValue<string>();
        string customerLine1 = sheet.Cell("CustomerLine1").GetValue<string>();
        string customerLine2 = sheet.Cell("CustomerLine2").GetValue<string>();
        string customerLine3 = sheet.Cell("CustomerLine3").GetValue<string>();
        string customerCity = sheet.Cell("CustomerCity").GetValue<string>();
        string customerState = sheet.Cell("CustomerState").GetValue<string>();
        string customerZip = sheet.Cell("CustomerZip").GetValue<string>();
        string customerCountry = sheet.Cell("CustomerCountry").GetValue<string>();

        var qtyStart = sheet.Cell("QtyStart");
        var widthStart = sheet.Cell("WidthStart");
        var heightStart = sheet.Cell("HeightStart");
        var depthStart = sheet.Cell("DepthStart");

        List<DrawerBoxData> boxes = new();

        int offset = 1;
        while (true) {

            var qtyStr = qtyStart.GetOffsetCell(offset).GetValue<string>();

            if (string.IsNullOrWhiteSpace(qtyStr) || !int.TryParse(qtyStr, out int qty)) break;

            double width = widthStart.GetOffsetCell(offset).GetValue<double>();
            double height = heightStart.GetOffsetCell(offset).GetValue<double>();
            double depth = depthStart.GetOffsetCell(offset).GetValue<double>();

            var heightDim = Dimension.FromInches(height);
            var widthDim = Dimension.FromInches(width);
            var depthDim = Dimension.FromInches(depth);
            boxes.Add(new() {
                Line = offset,
                UnitPrice = 0M,
                Qty = qty,
                Height = heightDim,
                Width = widthDim,
                Depth = depthDim,
                Logo = false,
                PostFinish = false,
                ScoopFront = false,
                FaceMountingHoles = false,
                UBox = false,
                FixedDividers = false,
                BoxMaterialOptionId = Guid.NewGuid(),
                BottomMaterialOptionId = Guid.NewGuid(),
                Clips = "No Clips",
                Notch = "No Notch",
                Accessory = "No Accessories"
            });

            offset++;

        }

        // TODO: get id from configuration file
        Guid vendorId = Guid.Parse("a81d759d-5b6c-4053-8cec-55a6c94d609e");
        bool didError = false;
        Company? customer = null;
        var response = await _bus.Send(new GetCompanyByName.Query(customerName));

        response.Match(
            c => {
                customer = c;
            },
            error => {
                // TODO: log error
                didError = true;
            }
        );

        if (customer is null) {
            customer = await CreateCustomer(customerName, customerLine1, customerLine2, customerLine3, customerCity, customerState, customerZip, customerCountry);
            if (customer is null) return null; //new(new Error() { Message = "Could not find/create customer" });
        }

        if (customer is null || didError) return null; //new(new Error() { Message = "Could not find/create customer" });

        decimal tax = 0M;
        decimal shipping = 0M;
        decimal priceAdj = 0M;
        string orderComment = "";
        Dictionary<string, string> info = new();
        var items = new List<AdditionalItemData>();

        return new OrderData() {
            Number = number,
            Name = name,
            Comment = orderComment,
            Tax = tax,
            Shipping = shipping,
            PriceAdjustment = priceAdj,
            OrderDate = orderdate,
            CustomerId = customer.Id,
            VendorId = vendorId,
            Info = info,
            AdditionalItems = items,
            Boxes = boxes
        };

    }

    public override Task<ValidationResult> ValidateSource(string source) {

        if (!File.Exists(source)) 
            return Task.FromResult(new ValidationResult() {
                    IsValid = false,
                    ErrorMessage = $"Given file does not exist\n'{source}'"
                });

        var extension = Path.GetExtension(source);
        if (extension != ".xlsx" && extension != ".xlsm")
            return Task.FromResult(new ValidationResult() {
                IsValid = false,
                ErrorMessage = $"Invalid file type '{extension}'"
            });

        return Task.FromResult(new ValidationResult() { IsValid = true });

    }

    private async Task<Company?> CreateCustomer(string name, string line1, string line2, string line3, string city, string state, string zip, string country) {
        var adderes = new Address() {
            Line1 = line1,
            Line2 = line2,
            Line3 = line3,
            City = city,
            State = state,
            Zip = zip,
            Country = country
        };

        // TODO: get customer contact info from allmoxy order data
        Response<Company> createResponse = await _bus.Send(new CreateCompany.Command(name, adderes, "", "", ""));

        Company? customer = null;
        createResponse.Match(
            c => {
                customer = c;
            },
            error => {
                // TODO: log error
                customer = null;
            }
        );

        return customer;

    }

}
