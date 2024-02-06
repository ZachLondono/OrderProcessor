using Domain.Orders.ValueObjects;
using Domain.Orders.Entities.Products;

namespace Domain.Orders.Entities;

public class Order {

    public Guid Id { get; }
    public string Source { get; }
    public string Number { get; }
    public string Name { get; }
    public string WorkingDirectory { get; set; }
    public Guid CustomerId { get; }
    public Guid VendorId { get; }
    public string CustomerComment { get; }
    public DateTime OrderDate { get; }
    public DateTime? DueDate { get; set; }
    public ShippingInfo Shipping { get; }
    public BillingInfo Billing { get; }
    public decimal Tax { get; }
    public decimal PriceAdjustment { get; }
    public bool Rush { get; }
    public IReadOnlyDictionary<string, string> Info { get; }
    public IReadOnlyCollection<IProduct> Products { get; }
    public IReadOnlyCollection<AdditionalItem> AdditionalItems { get; }

    public string Note { get; set; } = string.Empty;

    public decimal SubTotal {
        get => Products.Sum(b => b.Qty * b.UnitPrice) + AdditionalItems.Sum(i => i.UnitPrice);
    }

    public decimal AdjustedSubTotal {
        get => SubTotal + PriceAdjustment;
    }

    public decimal Total {
        get => SubTotal + Tax + Shipping.Price;
    }

    public Order(Guid id, string source, string number, string name, string note, string workingDirectory, Guid customerId, Guid vendorId, string customerComment, DateTime orderDate, DateTime? dueDate, ShippingInfo shipping, BillingInfo billing, decimal tax, decimal priceAdjustment, bool rush, IReadOnlyDictionary<string, string> info, IReadOnlyCollection<IProduct> products, IReadOnlyCollection<AdditionalItem> additionalItems) {
        Id = id;
        Source = source;
        Number = number;
        Name = name;
        Note = note;
        WorkingDirectory = workingDirectory;
        CustomerId = customerId;
        VendorId = vendorId;
        CustomerComment = customerComment;
        OrderDate = orderDate;
        DueDate = dueDate;
        Shipping = shipping;
        Billing = billing;
        Tax = tax;
        PriceAdjustment = priceAdjustment;
        Rush = rush;
        Info = info;
        Products = products;
        AdditionalItems = additionalItems;
    }

    public static Order Create(string source, string number, string name, string note, string workingDirectory, Guid customerId, Guid vendorId, string comment, DateTime orderDate, DateTime? dueDate, ShippingInfo shipping, BillingInfo billing, decimal tax, decimal priceAdjustment, bool rush, IReadOnlyDictionary<string, string> info, IReadOnlyCollection<IProduct> products, IReadOnlyCollection<AdditionalItem> additionalItems, Guid? id = null) {
        return new Order(id ?? Guid.NewGuid(), source, number, name, note, workingDirectory, customerId, vendorId, comment, orderDate, dueDate, shipping, billing, tax, priceAdjustment, rush, info, products, additionalItems);
    }

}
