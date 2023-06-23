using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.Products;

public class DoweledDrawerBoxProduct : DoweledDrawerBox, IProduct {

    public Guid Id { get; } 

    public decimal UnitPrice { get; }

    public string Room { get; set; }

    public DoweledDrawerBoxProduct(Guid id, decimal unitPrice, int qty, string room, int productNumber, Dimension height, Dimension width, Dimension depth, DoweledDrawerBoxMaterial front, DoweledDrawerBoxMaterial back, DoweledDrawerBoxMaterial sides, DoweledDrawerBoxMaterial bottom, bool machineThicknessForUMSlides, bool drillBacksForHooks, bool drillFrontsForDrawerFace, Dimension frontBackHeightAdjustment)
        : base(qty, productNumber, height, width, depth, front, back, sides, bottom, machineThicknessForUMSlides, drillBacksForHooks, drillFrontsForDrawerFace, frontBackHeightAdjustment) {
        Id = id;
        UnitPrice = unitPrice;
        Room = room;
    }

    public string GetDescription() => "Doweled Drawer Box";

    // TODO: maybe add option to include slides, clips etc.
    public IEnumerable<Supply> GetSupplies() => Enumerable.Empty<Supply>();

}
