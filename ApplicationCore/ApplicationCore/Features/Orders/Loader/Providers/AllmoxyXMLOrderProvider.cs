using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Orders.Loader.Providers.AllmoxyXMLModels;
using System.Xml.Serialization;
using ApplicationCore.Features.Orders.Loader.XMLValidation;
using System.Text;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using MoreLinq;
using ApplicationCore.Features.Orders.Loader.Dialog;
using ApplicationCore.Features.Orders.Shared.Domain.Builders;

namespace ApplicationCore.Features.Orders.Loader.Providers;

internal class AllmoxyXMLOrderProvider : IOrderProvider {

    private readonly AllmoxyConfiguration _configuration;
    private readonly AllmoxyClientFactory _clientfactory;
    private readonly IXMLValidator _validator;
    private readonly ProductBuilderFactory _builderFactory;

    public IOrderLoadingViewModel? OrderLoadingViewModel { get; set; }

    public AllmoxyXMLOrderProvider(AllmoxyConfiguration configuration, AllmoxyClientFactory clientfactory, IXMLValidator validator, ProductBuilderFactory builderFactory) {
        _configuration = configuration;
        _clientfactory = clientfactory;
        _validator = validator;
        _builderFactory = builderFactory;
    }

    public Task<OrderData?> LoadOrderData(string source) {

        // Load order to a string
        string exportXML;
        try {
            exportXML = _clientfactory.CreateClient().GetExport(source, 6);
        } catch (Exception ex) {
            OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, $"Could not load order data from Allmoxy: {ex.Message}");
            return Task.FromResult<OrderData?>(null); ;
        }

        // Validate data
        if (!ValidateData(exportXML)) {
            OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, "Order data was not valid");
            return Task.FromResult<OrderData?>(null);
        }

        // Deserialize data
        OrderModel? data = DeserializeData(exportXML);
        if (data is null) {
            OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, "Could not find order information in given data");
            return Task.FromResult<OrderData?>(null);
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

        Customer customer = new() {
            Name = data.Customer.Company
        };

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

        OrderData? order = new() {
            Number = data.Number.ToString(),
            Name = data.Name,
            Comment = data.Description,
            Shipping = shipping,
            Billing = billing,
            Tax = AllmoxyXMLOrderProviderHelpers.StringToMoney(data.Invoice.Tax),
            PriceAdjustment = 0M,
            OrderDate = orderDate,
            Customer = customer,
            VendorId = Guid.Parse(_configuration.VendorId),
            AdditionalItems = new(),
            Products = products,
            Rush = data.Shipping.Method.Contains("Rush"),
            Info = info
        };

        return Task.FromResult<OrderData?>(order);

    }

    public bool ValidateData(string data) {

        try {

            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));
            var errors = _validator.ValidateXML(stream, _configuration.SchemaFilePath);

            errors.ForEach(error => OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, $"[XML Error] [{error.Severity}] {error.Exception.Message}"));

            return !errors.Any();

        } catch {

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

        }

    }

    private static OrderModel? DeserializeData(string xmlString) {
        var serializer = new XmlSerializer(typeof(OrderModel));
        using var reader = new StringReader(xmlString);
        if (serializer.Deserialize(reader) is OrderModel data) {
            return data;
        }
        return null;
    }
}
