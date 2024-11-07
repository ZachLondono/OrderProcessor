using Domain.Orders.Entities.Products.DrawerBoxes;
using Domain.Orders.ValueObjects;
using Domain.ValueObjects;

namespace ApplicationCore.Tests.Unit.Orders.Persistence.DrawerBoxes;

public class DoweledDrawerBoxPersistenceTests : PersistenceTests {

    [Fact]
    public async Task InsertOrderWithDoweledDrawerBox() {
        var db = new DoweledDrawerBoxProduct(Guid.NewGuid(), 0M, 1, "", 1,
                                            Dimension.FromInches(4.125), Dimension.FromInches(21), Dimension.FromInches(21),
                                            new("", Dimension.Zero, false), new("", Dimension.Zero, false), new("", Dimension.Zero, false), new("", Dimension.Zero, false),
                                            false, Dimension.Zero, DoweledDrawerBoxConfig.NO_NOTCH);
        await InsertAndQueryOrderWithProduct(db);
    }

    [Fact]
    public async Task DeleteOrderWithDoweledDrawerBox() {
        var db = new DoweledDrawerBoxProduct(Guid.NewGuid(), 0M, 1, "", 1,
                                            Dimension.FromInches(4.125), Dimension.FromInches(21), Dimension.FromInches(21),
                                            new("", Dimension.Zero, false), new("", Dimension.Zero, false), new("", Dimension.Zero, false), new("", Dimension.Zero, false),
                                            false, Dimension.Zero, DoweledDrawerBoxConfig.NO_NOTCH);
        await InsertAndDeleteOrderWithProduct(db);
    }

    [Fact]
    public async Task InsertOrderWithDoweledDrawerBoxWithProductionNotes() {
        var db = new DoweledDrawerBoxProduct(Guid.NewGuid(), 0M, 1, "", 1,
                                            Dimension.FromInches(4.125), Dimension.FromInches(21), Dimension.FromInches(21),
                                            new("", Dimension.Zero, false), new("", Dimension.Zero, false), new("", Dimension.Zero, false), new("", Dimension.Zero, false),
                                            false, Dimension.Zero, DoweledDrawerBoxConfig.NO_NOTCH) {
            ProductionNotes = new() { "A", "B", "C" }
        };
        await InsertAndQueryOrderWithProduct(db);
    }

    [Fact]
    public async Task DeleteOrderWithDoweledDrawerBoxWithProductionNotes() {
        var db = new DoweledDrawerBoxProduct(Guid.NewGuid(), 0M, 1, "", 1,
                                            Dimension.FromInches(4.125), Dimension.FromInches(21), Dimension.FromInches(21),
                                            new("", Dimension.Zero, false), new("", Dimension.Zero, false), new("", Dimension.Zero, false), new("", Dimension.Zero, false),
                                            false, Dimension.Zero, DoweledDrawerBoxConfig.NO_NOTCH) {
            ProductionNotes = new() { "A", "B", "C" }
        };
        await InsertAndDeleteOrderWithProduct(db);
    }

}
