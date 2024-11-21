using OrderLoading.ClosetProCSVCutList;
using Domain.Companies.ValueObjects;
using Domain.Orders.Entities;
using Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Domain.Orders.Persistance;
using Domain.Services;
using Domain.Orders.ValueObjects;
using Domain.Orders.Entities.Hardware;
using OrderLoading.ClosetProCSVCutList.PartList;
using OrderLoading.ClosetProCSVCutList.PickList;
using OrderLoading.ClosetProCSVCutList.Header;
using Domain.Services.WorkingDirectory;

namespace OrderLoading.LoadClosetProOrderData;

public abstract class ClosetProCSVOrderProvider : IOrderProvider {

	public IOrderLoadWidgetViewModel? OrderLoadingViewModel { get; set; }

	private readonly ClosetProCSVReader _reader;
	private readonly PartListProcessor _partListProcessor;
    private readonly OrderHeaderProcessor _orderHeaderProcessor;

	public ClosetProCSVOrderProvider(ClosetProCSVReader reader, PartListProcessor partListProcessor, OrderHeaderProcessor orderHeaderProcessor) {
		_reader = reader;
		_partListProcessor = partListProcessor;
        _orderHeaderProcessor = orderHeaderProcessor;
	}

	protected abstract Task<string?> GetCSVDataFromSourceAsync(string source);

	public async Task<OrderData?> LoadOrderData(string sourceObj) {

        OrderLoadingSettings settings = GetOrderLoadingSettings(sourceObj);

        var csvData = await GetCSVDataFromSourceAsync(settings.FilePath);

        if (csvData is null) {
            OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, "No order data found");
            return null;
        }

        _reader.OnReadError += (msg) => OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, msg);
        var info = await _reader.ReadCSVData(csvData);

        // TODO: get this info from a configuration file
        var vendorId = Guid.Parse("a81d759d-5b6c-4053-8cec-55a6c94d609e");

        var header = await _orderHeaderProcessor.ParseOrderHeader(info.Header, settings);
        var pickList = PickListProcessor.ParsePickList(info.PickList);
        var partList = _partListProcessor.ParsePartList(header.Customer.ClosetProSettings, info.Parts, pickList.HardwareSpread);

        var workingDirectory = WorkingDirectoryStructure.Create(header.WorkingDirectory, true);
        await workingDirectory.WriteAllTextToIncomingAsync("Closet Pro Cut List.csv", csvData, false);

        IEnumerable<Supply> supplies = [
            .. pickList.Supplies,
            .. partList.Supplies
        ];

        var suppliesArray = supplies.Where(s => s.Qty != 0).ToArray();
        Hardware hardware = new(suppliesArray, partList.DrawerSlides, partList.HangingRails);

        List<AdditionalItem> additionalItems = [];

        return new OrderData() {
            VendorId = vendorId,
            CustomerId = header.Customer.Id,
            Name = header.OrderName,
            Number = header.OrderNumber,
            WorkingDirectory = header.WorkingDirectory,
            Products = partList.Products.ToList(),
            AdditionalItems = additionalItems,

            OrderDate = DateTime.Today,
            DueDate = null,
            Rush = false,
            Info = [],
            Comment = string.Empty,
            PriceAdjustment = 0M,
            Tax = 0M,
            Billing = new() {
                InvoiceEmail = null,
                PhoneNumber = "",
                Address = new()
            },
            Shipping = new() {
                Contact = "",
                Address = new(),
                Method = "Pick Up",
                PhoneNumber = "",
                Price = 0M
            },
            Hardware = hardware
        };

    }

    private static OrderLoadingSettings GetOrderLoadingSettings(string sourceObj) {
        var sourceObjParts = sourceObj.Split('*');

        if (sourceObjParts.Length != 3) {
            throw new InvalidOperationException("Invalid data source");
        }

        string source = sourceObjParts[0];
        string? customOrderNumber = string.IsNullOrWhiteSpace(sourceObjParts[1]) ? null : sourceObjParts[1];
        string? customWorkingDirectoryRoot = string.IsNullOrWhiteSpace(sourceObjParts[2]) ? null : sourceObjParts[2];
        var settings = new OrderLoadingSettings(source, customOrderNumber, customWorkingDirectoryRoot);
        return settings;
    }

	public static bool TryParseMoneyString(string text, out decimal value) {
		return decimal.TryParse(text.Replace("$", ""), out value);
	}

}