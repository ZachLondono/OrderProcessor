using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.Products;

public interface IClosetPartProduct {

    public int Qty { get; }
    public decimal UnitPrice { get; }
    public int ProductNumber { get; }
    public string Room { get; set; }
    public string SKU { get; }
    public Dimension Width { get; }
    public Dimension Length { get; }
    public ClosetMaterial Material { get; }
    public ClosetPaint? Paint { get; }
    public string EdgeBandingColor { get; }
    public string Comment { get; }
    public string GetDescription();

}
