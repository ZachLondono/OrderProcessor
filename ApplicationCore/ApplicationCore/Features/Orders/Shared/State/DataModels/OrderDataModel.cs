using ApplicationCore.Features.Orders.Shared.Domain;
using ApplicationCore.Features.Orders.Shared.Domain.Products;

namespace ApplicationCore.Features.Orders.Shared.State.DataModels;

public class OrderDataModel {

    public string Source { get; set; } = string.Empty;

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

    public bool Rush { get; set; }

    public IDictionary<string, string> Info { get; set; } = new Dictionary<string, string>();

    public Order AsDomainModel(Guid orderId, IEnumerable<IProduct> products, IEnumerable<AdditionalItem> items) {

        return new Order(orderId, Source, Status, Number, Name, CustomerId, VendorId, ProductionNote, CustomerComment, OrderDate, ReleaseDate, ProductionDate, CompleteDate, Tax, Shipping, PriceAdjustment, Rush, Info.AsReadOnly(), products, items);

    }

}
