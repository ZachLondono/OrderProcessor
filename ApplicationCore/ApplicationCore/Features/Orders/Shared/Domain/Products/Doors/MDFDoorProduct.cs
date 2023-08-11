using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.Components;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.Products.Doors;

public class MDFDoorProduct : MDFDoor, IProduct, IMDFDoorContainer {

    public Guid Id { get; }
    public decimal UnitPrice { get; }
    public string Room { get; set; }
    public string GetDescription() => "MDF Door";

    public MDFDoorProduct(Guid id, decimal unitPrice, string room, int qty, int productNumber, DoorType type, Dimension height, Dimension width, string note, DoorFrame frameSize, string material, Dimension thickness, string framingBead, string edgeDetail, string panelDetail, Dimension panelDrop, DoorOrientation orientation, AdditionalOpening[] additionalOpenings, string? paintColor)
        : base(qty, productNumber, type, height, width, note, frameSize, material, thickness, framingBead, edgeDetail, panelDetail, panelDrop, orientation, additionalOpenings, paintColor) {
        Id = id;
        UnitPrice = unitPrice;
        Room = room;
    }

    public static MDFDoorProduct Create(decimal unitPrice, string room, int qty, int productNumber, DoorType type, Dimension height, Dimension width, string note, DoorFrame frameSize, string material, Dimension thickness, string framingBead, string edgeDetail, string panelDetail, Dimension panelDrop, DoorOrientation orientation, AdditionalOpening[] additionalOpenings, string? paintColor)
        => new(Guid.NewGuid(), unitPrice, room, qty, productNumber, type, height, width, note, frameSize, material, thickness, framingBead, edgeDetail, panelDetail, panelDrop, orientation, additionalOpenings, paintColor);

    public bool ContainsDoors() => true;

    public IEnumerable<MDFDoor> GetDoors(Func<MDFDoorBuilder> getBuilder) { yield return this; }

    public IEnumerable<Supply> GetSupplies() => Enumerable.Empty<Supply>();

}