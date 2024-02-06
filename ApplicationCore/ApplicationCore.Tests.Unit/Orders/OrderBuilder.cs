using Domain.Orders.Entities;
using Domain.Orders.ValueObjects;
using Domain.Orders.Entities.Products;

namespace ApplicationCore.Tests.Unit.Orders;

internal class OrderBuilder {

    public Guid Id { get; set; } = Guid.NewGuid();
    public string Source { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public string WorkingDirectory { get; set; } = string.Empty;
    public Guid CustomerId { get; set; } = Guid.NewGuid();
    public Guid VendorId { get; set; } = Guid.NewGuid();
    public string Comment { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; } = DateTime.Today;
    public DateTime? DueDate { get; set; } = null;
    public decimal Tax { get; set; } = decimal.Zero;
    public ShippingInfo Shipping { get; set; }
    public BillingInfo Billing { get; set; }
    public decimal PriceAdjustment { get; set; } = decimal.Zero;
    public bool Rush { get; set; } = false;
    public Dictionary<string, string> Info { get; set; } = new();
    public List<AdditionalItem> Items { get; set; } = new();
    public List<IProduct> Products { get; set; } = new();

    public OrderBuilder() {

        Shipping = new() {
            Contact = "",
            Method = "",
            PhoneNumber = "",
            Price = 0M,
            Address = new()
        };

        Billing = new() {
            InvoiceEmail = null,
            PhoneNumber = "",
            Address = new(),
        };

    }

    public OrderBuilder WithId(Guid id) {
        Id = id;
        return this;
    }

    public OrderBuilder WithSource(string source) {
        Source = source;
        return this;
    }

    public OrderBuilder WithNumber(string number) {
        Number = number;
        return this;
    }

    public OrderBuilder WithName(string name) {
        Name = name;
        return this;
    }

    public OrderBuilder WithCustomerId(Guid customerId) {
        CustomerId = customerId;
        return this;
    }

    public OrderBuilder WithVendorId(Guid vendorId) {
        VendorId = vendorId;
        return this;
    }

    public OrderBuilder WithComment(string comment) {
        Comment = comment;
        return this;
    }

    public OrderBuilder WithOrderDate(DateTime orderDate) {
        OrderDate = orderDate;
        return this;
    }

    public OrderBuilder WithDueDate(DateTime? dueDate) {
        DueDate = dueDate;
        return this;
    }

    public OrderBuilder WithTax(decimal tax) {
        Tax = tax;
        return this;
    }

    public OrderBuilder WithShipping(ShippingInfo shipping) {
        Shipping = shipping;
        return this;
    }

    public OrderBuilder WithBilling(BillingInfo billing) {
        Billing = billing;
        return this;
    }

    public OrderBuilder WithPriceAdjustment(decimal priceAdjustment) {
        PriceAdjustment = priceAdjustment;
        return this;
    }

    public OrderBuilder WithInfo(Dictionary<string, string> info) {
        Info = info;
        return this;
    }

    public OrderBuilder WithProducts(List<IProduct> products) {
        Products = products;
        return this;
    }

    public OrderBuilder WithItems(List<AdditionalItem> items) {
        Items = items;
        return this;
    }

    public OrderBuilder WithRush(bool rush) {
        Rush = rush;
        return this;
    }

    public Order Build() => new(Id, Source, Number, Name, Note, WorkingDirectory, CustomerId, VendorId, Comment, OrderDate, DueDate, Shipping, Billing, Tax, PriceAdjustment, Rush, Info, Products, Items);

}
