using Domain.Orders.Components;
using Domain.Orders.ValueObjects;
using Domain.ValueObjects;
using CADCodeProxy.Machining;
using Domain.Orders.Entities.Products;
using Domain.Orders;

namespace Domain.Orders.Entities.Products.DrawerBoxes;

public class DoweledDrawerBoxProduct : DoweledDrawerBox, IProduct, ICNCPartContainer {

    // TODO: add property for Under Mount Notch

    public Guid Id { get; }
    public int ProductNumber { get; }
    public decimal UnitPrice { get; }
    public string Room { get; set; }
    public List<string> ProductionNotes { get; set; } = new();

    public DoweledDrawerBoxProduct(Guid id, decimal unitPrice, int qty, string room, int productNumber, Dimension height, Dimension width, Dimension depth, DoweledDrawerBoxMaterial front, DoweledDrawerBoxMaterial back, DoweledDrawerBoxMaterial sides, DoweledDrawerBoxMaterial bottom, bool machineThicknessForUMSlides, Dimension frontBackHeightAdjustment)
        : base(qty, height, width, depth, front, back, sides, bottom, machineThicknessForUMSlides, frontBackHeightAdjustment) {
        Id = id;
        ProductNumber = productNumber;
        UnitPrice = unitPrice;
        Room = room;
    }

    public string GetDescription() => $"Doweled Drawer Box";

    // TODO: maybe add option to include slides, clips etc.
    public IEnumerable<Supply> GetSupplies() => Enumerable.Empty<Supply>();

    public IEnumerable<Part> GetCNCParts(string customerName) => GetCNCParts(ProductNumber, customerName, Room);

}
