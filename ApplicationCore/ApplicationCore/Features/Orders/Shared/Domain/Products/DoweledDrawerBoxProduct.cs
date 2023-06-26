using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Shared.Domain;
using CADCodeProxy.Machining;

namespace ApplicationCore.Features.Orders.Shared.Domain.Products;

public class DoweledDrawerBoxProduct : DoweledDrawerBox, IProduct, ICNCPartContainer {

    public Guid Id { get; } 
    public int Qty { get; }
    public int ProductNumber { get; }
    public decimal UnitPrice { get; }
    public string Room { get; set; }

    public DoweledDrawerBoxProduct(Guid id, decimal unitPrice, int qty, string room, int productNumber, Dimension height, Dimension width, Dimension depth, DoweledDrawerBoxMaterial front, DoweledDrawerBoxMaterial back, DoweledDrawerBoxMaterial sides, DoweledDrawerBoxMaterial bottom, bool machineThicknessForUMSlides, Dimension frontBackHeightAdjustment)
        : base(height, width, depth, front, back, sides, bottom, machineThicknessForUMSlides, frontBackHeightAdjustment) {
        Id = id;
        Qty = qty;
        ProductNumber = productNumber; 
        UnitPrice = unitPrice;
        Room = room;
    }

    public string GetDescription() => "Doweled Drawer Box";

    // TODO: maybe add option to include slides, clips etc.
    public IEnumerable<Supply> GetSupplies() => Enumerable.Empty<Supply>();

    public IEnumerable<Part> GetCNCParts() => GetCNCParts(Qty, ProductNumber);

}
