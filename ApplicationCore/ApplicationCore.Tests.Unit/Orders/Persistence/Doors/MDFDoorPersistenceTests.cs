using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.Products.Doors;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Tests.Unit.Orders.Persistence.Doors;

public class MDFDoorPersistenceTests : PersistenceTests {

    [Fact]
    public void InsertOrderWithMDFDoor() {
        var door = new MDFDoorProduct(Guid.NewGuid(), 0M, "", 1, 1, DoorType.Door, Dimension.FromInches(12), Dimension.FromInches(12), "", new DoorFrame(Dimension.FromInches(3)), "", Dimension.Zero, "", "", "", Dimension.Zero, DoorOrientation.Vertical, Array.Empty<AdditionalOpening>(), null);
        InsertAndQueryOrderWithProduct(door);
    }

    [Fact]
    public void DeleteOrderWithMDFDoor() {
        var door = new MDFDoorProduct(Guid.NewGuid(), 0M, "", 1, 1, DoorType.Door, Dimension.FromInches(12), Dimension.FromInches(12), "", new DoorFrame(Dimension.FromInches(3)), "", Dimension.Zero, "", "", "", Dimension.Zero, DoorOrientation.Vertical, Array.Empty<AdditionalOpening>(), null);
        InsertAndDeleteOrderWithProduct(door);
    }

}
