using ApplicationCore.Features.Orders.Shared.Domain.Products;

namespace ApplicationCore.Features.Orders.Shared.Domain;

public class Order {

    public Guid Id { get; }
    public string Source { get; private set; }
    public Status Status { get; private set; }
    public string Number { get; }
    public string Name { get; }
    public Customer Customer { get; }
    public Guid VendorId { get; }
    public string ProductionNote { get; }
    public string CustomerComment { get; }
    public DateTime OrderDate { get; }
    public DateTime? ReleaseDate { get; private set; }
    public DateTime? ProductionDate { get; private set; }
    public DateTime? CompleteDate { get; private set; }
    public ShippingInfo Shipping { get; }
    public decimal Tax { get; }
    public decimal PriceAdjustment { get; }
    public bool Rush { get; }
    public IReadOnlyDictionary<string, string> Info { get; }
    public IEnumerable<IProduct> Products { get; }
    public IEnumerable<AdditionalItem> AdditionalItems { get; }

    public decimal SubTotal {
        get => Products.Sum(b => b.Qty * b.UnitPrice) + AdditionalItems.Sum(i => i.Price);
    }

    public decimal AdjustedSubTotal {
        get => SubTotal + PriceAdjustment;
    }

    public decimal Total {
        get => SubTotal + Tax + Shipping.Price;
    }

    public Order(Guid id, string source, Status status, string number, string name, Customer customer, Guid vendorId, string productionNote, string customerComment, DateTime orderDate, DateTime? releaseDate, DateTime? productionDate, DateTime? completeDate, ShippingInfo shipping, decimal tax, decimal priceAdjustment, bool rush, IReadOnlyDictionary<string, string> info, IEnumerable<IProduct> products, IEnumerable<AdditionalItem> additionalItems) {
        Id = id;
        Source = source;
        Status = status;
        Number = number;
        Name = name;
        Customer = customer;
        VendorId = vendorId;
        ProductionNote = productionNote;
        CustomerComment = customerComment;
        OrderDate = orderDate;
        ReleaseDate = releaseDate;
        ProductionDate = productionDate;
        CompleteDate = completeDate;
        Shipping = shipping;
        Tax = tax;
        PriceAdjustment = priceAdjustment;
        Rush = rush;
        Info = info;
        Products = products;
        AdditionalItems = additionalItems;
    }

    public static Order Create(string source, string number, string name, Customer customer, Guid vendorId, string comment, DateTime orderDate, ShippingInfo shipping, decimal tax, decimal priceAdjustment, bool rush, IReadOnlyDictionary<string, string> info, IEnumerable<IProduct> products, IEnumerable<AdditionalItem> additionalItems, Guid? id = null) {
        return new Order(id ?? Guid.NewGuid(), source, Status.Pending, number, name, customer, vendorId, "", comment, orderDate, null, null, null, shipping, tax, priceAdjustment, rush, info, products, additionalItems);
    }

    public void Release(DateTime? productionDate = null) {
        ReleaseDate = DateTime.Now;
        if (productionDate is not null) ProductionDate = (DateTime)productionDate;
        else ProductionDate = DateTime.Today.AddDays(7);
        if (Status < Status.Released) Status = Status.Released;
    }

    public void Complete() {
        CompleteDate = DateTime.Now;
        Status = Status.Completed;
    }

}
