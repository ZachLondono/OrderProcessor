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
using CompanyCustomer = ApplicationCore.Features.Companies.Contracts.Entities.Customer;

namespace ApplicationCore.Features.Orders.OrderLoading.LoadClosetProOrderData;

internal abstract class ClosetProCSVOrderProvider : IOrderProvider {

    public IOrderLoadWidgetViewModel? OrderLoadingViewModel { get; set; }

    private readonly ILogger<ClosetProCSVOrderProvider> _logger;
    private readonly ClosetProCSVReader _reader;
    private readonly ClosetProPartMapper _partMapper;
    private readonly IFileReader _fileReader;
    private readonly IOrderingDbConnectionFactory _dbConnectionFactory;
    private readonly GetCustomerIdByNameAsync _getCustomerIdByNameAsync;
    private readonly GetCustomerOrderPrefixByIdAsync _getCustomerOrderPrefixByIdAsync;
    private readonly InsertCustomerAsync _insertCustomerAsync;

    public ClosetProCSVOrderProvider(ILogger<ClosetProCSVOrderProvider> logger, ClosetProCSVReader reader, ClosetProPartMapper partMapper, IFileReader fileReader, IOrderingDbConnectionFactory dbConnectionFactory, GetCustomerIdByNameAsync getCustomerIdByNameIdAsync, InsertCustomerAsync insertCustomerAsync, GetCustomerOrderPrefixByIdAsync getCustomerOrderPrefixByIdAsync) {
        _logger = logger;
        _reader = reader;
        _partMapper = partMapper;
        _fileReader = fileReader;
        _dbConnectionFactory = dbConnectionFactory;
        _getCustomerIdByNameAsync = getCustomerIdByNameIdAsync;
        _insertCustomerAsync = insertCustomerAsync;
        _getCustomerOrderPrefixByIdAsync = getCustomerOrderPrefixByIdAsync;
    }

    protected abstract Task<string?> GetCSVDataFromSourceAsync(string source);

    public async Task<OrderData?> LoadOrderData(string source) {

        var csvData = await GetCSVDataFromSourceAsync(source);

        if (csvData is null) {
            OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, "No order data found");
            return null;
        }

        _reader.OnReadError += (msg) => OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, msg);
        var info = await _reader.ReadCSVData(csvData);

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

        // TODO: get this info from a configuration file
        var vendorId = Guid.Parse("a81d759d-5b6c-4053-8cec-55a6c94d609e");
        string designerName = info.Header.GetDesignerName();
        var customerId = await GetOrCreateCustomerId(info.Header.DesignerCompany, designerName);

        var orderNumber = await GetNextOrderNumber(customerId);
        string workingDirectory = CreateWorkingDirectory(source, info, orderNumber);

        var orderNumberPrefix = await _getCustomerOrderPrefixByIdAsync(customerId) ?? throw new InvalidOperationException("Could not get customer data");
        orderNumber = $"{orderNumberPrefix}{orderNumber}";

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

    private async Task<string> GetNextOrderNumber(Guid customerId) {

        using var connection = await _dbConnectionFactory.CreateConnection();

        connection.Open();
        var trx = connection.BeginTransaction();

        try {

            var newNumber = await connection.QuerySingleOrDefaultAsync<int?>("SELECT number FROM order_numbers WHERE customer_id = @CustomerId;", new {
                CustomerId = customerId 
            });

            if (newNumber is null) {
                int initialNumber = 1;
                await connection.ExecuteAsync("INSERT INTO order_numbers (customer_id, number) VALUES (@CustomerId, @InitialNumber);", new {
                    CustomerId = customerId,
                    InitialNumber = initialNumber
                }, trx);
                newNumber = initialNumber;
            }

            await connection.ExecuteAsync("UPDATE order_numbers SET number = @IncrementedValue WHERE customer_id = @CustomerId", new {
                CustomerId = customerId,
                IncrementedValue = newNumber + 1
            });

            trx.Commit();

            return newNumber?.ToString() ?? "0";

        } catch {
            trx.Rollback();
            throw;
        } finally {
            connection.Close();
        }

    }

    private async Task<Guid> GetOrCreateCustomerId(string designerCompanyName, string designerName) {
 
        Guid? customerId = await _getCustomerIdByNameAsync(designerCompanyName);

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
