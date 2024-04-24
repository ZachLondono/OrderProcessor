using Domain.Orders.Components;
using Domain.Orders.Enums;
using Domain.Orders.ValueObjects;
using Domain.ValueObjects;

namespace Domain.Orders.Entities.Products.Doors;

public class FivePieceDoorProduct : FivePieceDoor, IProduct {

    public Guid Id { get; init; }
    public int Qty { get; init; }
    public decimal UnitPrice { get; init; }
    public int ProductNumber { get; init; }
    public string Room { get; set; }
    public List<string> ProductionNotes { get; set; } = [];

    public FivePieceDoorProduct(Guid id, int qty, decimal unitPrice, int productNumber, string room, Dimension width, Dimension height, DoorFrame frameSize, Dimension frameThickness, Dimension panelThickness, string material, DoorType doorType)
            : base(width, height, frameSize, frameThickness, panelThickness, material, doorType) {
        Id = id;
        Qty = qty;
        UnitPrice = unitPrice;
        ProductNumber = productNumber;
        Room = room;
    }

    public string GetDescription() => DoorType switch {
        DoorType.DrawerFront => "Five Piece Drawer Front",
        DoorType.Door or _ => "Five Piece Door"
    };

    public IEnumerable<FivePieceDoorPart> GetParts() => GetParts(Qty);

}
