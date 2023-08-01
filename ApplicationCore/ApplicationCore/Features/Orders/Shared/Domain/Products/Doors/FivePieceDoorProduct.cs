using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Shared.Domain;
using CADCodeProxy.Machining;

namespace ApplicationCore.Features.Orders.Shared.Domain.Products.Doors;

internal class FivePieceDoorProduct : FivePieceDoor, IProduct, ICNCPartContainer {

    public Guid Id { get; init; } 
    public int Qty { get; init; }
    public decimal UnitPrice { get; init; }
    public int ProductNumber { get; init; }
    public string Room { get; set; }

    public FivePieceDoorProduct(Guid id, int qty, decimal unitPrice, int productNumber, string room, Dimension width, Dimension height, DoorFrame frameSize, Dimension frameThickness, Dimension panelThickness, string material)
            : base(width, height, frameSize, frameThickness, panelThickness, material) {
        Id = id;
        Qty = qty;
        UnitPrice = unitPrice;
        ProductNumber = productNumber;
        Room = room;
    }

    public string GetDescription() => "Five Piece Door";

    public IEnumerable<Supply> GetSupplies() => Enumerable.Empty<Supply>();

    public IEnumerable<Part> GetCNCParts(string customerName) => GetCNCParts(Qty, ProductNumber, customerName, Room); 

    public bool ContainsCNCParts() => true;

}
