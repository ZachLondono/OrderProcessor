using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;

namespace ApplicationCore.Features.Orders.Shared.Domain.Entities;

public class Order {

    public Guid Id { get; }
    public string Source { get; }
    public string Number { get; }
    public string Name { get; }
    public Guid CustomerId { get; }
    public Guid VendorId { get; }
    public string CustomerComment { get; }
    public DateTime OrderDate { get; }
    public ShippingInfo Shipping { get; }
    public BillingInfo Billing { get; }
    public decimal Tax { get; }
    public decimal PriceAdjustment { get; }
    public bool Rush { get; }
    public IReadOnlyDictionary<string, string> Info { get; }
    public IEnumerable<IProduct> Products { get; }
    public IEnumerable<AdditionalItem> AdditionalItems { get; }

    public string Note { get; set; } = string.Empty;

    public decimal SubTotal {
        get => Products.Sum(b => b.Qty * b.UnitPrice) + AdditionalItems.Sum(i => i.Price);
    }

    public decimal AdjustedSubTotal {
        get => SubTotal + PriceAdjustment;
    }

    public decimal Total {
        get => SubTotal + Tax + Shipping.Price;
    }

    public Order(Guid id, string source, string number, string name, Guid customerId, Guid vendorId, string customerComment, DateTime orderDate, ShippingInfo shipping, BillingInfo billing, decimal tax, decimal priceAdjustment, bool rush, IReadOnlyDictionary<string, string> info, IEnumerable<IProduct> products, IEnumerable<AdditionalItem> additionalItems) {
        Id = id;
        Source = source;
        Number = number;
        Name = name;
        CustomerId = customerId;
        VendorId = vendorId;
        CustomerComment = customerComment;
        OrderDate = orderDate;
        Shipping = shipping;
        Billing = billing;
        Tax = tax;
        PriceAdjustment = priceAdjustment;
        Rush = rush;
        Info = info;
        Products = products;
        AdditionalItems = additionalItems;
    }

    public static Order Create(string source, string number, string name, Guid customerId, Guid vendorId, string comment, DateTime orderDate, ShippingInfo shipping, BillingInfo billing, decimal tax, decimal priceAdjustment, bool rush, IReadOnlyDictionary<string, string> info, IEnumerable<IProduct> products, IEnumerable<AdditionalItem> additionalItems, Guid? id = null) {
        return new Order(id ?? Guid.NewGuid(), source, number, name, customerId, vendorId, comment, orderDate, shipping, billing, tax, priceAdjustment, rush, info, products, additionalItems);
    }

}
