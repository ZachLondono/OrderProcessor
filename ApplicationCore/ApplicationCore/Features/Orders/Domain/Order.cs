namespace ApplicationCore.Features.Orders.Domain;

public class Order {

    public Guid Id { get; }
    public string Source { get; private set; }
    public Status Status { get; private set; }
    public string Number { get; }
    public string Name { get; }
    public Guid CustomerId { get; }
    public Guid VendorId { get; }
    public string ProductionNote { get; }
    public string CustomerComment { get; }
    public DateTime OrderDate { get; }
    public DateTime? ReleaseDate { get; private set; }
    public DateTime? ProductionDate { get; private set; }
    public DateTime? CompleteDate { get; private set; }
    public decimal Tax { get; } = 0M;
    public decimal Shipping { get; } = 0M;
    public decimal PriceAdjustment { get; } = 0M;
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
        get => SubTotal + Tax + Shipping;
    }

    // TODO: Add related files

    public Order(Guid id, string source, Status status, string number, string name, Guid customerId, Guid vendorId, string productionNote, string customerComment, DateTime orderDate, DateTime? releaseDate, DateTime? productionDate, DateTime? completeDate, decimal tax, decimal shipping, decimal priceAdjustment, bool rush, IReadOnlyDictionary<string, string> info, IEnumerable<IProduct> products, IEnumerable<AdditionalItem> additionalItems) {
        Id = id;
        Source = source;
        Status = status;
        Number = number;
        Name = name;
        CustomerId = customerId;
        VendorId = vendorId;
        ProductionNote = productionNote;
        CustomerComment = customerComment;
        OrderDate = orderDate;
        ReleaseDate = releaseDate;
        ProductionDate = productionDate;
        CompleteDate = completeDate;
        Tax = tax;
        Shipping = shipping;
        PriceAdjustment = priceAdjustment;
        Rush = rush;
        Info = info;
        Products = products;
        AdditionalItems = additionalItems;
    }

    public static Order Create(string source, string number, string name, Guid customerId, Guid vendorId, string comment, DateTime orderDate, decimal tax, decimal shipping, decimal priceAdjustment, bool rush, IReadOnlyDictionary<string, string> info, IEnumerable<DrawerBox> boxes, IEnumerable<AdditionalItem> additionalItems, Guid? id = null) {
        return new Order(id ?? Guid.NewGuid(), source, Status.Pending, number, name, customerId, vendorId, "", comment, orderDate, null, null, null, tax, shipping, priceAdjustment, rush, info, boxes, additionalItems);
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
