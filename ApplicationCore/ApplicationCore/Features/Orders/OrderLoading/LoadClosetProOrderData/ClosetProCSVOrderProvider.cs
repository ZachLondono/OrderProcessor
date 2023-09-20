using ApplicationCore.Features.Companies.Contracts.Entities;
using ApplicationCore.Features.Companies.Contracts.ValueObjects;
using ApplicationCore.Features.Orders.OrderLoading.Dialog;
using ApplicationCore.Features.Orders.OrderLoading.LoadClosetProOrderData.Models;
using ApplicationCore.Features.Orders.OrderLoading.Models;
using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Shared.Data.Ordering;
using ApplicationCore.Shared.Domain;
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
    private readonly GetCustomerWorkingDirectoryRootByIdAsync _getCustomerWorkingDirectoryRootByIdAsync;
    private readonly InsertCustomerAsync _insertCustomerAsync;
    private readonly GetCustomerByIdAsync _getCustomerByIdAsync;

    public ClosetProCSVOrderProvider(ILogger<ClosetProCSVOrderProvider> logger, ClosetProCSVReader reader, ClosetProPartMapper partMapper, IFileReader fileReader, IOrderingDbConnectionFactory dbConnectionFactory, GetCustomerIdByNameAsync getCustomerIdByNameIdAsync, InsertCustomerAsync insertCustomerAsync, GetCustomerOrderPrefixByIdAsync getCustomerOrderPrefixByIdAsync, GetCustomerByIdAsync getCustomerByIdAsync, GetCustomerWorkingDirectoryRootByIdAsync getCustomerWorkingDirectoryRootByIdAsync) {
        _logger = logger;
        _reader = reader;
        _partMapper = partMapper;
        _fileReader = fileReader;
        _dbConnectionFactory = dbConnectionFactory;
        _getCustomerIdByNameAsync = getCustomerIdByNameIdAsync;
        _insertCustomerAsync = insertCustomerAsync;
        _getCustomerOrderPrefixByIdAsync = getCustomerOrderPrefixByIdAsync;
        _getCustomerByIdAsync = getCustomerByIdAsync;
        _getCustomerWorkingDirectoryRootByIdAsync = getCustomerWorkingDirectoryRootByIdAsync;
    }

    protected abstract Task<string?> GetCSVDataFromSourceAsync(string source);

    public record FrontHardware(string Name, Dimension Spread);

    public async Task<OrderData?> LoadOrderData(string data) {

        var parts = data.Split('*');

        if (parts.Length != 3) {
            throw new InvalidOperationException("Invalid data source");
        }

        string source = parts[0];
        string? customOrderNumber = string.IsNullOrWhiteSpace(parts[1]) ? null : parts[1];
        string? customWorkingDirectoryRoot = string.IsNullOrWhiteSpace(parts[2]) ? null : parts[2];

        var csvData = await GetCSVDataFromSourceAsync(source);

        if (csvData is null) {
            OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, "No order data found");
            return null;
        }

        _reader.OnReadError += (msg) => OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, msg);
        var info = await _reader.ReadCSVData(csvData);

        // TODO: get this info from a configuration file
        var vendorId = Guid.Parse("a81d759d-5b6c-4053-8cec-55a6c94d609e");
        string designerName = info.Header.GetDesignerName();
        var customer = await GetOrCreateCustomer(info.Header.DesignerCompany, designerName);

        _partMapper.Settings = customer.ClosetProSettings;

        List<AdditionalItem> additionalItems = new();
        additionalItems.AddRange(_partMapper.MapPickListToItems(info.PickList, out var hardwareSpread));
        _partMapper.HardwareSpread = hardwareSpread;
        additionalItems.AddRange(ClosetProPartMapper.MapAccessoriesToItems(info.Accessories));
        additionalItems.AddRange(ClosetProPartMapper.MapBuyOutPartsToItems(info.BuyOutParts));
        List<IProduct> products = _partMapper.MapPartsToProducts(info.Parts);

        string orderNumber;
        if (customOrderNumber is null && string.IsNullOrWhiteSpace(customOrderNumber)) {
            orderNumber = await GetNextOrderNumber(customer.Id);
            var orderNumberPrefix = await _getCustomerOrderPrefixByIdAsync(customer.Id) ?? "";
            orderNumber = $"{orderNumberPrefix}{orderNumber}";
        } else {
            orderNumber = customOrderNumber;
        }

        string? workingDirectoryRoot;
        if (customWorkingDirectoryRoot is null) {
            workingDirectoryRoot = await _getCustomerWorkingDirectoryRootByIdAsync(customer.Id);
        } else {
            workingDirectoryRoot = customWorkingDirectoryRoot;
        }
        string workingDirectory = CreateWorkingDirectory(source, info, orderNumber, workingDirectoryRoot);

        return new OrderData() {
            VendorId = vendorId,
            CustomerId = customer.Id,
            Name = info.Header.OrderName,
            Number = orderNumber,
            WorkingDirectory = workingDirectory,
            Products = products,
            AdditionalItems = additionalItems,

            OrderDate = DateTime.Today,
            DueDate = null,
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

    private string CreateWorkingDirectory(string source, ClosetProOrderInfo info, string orderNumber, string? customerWorkingDirectoryRoot) {
        string cpDefaultWorkingDirectory = @"R:\Job Scans\ClosetProSoftware"; // TODO: Get base directory from configuration file
        string workingDirectory = Path.Combine((customerWorkingDirectoryRoot ?? cpDefaultWorkingDirectory), _fileReader.RemoveInvalidPathCharacters($"{orderNumber} - {info.Header.DesignerCompany} - {info.Header.OrderName}", ' '));
        if (TryToCreateWorkingDirectory(workingDirectory, out string? incomingDir) && incomingDir is not null) {
            string dataFile = _fileReader.GetAvailableFileName(incomingDir, "Incoming", ".csv");
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

    private async Task<Customer> GetOrCreateCustomer(string designerCompanyName, string designerName) {

        Guid? customerId = await _getCustomerIdByNameAsync(designerCompanyName);

        if (customerId is Guid id) {

            var customer = await _getCustomerByIdAsync(id);
            if (customer is null) {
                throw new InvalidOperationException("Unable to load customer information");
            }
            return customer;

        } else {

            var contact = new Contact() {
                Name = designerName,
                Email = null,
                Phone = null
            };

            var newCustomer = CompanyCustomer.Create(designerCompanyName, "Pick Up", contact, new(), contact, new());

            await _insertCustomerAsync(newCustomer);

            return newCustomer;

        }

    }

    private bool TryToCreateWorkingDirectory(string workingDirectory, out string? incomingDirectory) {

        workingDirectory = workingDirectory.Trim();

        try {

            if (Directory.Exists(workingDirectory)) {
                incomingDirectory = CreateSubDirectories(workingDirectory);
                return true;
            } else if (Directory.CreateDirectory(workingDirectory).Exists) {
                incomingDirectory = CreateSubDirectories(workingDirectory);
                return true;
            } else {
                incomingDirectory = null;
                return false;
            }

        } catch (Exception ex) {
            incomingDirectory = null;
            OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Warning, $"Could not create working directory {workingDirectory} - {ex.Message}");
        }

        return false;

    }

    private static string? CreateSubDirectories(string workingDirectory) {
        var cutListDir = Path.Combine(workingDirectory, "CUTLIST");
        _ = Directory.CreateDirectory(cutListDir);

        var ordersDir = Path.Combine(workingDirectory, "orders");
        _ = Directory.CreateDirectory(ordersDir);

        var incomingDir = Path.Combine(workingDirectory, "incoming");
        return Directory.CreateDirectory(incomingDir).Exists ? incomingDir : null;
    }

    public static bool TryParseMoneyString(string text, out decimal value) {
        return decimal.TryParse(text.Replace("$", ""), out value);
    }

}
