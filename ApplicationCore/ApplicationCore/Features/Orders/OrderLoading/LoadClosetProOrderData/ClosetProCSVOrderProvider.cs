using ApplicationCore.Features.ClosetProCSVCutList;
using ApplicationCore.Features.ClosetProCSVCutList.CSVModels;
using ApplicationCore.Features.ClosetProCSVCutList.Products;
using Domain.Companies.Entities;
using Domain.Companies.ValueObjects;
using ApplicationCore.Features.Orders.OrderLoading.Dialog;
using ApplicationCore.Features.Orders.OrderLoading.Models;
using Domain.Orders.Builders;
using Domain.Orders.Entities;
using ApplicationCore.Shared.Data.Ordering;
using Domain.ValueObjects;
using ApplicationCore.Shared.Services;
using Dapper;
using Microsoft.Extensions.Logging;
using static Domain.Companies.CompanyDirectory;
using CompanyCustomer = Domain.Companies.Entities.Customer;
using Domain.Orders.Entities.Products;

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
    private readonly ComponentBuilderFactory _componentBuilderFactory;

    public ClosetProCSVOrderProvider(ILogger<ClosetProCSVOrderProvider> logger, ClosetProCSVReader reader, ClosetProPartMapper partMapper, IFileReader fileReader, IOrderingDbConnectionFactory dbConnectionFactory, GetCustomerIdByNameAsync getCustomerIdByNameIdAsync, InsertCustomerAsync insertCustomerAsync, GetCustomerOrderPrefixByIdAsync getCustomerOrderPrefixByIdAsync, GetCustomerByIdAsync getCustomerByIdAsync, GetCustomerWorkingDirectoryRootByIdAsync getCustomerWorkingDirectoryRootByIdAsync, ComponentBuilderFactory componentBuilderFactory) {
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
        _componentBuilderFactory = componentBuilderFactory;
    }

    protected abstract Task<string?> GetCSVDataFromSourceAsync(string source);

    public record FrontHardware(string Name, Dimension Spread);

    public async Task<OrderData?> LoadOrderData(string sourceObj) {

        var sourceObjParts = sourceObj.Split('*');

        if (sourceObjParts.Length != 3) {
            throw new InvalidOperationException("Invalid data source");
        }

        string source = sourceObjParts[0];
        string? customOrderNumber = string.IsNullOrWhiteSpace(sourceObjParts[1]) ? null : sourceObjParts[1];
        string? customWorkingDirectoryRoot = string.IsNullOrWhiteSpace(sourceObjParts[2]) ? null : sourceObjParts[2];

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

        List<OtherPart> otherParts = [];
        otherParts.AddRange(ClosetProPartMapper.MapPickListToItems(info.PickList, [], out var hardwareSpread));
        otherParts.AddRange(ClosetProPartMapper.MapAccessoriesToItems(info.Accessories));
        otherParts.AddRange(ClosetProPartMapper.MapBuyOutPartsToItems(info.BuyOutParts));
        var additionalItems = otherParts.Select(p => new AdditionalItem(Guid.NewGuid(), p.Qty, $"{p.Name}", p.UnitPrice)).ToList();

        _partMapper.GroupLikeProducts = true; // TODO: Move this into the closet pro settings object
        var products = _partMapper.MapPartsToProducts(info.Parts, hardwareSpread)
                                    .Select(p => CreateProductFromClosetProProduct(p, customer.ClosetProSettings, _componentBuilderFactory))
                                    .ToList();

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
        string workingDirectory = await CreateWorkingDirectory(csvData, info, orderNumber, workingDirectoryRoot);

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

    private static IProduct CreateProductFromClosetProProduct(IClosetProProduct product, ClosetProSettings settings, ComponentBuilderFactory factory) {

        if (product is CornerShelf cornerShelf) {

            return cornerShelf.ToProduct(settings);

        } else if (product is DrawerBox db) {

            return db.ToProduct(factory, settings);

        } else if (product is FivePieceFront fivePieceFront) {

            return fivePieceFront.ToProduct();

        } else if (product is HutchVerticalPanel hutch) {

            return hutch.ToProduct(settings.VerticalPanelBottomRadius);

        } else if (product is IslandVerticalPanel island) {

            return island.ToProduct();

        } else if (product is MDFFront mdfFront) {

            return mdfFront.ToProduct();

        } else if (product is MelamineSlabFront melaSlab) {

            return melaSlab.ToProduct();

        } else if (product is MiscellaneousClosetPart misc) {

            return misc.ToProduct(settings);

        } else if (product is Shelf shelf) {

            return shelf.ToProduct(settings);

        } else if (product is TransitionVerticalPanel transition) {

            return transition.ToProduct(settings.VerticalPanelBottomRadius);

        } else if (product is VerticalPanel vertical) {

            return vertical.ToProduct(settings.VerticalPanelBottomRadius);

        } else if (product is ZargenDrawerBox zargen) {

            return zargen.ToProduct();

        } else if (product is DividerShelf dividerShelf) {

            return dividerShelf.ToProduct();

        } else if (product is DividerVerticalPanel dividerPanel) {

            return dividerPanel.ToProduct();

        } else {

            throw new InvalidOperationException("Unexpected product");

        }

    }

    private async Task<string> CreateWorkingDirectory(string csvData, ClosetProOrderInfo info, string orderNumber, string? customerWorkingDirectoryRoot) {
        string cpDefaultWorkingDirectory = @"R:\Job Scans\ClosetProSoftware"; // TODO: Get base directory from configuration file
        string workingDirectory = Path.Combine((customerWorkingDirectoryRoot ?? cpDefaultWorkingDirectory), _fileReader.RemoveInvalidPathCharacters($"{orderNumber} - {info.Header.DesignerCompany} - {info.Header.OrderName}", ' '));
        if (TryToCreateWorkingDirectory(workingDirectory, out string? incomingDir) && incomingDir is not null) {
            string dataFile = _fileReader.GetAvailableFileName(incomingDir, "Incoming", ".csv");
            await File.WriteAllTextAsync(dataFile, csvData);
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
