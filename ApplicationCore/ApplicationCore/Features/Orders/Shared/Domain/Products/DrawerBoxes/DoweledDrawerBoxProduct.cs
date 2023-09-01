using ApplicationCore.Features.Orders.Shared.Domain.Components;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Shared.Domain;
using CADCodeProxy.Machining;

namespace ApplicationCore.Features.Orders.Shared.Domain.Products.DrawerBoxes;

public class DoweledDrawerBoxProduct : DoweledDrawerBox, IProduct, ICNCPartContainer {

    public Guid Id { get; }
    public int ProductNumber { get; }
    public decimal UnitPrice { get; }
    public string Room { get; set; }

    public DoweledDrawerBoxProduct(Guid id, decimal unitPrice, int qty, string room, int productNumber, Dimension height, Dimension width, Dimension depth, DoweledDrawerBoxMaterial front, DoweledDrawerBoxMaterial back, DoweledDrawerBoxMaterial sides, DoweledDrawerBoxMaterial bottom, bool machineThicknessForUMSlides, Dimension frontBackHeightAdjustment)
        : base(qty, height, width, depth, front, back, sides, bottom, machineThicknessForUMSlides, frontBackHeightAdjustment) {
        Id = id;
        ProductNumber = productNumber;
        UnitPrice = unitPrice;
        Room = room;
    }

    public string GetDescription() => $"Doweled Drawer Box{(MachineThicknessForUMSlides ? "" : " - With UM Notch")}";

    // TODO: maybe add option to include slides, clips etc.
    public IEnumerable<Supply> GetSupplies() => Enumerable.Empty<Supply>();

    public IEnumerable<Part> GetCNCParts(string customerName) => GetCNCParts(Qty, ProductNumber, customerName, Room);

}
