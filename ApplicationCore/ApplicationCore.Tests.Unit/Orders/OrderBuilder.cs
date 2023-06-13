using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;

namespace ApplicationCore.Tests.Unit.Orders;

internal class OrderBuilder {

    private Guid _id = Guid.NewGuid();
    private string _source = string.Empty;
    private string _number = string.Empty;
    private string _name = string.Empty;
    private string _note = string.Empty;
    private string _workingDirectory = string.Empty;
    private Guid _customerId = Guid.NewGuid();
    private Guid _vendorId = Guid.NewGuid();
    private string _comment = string.Empty;
    private DateTime _orderDate = DateTime.Today;
    private decimal _tax = decimal.Zero;
    private ShippingInfo _shipping;
    private BillingInfo _billing;
    private decimal _priceAdjustment = decimal.Zero;
    private bool _rush = false;
    private Dictionary<string, string> _info = new();
    private List<AdditionalItem> _items = new();
    private List<IProduct> _products = new();

    public OrderBuilder() {

        _shipping = new() {
            Contact = "",
            Method = "",
            PhoneNumber = "",
            Price = 0M,
            Address = new()
        };

        _billing = new() {
            InvoiceEmail = null,
            PhoneNumber = "",
            Address = new(),
        };

    }

    public OrderBuilder WithId(Guid id) {
        _id = id;
        return this;
    }

    public OrderBuilder WithSource(string source) {
        _source = source;
        return this;
    }

    public OrderBuilder WithNumber(string number) {
        _number = number;
        return this;
    }

    public OrderBuilder WithName(string name) {
        _name = name;
        return this;
    }

    public OrderBuilder WithCustomerId(Guid customerId) {
        _customerId = customerId;
        return this;
    }

    public OrderBuilder WithVendorId(Guid vendorId) {
        _vendorId = vendorId;
        return this;
    }

    public OrderBuilder WithComment(string comment) {
        _comment = comment;
        return this;
    }

    public OrderBuilder WithOrderDate(DateTime orderDate) {
        _orderDate = orderDate;
        return this;
    }

    public OrderBuilder WithTax(decimal tax) {
        _tax = tax;
        return this;
    }

    public OrderBuilder WithShipping(ShippingInfo shipping) {
        _shipping = shipping;
        return this;
    }

    public OrderBuilder WithBilling(BillingInfo billing) {
        _billing = billing;
        return this;
    }

    public OrderBuilder WithPriceAdjustment(decimal priceAdjustment) {
        _priceAdjustment = priceAdjustment;
        return this;
    }

    public OrderBuilder WithInfo(Dictionary<string, string> info) {
        _info = info;
        return this;
    }

    public OrderBuilder WithProducts(List<IProduct> products) {
        _products = products;
        return this;
    }

    public OrderBuilder WithItems(List<AdditionalItem> items) {
        _items = items;
        return this;
    }

    public OrderBuilder WithRush(bool rush) {
        _rush = rush;
        return this;
    }

    public Order Build() => new(_id, _source, _number, _name, _note, _workingDirectory, _customerId, _vendorId, _comment, _orderDate, _shipping, _billing, _tax, _priceAdjustment, _rush, _info, _products, _items);

}
