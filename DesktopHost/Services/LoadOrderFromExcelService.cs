using ApplicationCore.Features.Companies.Commands;
using ApplicationCore.Features.Companies.Domain;
using ApplicationCore.Features.Companies.Domain.ValueObjects;
using ApplicationCore.Features.Companies.Queries;
using ApplicationCore.Features.Orders.Commands;
using ApplicationCore.Features.Orders.Domain;
using ApplicationCore.Features.Orders.Domain.ValueObjects;
using ApplicationCore.Features.Orders.Queries;
using ApplicationCore.Infrastructure;
using ApplicationCore.Shared;
using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dimension = ApplicationCore.Features.Orders.Domain.ValueObjects.Dimension;
using Order = ApplicationCore.Features.Orders.Domain.Order;
using Path = System.IO.Path;

namespace DesktopHost.Services;

internal class LoadOrderFromExcelService {

    private readonly IBus _bus;
    private readonly IMessageBoxService _messageBoxService;

    public LoadOrderFromExcelService(IBus bus, IMessageBoxService messageBoxService) {
        _bus = bus;
        _messageBoxService = messageBoxService;
    }

    public Response<Order> LoadOrder(string filePath) {

        if (!File.Exists(filePath)) return new Response<Order>(new Error() { Message = "Given file does not exist" });

        var extension = Path.GetExtension(filePath);
        if (extension != ".xlsx" && extension != ".xlsm") return new Response<Order>(new Error() { Message = $"Invalid file type {extension}" });

        var existsResult = _bus.Send(new GetOrderIdWithSource.Query(filePath)).Result;
        Guid? existingOrderId = null;
        existsResult.Match(
            existingId => {

                if (existingId is null) return;

                var result = _messageBoxService.OpenDialogYesNo("An order from this source already exists, do you want to overwrite the existing order?", "Order Exists");
                if (result is YesNoResult.Yes) {
                    existingOrderId = existingId;
                }

            },
            error => {
                // TODO: log error
                _messageBoxService.OpenDialog("Error", $"Error checking if order exists\n{error.Message}");
            }
        );

        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var wb = new XLWorkbook(stream);

        var sheet = wb.Worksheet("Order");

        if (sheet is null) return new Response<Order>(new Error() { Message = "Order sheet could not be found" });

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

        List<DrawerBox> boxes = new();
        var option = new DrawerBoxOption(Guid.NewGuid(), "");
        var options = new DrawerBoxOptions(option, option, option, option, option);

        int offset = 1;
        while (true) {

            var qtyStr = GetOffsetCell(qtyStart, offset).GetValue<string>();

            if (string.IsNullOrWhiteSpace(qtyStr) || !int.TryParse(qtyStr, out int qty)) break;

            double width = GetOffsetCell(widthStart, offset).GetValue<double>();
            double height = GetOffsetCell(heightStart, offset).GetValue<double>();
            double depth = GetOffsetCell(depthStart, offset).GetValue<double>();

            var heightDim = Dimension.FromInches(height);
            var widthDim = Dimension.FromInches(width);
            var depthDim = Dimension.FromInches(depth);
            boxes.Add(DrawerBox.Create(offset, 0M, qty, heightDim, widthDim, depthDim, options));

            offset++;

        }

        // TODO: get id from configuration file
        Guid vendorId = Guid.Parse("a81d759d-5b6c-4053-8cec-55a6c94d609e");
        bool didError = false;
        Company? customer = null;
        var response = _bus.Send(new GetCompanyByName.Query(customerName)).Result;

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
            customer = CreateCustomer(customerName, customerLine1, customerLine2, customerLine3, customerCity, customerState, customerZip, customerCountry);
            if (customer is null) return new(new Error() { Message = "Could not find/create customer" });
        }

        if (customer is null || didError) return new(new Error() { Message = "Could not find/create customer" });

        decimal tax = 0M;
        decimal shipping = 0M;
        decimal priceAdj = 0M;
        string orderComment = "";
        Dictionary<string, string> info = new();
        var items = Enumerable.Empty<AdditionalItem>();

        Task<Response<Order>> createTask;
        if (existingOrderId is not null) {
            createTask = _bus.Send(new OverwriteExistingOrderWithId.Command((Guid) existingOrderId, filePath, number, name, customer.Id, vendorId, orderComment, orderdate, tax, shipping, priceAdj, info, boxes, items));
        } else { 
            createTask = _bus.Send(new CreateNewOrder.Command(filePath, number, name, customer.Id, vendorId, orderComment, orderdate, tax, shipping, priceAdj, info, boxes, items));
        }

        return createTask.Result;

    }

    private Company? CreateCustomer(string name, string line1, string line2, string line3, string city, string state, string zip, string country) {
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
        Task<Response<Company>> createTask = _bus.Send(new CreateCompany.Command(name, adderes, "", "", ""));
        createTask.Wait();
        Response<Company> createResponse = createTask.Result;

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

    public static IXLCell GetOffsetCell(IXLCell relative, int rowOffset = 0, int colOffset = 0) {
        return relative.Address.Worksheet.Cell(relative.Address.RowNumber + rowOffset, relative.Address.ColumnNumber + colOffset);
    }

}
