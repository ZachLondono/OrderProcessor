using Domain.Orders.Entities.Products.Doors;
using Domain.Orders.Enums;
using Domain.Orders.ValueObjects;
using Domain.ValueObjects;
using FluentAssertions;
using OneOf.Types;

namespace ApplicationCore.Tests.Unit.Orders.Persistence.Doors;

public class MDFDoorPersistenceTests : PersistenceTests {

    [Fact]
    public async Task InsertOrderWithMDFDoor_NoFinish() {
        var door = new MDFDoorProduct(Guid.NewGuid(), 0M, "", 1, 1, DoorType.Door, Dimension.FromInches(12), Dimension.FromInches(12), "", new DoorFrame(Dimension.FromInches(3)), "", Dimension.Zero, "", "", "", Dimension.Zero, DoorOrientation.Vertical, Array.Empty<AdditionalOpening>(), new None(), false);
        await InsertAndQueryOrderWithProduct(door, (actual) => {
            actual.Finish.Value.Should().BeOfType<None>();
        });
    }

    [Fact]
    public async Task InsertOrderWithMDFDoor_PaintFinish() {
        string color = "Custom Paint Color";
        var door = new MDFDoorProduct(Guid.NewGuid(), 0M, "", 1, 1, DoorType.Door, Dimension.FromInches(12), Dimension.FromInches(12), "", new DoorFrame(Dimension.FromInches(3)), "", Dimension.Zero, "", "", "", Dimension.Zero, DoorOrientation.Vertical, Array.Empty<AdditionalOpening>(), new Paint(color), false);
        await InsertAndQueryOrderWithProduct(door, (actual) => {
            actual.Finish.Value.Should().BeOfType<Paint>();
            actual.Finish.Value.As<Paint>().Color.Should().Be(color);
        });

    }

    [Fact]
    public async Task InsertOrderWithMDFDoor_PrimerFinish() {
        string color = "Custom Primer Color";
        var door = new MDFDoorProduct(Guid.NewGuid(), 0M, "", 1, 1, DoorType.Door, Dimension.FromInches(12), Dimension.FromInches(12), "", new DoorFrame(Dimension.FromInches(3)), "", Dimension.Zero, "", "", "", Dimension.Zero, DoorOrientation.Vertical, Array.Empty<AdditionalOpening>(), new Primer(color), false);
        await InsertAndQueryOrderWithProduct(door, (actual) => {
            actual.Finish.Value.Should().BeOfType<Primer>();
            actual.Finish.Value.As<Primer>().Color.Should().Be(color);
        });
    }

    [Fact]
    public async Task DeleteOrderWithMDFDoor() {
        var door = new MDFDoorProduct(Guid.NewGuid(), 0M, "", 1, 1, DoorType.Door, Dimension.FromInches(12), Dimension.FromInches(12), "", new DoorFrame(Dimension.FromInches(3)), "", Dimension.Zero, "", "", "", Dimension.Zero, DoorOrientation.Vertical, Array.Empty<AdditionalOpening>(), new None(), false);
        await InsertAndDeleteOrderWithProduct(door);
    }

}
