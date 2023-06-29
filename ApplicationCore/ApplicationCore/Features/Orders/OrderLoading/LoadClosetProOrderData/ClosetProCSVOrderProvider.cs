using ApplicationCore.Features.Companies.Contracts.ValueObjects;
using ApplicationCore.Features.Orders.OrderLoading.Dialog;
using ApplicationCore.Features.Orders.OrderLoading.LoadClosetProOrderData.Models;
using ApplicationCore.Features.Orders.OrderLoading.Models;
using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Shared.Data.Ordering;
using ApplicationCore.Shared.Services;
using Dapper;
using Microsoft.Extensions.Logging;
using static ApplicationCore.Features.Companies.Contracts.CompanyDirectory;
using CompanyAddress = ApplicationCore.Features.Companies.Contracts.ValueObjects.Address;
using CompanyCustomer = ApplicationCore.Features.Companies.Contracts.Entities.Customer;

namespace ApplicationCore.Features.Orders.OrderLoading.LoadClosetProOrderData;

internal class ClosetProCSVOrderProvider : IOrderProvider {

    public IOrderLoadWidgetViewModel? OrderLoadingViewModel { get; set; }

    private readonly ILogger<ClosetProCSVOrderProvider> _logger;
    private readonly ClosetProCSVReader _reader;
    private readonly ClosetProPartMapper _partMapper;
    private readonly IFileReader _fileReader;
    private readonly IOrderingDbConnectionFactory _dbConnectionFactory;
    private readonly GetCustomerIdByNameAsync _getCustomerIdByNameIdAsync;
    private readonly InsertCustomerAsync _insertCustomerAsync;

    public ClosetProCSVOrderProvider(ILogger<ClosetProCSVOrderProvider> logger, ClosetProCSVReader reader, ClosetProPartMapper partMapper, IFileReader fileReader, IOrderingDbConnectionFactory dbConnectionFactory, GetCustomerIdByNameAsync getCustomerIdByNameIdAsync, InsertCustomerAsync insertCustomerAsync) {
        _logger = logger;
        _reader = reader;
        _partMapper = partMapper;
        _fileReader = fileReader;
        _dbConnectionFactory = dbConnectionFactory;
        _getCustomerIdByNameIdAsync = getCustomerIdByNameIdAsync;
        _insertCustomerAsync = insertCustomerAsync;
    }

    public async Task<OrderData?> LoadOrderData(string source) {

        if (!File.Exists(source)) {
            OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, "File cannot be found");
            return null;
        }

        _reader.OnReadError += (msg) => OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, msg);

        var info = await _reader.ReadCSVFile(source);

        List<IProduct> products = new();
        foreach (var part in info.Parts) {
            var product = _partMapper.CreateProductFromPart(part);
            if (product is not null) {
                products.Add(product);
            } else {
                OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Warning, $"Skipping part {part.PartNum} - {part.PartName} / {part.ExportName}");
                _logger.LogWarning("Skipping part {Part}", part);
            }
        }

        List<AdditionalItem> additionalItems = new();
        foreach (var item in info.PickList) {

            if (!TryParseMoneyString(item.Cost, out var cost)) {
                OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Warning, $"Could not parse item cost '{item.Cost}'");
                cost = 0;
            }

            additionalItems.Add(new(Guid.NewGuid(), $"({item.Quantity}) {item.Name}", cost));

        }

        foreach (var item in info.Accessories) {

            if (!TryParseMoneyString(item.Cost, out var cost)) {
                OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Warning, $"Could not parse item cost '{item.Cost}'");
                cost = 0;
            }

            additionalItems.Add(new(Guid.NewGuid(), $"({item.Quantity}) {item.Name}", cost));

        }

        var orderNumber = await GetNextOrderNumber();

        string workingDirectory = CreateWorkingDirectory(source, info, orderNumber);

        // TODO: get this info from a configuration file
        var vendorId = Guid.Parse("a81d759d-5b6c-4053-8cec-55a6c94d609e");
        string designerName = info.Header.GetDesignerName();
        var customerId = await GetOrCreateCustomerId(info.Header.DesignerCompany, designerName);

        return new OrderData() {
            VendorId = vendorId,
            CustomerId = customerId,
            Name = info.Header.OrderName,
            Number = orderNumber,
            WorkingDirectory = workingDirectory,
            Products = products,
            AdditionalItems = additionalItems,

            OrderDate = DateTime.Today,
            Rush = false,
            Info = new(),
            Comment = string.Empty,
            PriceAdjustment = 0M,
            Tax = 0M,
            Billing = new() {
                InvoiceEmail = null,
                PhoneNumber = "",
                Address = new()
            },
            Shipping = new() {
                Contact = designerName,
                Address = new(),
                Method = "Pick Up",
                PhoneNumber = "",
                Price = 0M
            }
        };

    }

    private string CreateWorkingDirectory(string source, ClosetProOrderInfo info, string orderNumber) {
        // TODO: get base directory from a configuration file
        string workingDirectory = Path.Combine(@"R:\Job Scans\ClosetProSoftware", _fileReader.RemoveInvalidPathCharacters($"{orderNumber} - {info.Header.DesignerCompany} - {info.Header.OrderName}", ' '));
        bool workingDirExists = TryToCreateWorkingDirectory(workingDirectory);
        if (workingDirExists) {
            string dataFile = _fileReader.GetAvailableFileName(workingDirectory, "Incoming", ".csv");
            File.Copy(source, dataFile);
        }

        return workingDirectory;
    }

    private async Task<string> GetNextOrderNumber() {
        using var connection = await _dbConnectionFactory.CreateConnection();

        connection.Open();
        var trx = connection.BeginTransaction();

        try {

            string orderSourceName = "closet_pro_software";

            var newNumber = await connection.QuerySingleAsync<int>("SELECT number FROM order_numbers WHERE name = @OrderSourceName;", new {
                OrderSourceName = orderSourceName
            });

            await connection.ExecuteAsync("UPDATE order_numbers SET number = @IncrementedValue WHERE name = @OrderSourceName", new {
                OrderSourceName = orderSourceName,
                IncrementedValue = newNumber + 1
            });

            trx.Commit();

            return newNumber.ToString();

        } catch {
            trx.Rollback();
            throw;
        } finally {
            connection.Close();
        }

    }

    private async Task<Guid> GetOrCreateCustomerId(string designerCompanyName, string designerName) {
 
        Guid? customerId = await _getCustomerIdByNameIdAsync(designerCompanyName);

        if (customerId is Guid id) {
            return id;
        } else {

            var contact = new Contact() {
                Name = designerName,
                Email = null,
                Phone = null
            };

            var newCustomer = CompanyCustomer.Create(designerCompanyName, "Pick Up", contact, new(), contact, new());

            await _insertCustomerAsync(newCustomer);

            return newCustomer.Id;

        }

    }

    private bool TryToCreateWorkingDirectory(string workingDirectory) {

        if (Directory.Exists(workingDirectory)) {
            return true;
        }

        try {
            var dirInfo = Directory.CreateDirectory(workingDirectory);
            return dirInfo.Exists;
        } catch (Exception ex) {
            OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Warning, $"Could not create working directory {workingDirectory} - {ex.Message}");
        }

        return false;

    }

    public static bool TryParseMoneyString(string text, out decimal value) {
        return decimal.TryParse(text.Replace("$", ""), out value);
    }

}
