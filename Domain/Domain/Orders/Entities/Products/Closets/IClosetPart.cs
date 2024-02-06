using Domain.Orders.ValueObjects;
using Domain.Orders.Entities.Products;
using Domain.ValueObjects;

namespace Domain.Orders.Entities.Products.Closets;

public interface IClosetPartProduct : IProduct {

    public string SKU { get; }
    public Dimension Width { get; }
    public Dimension Length { get; }
    public ClosetMaterial Material { get; }
    public ClosetPaint? Paint { get; }
    public string EdgeBandingColor { get; }
    public string Comment { get; }

}
