using ApplicationCore.Features.Orders.Domain;

namespace ApplicationCore.Features.Orders.Queries.DataModels;

public class OrderDataModel {

    public string Number { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public Status Status { get; set; } = Status.UNKNOWN;

    public Guid CustomerId { get; set; }

    public Guid VendorId { get; set; }

    public string ProductionNote { get; set; } = string.Empty;

    public string CustomerComment { get; set; } = string.Empty;

    public DateTime OrderDate { get; set; }

    public DateTime? ReleaseDate { get; set; }

    public DateTime? ProductionDate { get; set; }

    public DateTime? CompleteDate { get; set; }

    public decimal Tax { get; set; }

    public decimal Shipping { get; set; }

    public decimal PriceAdjustment { get; set; }

    public IEnumerable<KeyValuePair<string, string>> Info { get; set; } = new Dictionary<string, string>();

    public Order AsDomainModel(Guid orderId, IEnumerable<DrawerBox> boxes, IEnumerable<AdditionalItem> items) {
        return new Order(orderId, Status, Number, Name, CustomerId, VendorId, ProductionNote, CustomerComment, OrderDate, ReleaseDate, ProductionDate, CompleteDate, Tax, Shipping, PriceAdjustment, new Dictionary<string,string>(Info), boxes, items);
    }

}
