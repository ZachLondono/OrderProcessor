using Domain.Orders.Components;
using Domain.Orders.ValueObjects;
using Domain.ValueObjects;
using CADCodeProxy.Machining;

namespace Domain.Orders.Entities.Products.DrawerBoxes;

public class DoweledDrawerBoxProduct : DoweledDrawerBox, IProduct, ICNCPartContainer {

    // TODO: add property for Under Mount Notch

    public Guid Id { get; }
    public int ProductNumber { get; }
    public decimal UnitPrice { get; }
    public string Room { get; set; }
    public List<string> ProductionNotes { get; set; } = [];

    public DoweledDrawerBoxProduct(Guid id, decimal unitPrice, int qty, string room, int productNumber, Dimension height, Dimension width, Dimension depth, DoweledDrawerBoxMaterial front, DoweledDrawerBoxMaterial back, DoweledDrawerBoxMaterial sides, DoweledDrawerBoxMaterial bottom, bool machineThicknessForUMSlides, Dimension frontBackHeightAdjustment)
        : base(qty, height, width, depth, front, back, sides, bottom, machineThicknessForUMSlides, frontBackHeightAdjustment) {
        Id = id;
        ProductNumber = productNumber;
        UnitPrice = unitPrice;
        Room = room;
    }

    public string GetDescription() => $"Doweled Drawer Box";

    public IEnumerable<Part> GetCNCParts(string customerName) => GetCNCParts(ProductNumber, customerName, Room);

}
