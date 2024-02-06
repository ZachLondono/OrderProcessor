using Domain.Orders.Products.DrawerBoxes;
using Domain.ValueObjects;

namespace ApplicationCore.Tests.Unit.Orders.Persistence.DrawerBoxes;

public class DoweledDrawerBoxPersistenceTests : PersistenceTests {

    [Fact]
    public void InsertOrderWithDoweledDrawerBox() {
        var db = new DoweledDrawerBoxProduct(Guid.NewGuid(), 0M, 1, "", 1,
                                            Dimension.FromInches(4.125), Dimension.FromInches(21), Dimension.FromInches(21),
                                            new("", Dimension.Zero, false), new("", Dimension.Zero, false), new("", Dimension.Zero, false), new("", Dimension.Zero, false),
                                            false, Dimension.Zero);
        InsertAndQueryOrderWithProduct(db);
    }

    [Fact]
    public void DeleteOrderWithDoweledDrawerBox() {
        var db = new DoweledDrawerBoxProduct(Guid.NewGuid(), 0M, 1, "", 1,
                                            Dimension.FromInches(4.125), Dimension.FromInches(21), Dimension.FromInches(21),
                                            new("", Dimension.Zero, false), new("", Dimension.Zero, false), new("", Dimension.Zero, false), new("", Dimension.Zero, false),
                                            false, Dimension.Zero);
        InsertAndDeleteOrderWithProduct(db);
    }

    [Fact]
    public void InsertOrderWithDoweledDrawerBoxWithProductionNotes() {
        var db = new DoweledDrawerBoxProduct(Guid.NewGuid(), 0M, 1, "", 1,
                                            Dimension.FromInches(4.125), Dimension.FromInches(21), Dimension.FromInches(21),
                                            new("", Dimension.Zero, false), new("", Dimension.Zero, false), new("", Dimension.Zero, false), new("", Dimension.Zero, false),
                                            false, Dimension.Zero) {
            ProductionNotes = new() { "A", "B", "C" }
        };
        InsertAndQueryOrderWithProduct(db);
    }

    [Fact]
    public void DeleteOrderWithDoweledDrawerBoxWithProductionNotes() {
        var db = new DoweledDrawerBoxProduct(Guid.NewGuid(), 0M, 1, "", 1,
                                            Dimension.FromInches(4.125), Dimension.FromInches(21), Dimension.FromInches(21),
                                            new("", Dimension.Zero, false), new("", Dimension.Zero, false), new("", Dimension.Zero, false), new("", Dimension.Zero, false),
                                            false, Dimension.Zero) {
            ProductionNotes = new() { "A", "B", "C" }
        };
        InsertAndDeleteOrderWithProduct(db);
    }
}



