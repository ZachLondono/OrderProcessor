using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.Products;

public class MDFDoorProduct : MDFDoor, IProduct, IDoorContainer {

    public Guid Id { get; }

    public decimal UnitPrice { get; }

    public MDFDoorProduct(Guid id, decimal unitPrice, int qty, int productNumber, DoorType type, Dimension height, Dimension width, string note, string material, string framingBead, string edgeDetail, DoorFrame frameSize, Dimension panelDrop, string? paintColor)
        : base(qty, productNumber, type, height, width, note, material, framingBead, edgeDetail, frameSize, panelDrop, paintColor) {
        Id = id;
        UnitPrice = unitPrice;
    }

    public static MDFDoorProduct Create(decimal unitPrice, int qty, int productNumber, DoorType type, Dimension height, Dimension width, string note, string material, string framingBead, string edgeDetail, DoorFrame frameSize, Dimension panelDrop, string? paintColor)
        => new(Guid.NewGuid(), unitPrice, qty, productNumber, type, height, width, note, material, framingBead, edgeDetail, frameSize, panelDrop, paintColor);

    public IEnumerable<MDFDoor> GetDoors(Func<MDFDoorBuilder> getBuilder) { yield return this; }

}