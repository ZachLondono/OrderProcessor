using Domain.Orders.Entities.Products.Doors;
using Domain.Orders.Enums;
using Domain.Orders.ValueObjects;
using Domain.ValueObjects;

namespace ApplicationCore.Tests.Unit.Orders.Persistence.Doors;

public class MDFDoorPersistenceTests : PersistenceTests {

    [Fact]
    public async Task InsertOrderWithMDFDoor() {
        var door = new MDFDoorProduct(Guid.NewGuid(), 0M, "", 1, 1, DoorType.Door, Dimension.FromInches(12), Dimension.FromInches(12), "", new DoorFrame(Dimension.FromInches(3)), "", Dimension.Zero, "", "", "", Dimension.Zero, DoorOrientation.Vertical, Array.Empty<AdditionalOpening>(), null);
        await InsertAndQueryOrderWithProduct(door);
    }

    [Fact]
    public async Task DeleteOrderWithMDFDoor() {
        var door = new MDFDoorProduct(Guid.NewGuid(), 0M, "", 1, 1, DoorType.Door, Dimension.FromInches(12), Dimension.FromInches(12), "", new DoorFrame(Dimension.FromInches(3)), "", Dimension.Zero, "", "", "", Dimension.Zero, DoorOrientation.Vertical, Array.Empty<AdditionalOpening>(), null);
        await InsertAndDeleteOrderWithProduct(door);
    }

}
