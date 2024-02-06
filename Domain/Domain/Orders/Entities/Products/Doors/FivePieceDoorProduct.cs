using Domain.Orders.Components;
using Domain.Orders.ValueObjects;
using Domain.Orders.Entities.Products;
using Domain.ValueObjects;

namespace Domain.Orders.Entities.Products.Doors;

public class FivePieceDoorProduct : FivePieceDoor, IProduct {

    public Guid Id { get; init; }
    public int Qty { get; init; }
    public decimal UnitPrice { get; init; }
    public int ProductNumber { get; init; }
    public string Room { get; set; }
    public List<string> ProductionNotes { get; set; } = new();

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

    public IEnumerable<FivePieceDoorPart> GetParts() => GetParts(Qty);

}
