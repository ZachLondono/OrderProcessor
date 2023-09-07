using ApplicationCore.Features.Orders.Shared.Domain.Products.Doors;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Tests.Unit.Orders.Persistence.Doors;

public class FivePieceDoorPersistenceTests : PersistenceTests {

    [Fact]
    public void InsertOrderWithFivePieceDoor() {
        var door = new FivePieceDoorProduct(Guid.NewGuid(), 1, 0M, 1, "", Dimension.FromInches(12), Dimension.FromInches(15), new(Dimension.FromInches(3)), Dimension.FromInches(0.75), Dimension.FromInches(0.25), "MDF");
        InsertAndQueryOrderWithProduct(door);
    }

    [Fact]
    public void DeleteOrderWithFivePieceDoor() {
        var door = new FivePieceDoorProduct(Guid.NewGuid(), 1, 0M, 1, "", Dimension.FromInches(12), Dimension.FromInches(15), new(Dimension.FromInches(3)), Dimension.FromInches(0.75), Dimension.FromInches(0.25), "MDF");
        InsertAndDeleteOrderWithProduct(door);
    }

    [Fact]
    public void InsertOrderWithFivePieceDoorWithProductionNotes() {
        var door = new FivePieceDoorProduct(Guid.NewGuid(), 1, 0M, 1, "", Dimension.FromInches(12), Dimension.FromInches(15), new(Dimension.FromInches(3)), Dimension.FromInches(0.75), Dimension.FromInches(0.25), "MDF") {
            ProductionNotes = new() { "A", "B", "C" }
        };
        InsertAndQueryOrderWithProduct(door);
    }

    [Fact]
    public void DeleteOrderWithFivePieceDoorWithProductionNotes() {
        var door = new FivePieceDoorProduct(Guid.NewGuid(), 1, 0M, 1, "", Dimension.FromInches(12), Dimension.FromInches(15), new(Dimension.FromInches(3)), Dimension.FromInches(0.75), Dimension.FromInches(0.25), "MDF") {
            ProductionNotes = new() { "A", "B", "C" }
        };
        InsertAndDeleteOrderWithProduct(door);
    }


}
