using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Orders.OrderLoading.Providers.AllmoxyXMLModels;
using System.Xml.Serialization;
using System.Text;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.OrderLoading.Dialog;
using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using System.Xml.Schema;
using static ApplicationCore.Features.Companies.Contracts.CompanyDirectory;
using Address = ApplicationCore.Features.Orders.Shared.Domain.ValueObjects.Address;
using CompanyAddress = ApplicationCore.Features.Companies.Contracts.ValueObjects.Address;
using CompanyCustomer = ApplicationCore.Features.Companies.Contracts.Entities.Customer;
using ApplicationCore.Features.Companies.Contracts.ValueObjects;
using System.Xml;
using ApplicationCore.Shared;
using Microsoft.Extensions.Options;
using ApplicationCore.Shared.Services;
using ApplicationCore.Features.Orders.OrderLoading.LoadAllmoxyOrderData.XMLValidation;
using ApplicationCore.Features.Orders.OrderLoading.Models;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Features.Orders.OrderLoading.LoadAllmoxyOrderData;

internal abstract class AllmoxyXMLOrderProvider : IOrderProvider {

    private readonly AllmoxyConfiguration _configuration;
    private readonly IXMLValidator _validator;
    private readonly ProductBuilderFactory _builderFactory;
    private readonly GetCustomerIdByAllmoxyIdAsync _getCustomerIdByAllmoxyIdAsync;
    private readonly InsertCustomerAsync _insertCustomerAsync;
    private readonly IFileReader _fileReader;
    private readonly GetCustomerOrderPrefixByIdAsync _getCustomerOrderPrefixByIdAsync;
    private readonly ILogger<AllmoxyXMLOrderProvider> _logger;

    public IOrderLoadWidgetViewModel? OrderLoadingViewModel { get; set; }

    public AllmoxyXMLOrderProvider(IOptions<AllmoxyConfiguration> configuration, IXMLValidator validator, ProductBuilderFactory builderFactory, GetCustomerIdByAllmoxyIdAsync getCustomerIdByAllmoxyIdAsync, InsertCustomerAsync insertCustomerAsync, IFileReader fileReader, GetCustomerOrderPrefixByIdAsync getCustomerOrderPrefixByIdAsync, ILogger<AllmoxyXMLOrderProvider> logger) {
        _configuration = configuration.Value;
        _validator = validator;
        _builderFactory = builderFactory;
        _getCustomerIdByAllmoxyIdAsync = getCustomerIdByAllmoxyIdAsync;
        _insertCustomerAsync = insertCustomerAsync;
        _fileReader = fileReader;
        _getCustomerOrderPrefixByIdAsync = getCustomerOrderPrefixByIdAsync;
        _logger = logger;
    }

    protected abstract Task<string> GetExportXMLFromSource(string source);

    public async Task<OrderData?> LoadOrderData(string source) {

        var exportXML = await GetExportXMLFromSource(source);

        if (exportXML == string.Empty) {
            OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, "No order data found");
            return null;
        }

        // Validate data
        if (!ValidateData(exportXML)) {
            OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, "Order data was not valid");
            return null;
        }

        // Deserialize data
        OrderModel? data = DeserializeData(exportXML);
        if (data is null) {
            OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, "Could not find order information in given data");
            return null;
        }

        // TODO: Get base directory from configuration file
        string workingDirectory = Path.Combine(@"R:\Job Scans\Allmoxy", _fileReader.RemoveInvalidPathCharacters($"{data.Number} - {data.Customer.Company} - {data.Name}", ' '));
        bool workingDirExists = TryToCreateWorkingDirectory(workingDirectory);

        if (workingDirExists) {
            string dataFile = _fileReader.GetAvailableFileName(workingDirectory, "Incoming", ".xml");
            await File.WriteAllTextAsync(dataFile, exportXML);
        }

        ShippingInfo shipping = new() {
            Contact = data.Shipping.Attn,
            Method = data.Shipping.Method,
            PhoneNumber = "",
            Price = AllmoxyXMLOrderProviderHelpers.StringToMoney(data.Invoice.Shipping),
            Address = new Address() {
                Line1 = data.Shipping.Address.Line1,
                Line2 = data.Shipping.Address.Line2,
                Line3 = data.Shipping.Address.Line3,
                City = data.Shipping.Address.City,
                State = data.Shipping.Address.State,
                Zip = data.Shipping.Address.Zip,
                Country = data.Shipping.Address.Country
            }
        };

        string customerName = data.Customer.Company;

        Guid customerId = await CreateCustomerIfNotExists(data, customerName);

        var info = new Dictionary<string, string>() {
            { "Notes", data.Note },
            { "Shipping Attn", data.Shipping.Attn },
            { "Shipping Instructions", data.Shipping.Instructions },
            { "Allmoxy Customer Id", data.Customer.CompanyId.ToString() }
        };

        DateTime orderDate = ParseOrderDate(data.OrderDate);

        var billing = new BillingInfo() {
            InvoiceEmail = null,
            PhoneNumber = "",
            Address = new()
        };

        List<IProduct> products = new();
        data.Products.ForEach(c => MapAndAddProduct(c, products));

        string number = data.Number.ToString();
        string? prefix = await _getCustomerOrderPrefixByIdAsync(customerId);
        if (prefix is not null) {
            number = prefix + number;
        }

        OrderData? order = new() {
            Number = number,
            Name = data.Name,
            WorkingDirectory = workingDirectory,                                         // TODO: Get default working directory from configuration file
            Comment = data.Description,
            Shipping = shipping,
            Billing = billing,
            Tax = AllmoxyXMLOrderProviderHelpers.StringToMoney(data.Invoice.Tax),
            PriceAdjustment = 0M,
            OrderDate = orderDate,
            CustomerId = customerId,
            VendorId = Guid.Parse(_configuration.VendorId),
            AdditionalItems = new(),
            Products = products,
            Rush = data.Shipping.Method.Contains("Rush"),
            Info = info
        };

        return order;

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
            _logger.LogError(ex, "Exception thrown while trying to create working directory", workingDirectory);
        }

        return false;

    }

    private async Task<Guid> CreateCustomerIfNotExists(OrderModel data, string customerName) {

        int allmoxyCustomerId = data.Customer.CompanyId;
        Guid? customerId = await _getCustomerIdByAllmoxyIdAsync(allmoxyCustomerId);

        if (customerId is Guid id) {
            return id;
        } else {

            var shippingContact = new Contact() {
                Name = data.Shipping.Attn,
                Email = null,
                Phone = null
            };

            var shippingAddress = new CompanyAddress() {
                Line1 = data.Shipping.Address.Line1,
                Line2 = data.Shipping.Address.Line2,
                Line3 = data.Shipping.Address.Line3,
                City = data.Shipping.Address.City,
                State = data.Shipping.Address.State,
                Zip = data.Shipping.Address.Zip,
                Country = data.Shipping.Address.Country
            };

            var billingContact = new Contact();

            var billingAddress = new CompanyAddress();

            var newCustomer = CompanyCustomer.Create(customerName, data.Shipping.Method, shippingContact, shippingAddress, billingContact, billingAddress);

            await _insertCustomerAsync(newCustomer, allmoxyCustomerId);

            return newCustomer.Id;

        }

    }

    public bool ValidateData(string data) {

        try {

            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));

            var directory = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var fullPath = Path.Combine(directory, _configuration.SchemaFilePath);
            var errors = _validator.ValidateXML(stream, fullPath);

            errors.ForEach(error => OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, $"[XML Error] [{error.Severity}] {error.Exception.Message}"));

            return !errors.Any();

        } catch (XmlSchemaException ex) {

            OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, $"[XML Schema Error] XML schema is not valid L{ex.LineNumber} - {ex.Message}");
            _logger.LogError(ex, "Exception thrown while comparing Allmoxy XML order data against schema");
            return false;

        } catch (XmlException ex) {

            OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, $"[XML Schema Error] XML schema is not valid L{ex.LineNumber} - {ex.Message}");
            _logger.LogError(ex, "Exception thrown while comparing Allmoxy XML order data against schema");
            return false;

        } catch (Exception ex) {

            OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, "Could not verify order data against schema");
            _logger.LogError(ex, "Exception thrown while comparing Allmoxy XML order data against schema");
            return false;

        }

    }

    private DateTime ParseOrderDate(string orderDateStr) {

        if (DateTime.TryParse(orderDateStr, out DateTime orderDate)) {
            return orderDate;
        }

        OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Warning, $"Could not parse order date '{(orderDateStr == "" ? "[BLANK]" : orderDateStr)}'");

        return DateTime.Now;

    }

    private void MapAndAddProduct(ProductModel data, List<IProduct> products) {

        try {

            var product = data.CreateProduct(_builderFactory);
            products.Add(product);

        } catch (Exception ex) {

            OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, $"Could not load product {ex.Message}");
            _logger.LogError(ex, "Exception thrown while mapping order data to product", data);

        }

    }

    private static OrderModel? DeserializeData(string exportXML) {
        var serializer = new XmlSerializer(typeof(OrderModel));
        var reader = new StringReader(exportXML);
        if (serializer.Deserialize(reader) is OrderModel data) {
            return data;
        }
        return null;
    }

}
