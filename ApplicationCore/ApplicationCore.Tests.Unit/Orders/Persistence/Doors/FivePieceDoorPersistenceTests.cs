using Domain.Orders.Entities.Products.Doors;
using Domain.Orders.Enums;
using Domain.ValueObjects;

namespace ApplicationCore.Tests.Unit.Orders.Persistence.Doors;

public class FivePieceDoorPersistenceTests : PersistenceTests {

    [Fact]
    public async Task InsertOrderWithFivePieceDoor() {
        var door = new FivePieceDoorProduct(Guid.NewGuid(),
                                            1,
                                            0M,
                                            1,
                                            "",
                                            Dimension.FromInches(12),
                                            Dimension.FromInches(15),
                                            new(Dimension.FromInches(3)),
                                            Dimension.FromInches(0.75),
                                            Dimension.FromInches(0.25),
                                            "MDF",
                                            DoorType.Door);
        await InsertAndQueryOrderWithProduct(door);
    }

    [Fact]
    public async Task DeleteOrderWithFivePieceDoor() {
        var door = new FivePieceDoorProduct(Guid.NewGuid(),
                                            1,
                                            0M,
                                            1,
                                            "",
                                            Dimension.FromInches(12),
                                            Dimension.FromInches(15),
                                            new(Dimension.FromInches(3)),
                                            Dimension.FromInches(0.75),
                                            Dimension.FromInches(0.25),
                                            "MDF",
                                            DoorType.Door);
        await InsertAndDeleteOrderWithProduct(door);
    }

    [Fact]
    public async Task InsertOrderWithFivePieceDoorWithProductionNotes() {
        var door = new FivePieceDoorProduct(Guid.NewGuid(),
                                            1,
                                            0M,
                                            1,
                                            "",
                                            Dimension.FromInches(12),
                                            Dimension.FromInches(15),
                                            new(Dimension.FromInches(3)),
                                            Dimension.FromInches(0.75),
                                            Dimension.FromInches(0.25),
                                            "MDF",
                                            DoorType.Door) {
            ProductionNotes = new() { "A", "B", "C" }
        };
        await InsertAndQueryOrderWithProduct(door);
    }

    [Fact]
    public async Task DeleteOrderWithFivePieceDoorWithProductionNotes() {
        var door = new FivePieceDoorProduct(Guid.NewGuid(),
                                            1,
                                            0M,
                                            1,
                                            "",
                                            Dimension.FromInches(12),
                                            Dimension.FromInches(15),
                                            new(Dimension.FromInches(3)),
                                            Dimension.FromInches(0.75),
                                            Dimension.FromInches(0.25),
                                            "MDF",
                                            DoorType.Door) {
            ProductionNotes = new() { "A", "B", "C" }
        };
        await InsertAndDeleteOrderWithProduct(door);
    }

}
