using Domain.Orders.Builders;
using Domain.Orders.Components;
using Domain.Orders.Enums;
using Domain.Orders.ValueObjects;
using Domain.ValueObjects;

namespace Domain.Orders.Entities.Products.Doors;

public class MDFDoorProduct : MDFDoor, IProduct, IMDFDoorContainer {

    public Guid Id { get; }
    public decimal UnitPrice { get; }
    public string Room { get; set; }
    public string GetDescription() => "MDF Door";
    public List<string> ProductionNotes { get; set; } = [];

    public MDFDoorProduct(Guid id, decimal unitPrice, string room, int qty, int productNumber, DoorType type, Dimension height, Dimension width, string note, DoorFrame frameSize, string material, Dimension thickness, string framingBead, string edgeDetail, string panelDetail, Dimension panelDrop, DoorOrientation orientation, AdditionalOpening[] additionalOpenings, MDFDoorFinish finish, MDFDoorPanel panel)
        : base(qty, productNumber, type, height, width, note, frameSize, material, thickness, framingBead, edgeDetail, panelDetail, panelDrop, orientation, additionalOpenings, finish, panel) {
        Id = id;
        UnitPrice = unitPrice;
        Room = room;
    }

    public static MDFDoorProduct Create(decimal unitPrice, string room, int qty, int productNumber, DoorType type, Dimension height, Dimension width, string note, DoorFrame frameSize, string material, Dimension thickness, string framingBead, string edgeDetail, string panelDetail, Dimension panelDrop, DoorOrientation orientation, AdditionalOpening[] additionalOpenings, MDFDoorFinish finish, MDFDoorPanel panel)
        => new(Guid.NewGuid(), unitPrice, room, qty, productNumber, type, height, width, note, frameSize, material, thickness, framingBead, edgeDetail, panelDetail, panelDrop, orientation, additionalOpenings, finish, panel);

    public bool ContainsDoors() => true;

    public IEnumerable<MDFDoor> GetDoors(Func<MDFDoorBuilder> getBuilder) { yield return this; }

}