using Domain.Orders.Entities.Products.DrawerBoxes;
using Domain.Orders.Enums;
using Domain.ValueObjects;

namespace ApplicationCore.Tests.Unit.Orders.Persistence.DrawerBoxes;

public class DovetailDrawerBoxPersistenceTests : PersistenceTests {

    [Fact]
    public async Task InsertOrderWithDovetailDrawerBox() {
        var db = new DovetailDrawerBoxProduct(Guid.NewGuid(), 0M, 1, "", 1, Dimension.FromInches(4.125), Dimension.FromInches(21), Dimension.FromInches(21), "", new Dictionary<string, string>(), new("MatA", "MatB", "MatC", "MatD", "Clips", "Slides", "Notches", "Accessory", LogoPosition.None));
        await InsertAndQueryOrderWithProduct(db);
    }

    [Fact]
    public async Task DeleteOrderWithDovetailDrawerBox() {
        var db = new DovetailDrawerBoxProduct(Guid.NewGuid(), 0M, 1, "", 1, Dimension.FromInches(4.125), Dimension.FromInches(21), Dimension.FromInches(21), "", new Dictionary<string, string>(), new("MatA", "MatB", "MatC", "MatD", "Clips", "Slides", "Notches", "Accessory", LogoPosition.None));
        await InsertAndDeleteOrderWithProduct(db);
    }

    [Fact]
    public async Task InsertOrderWithDovetailDrawerBoxWithProductionNotes() {
        var db = new DovetailDrawerBoxProduct(Guid.NewGuid(), 0M, 1, "", 1, Dimension.FromInches(4.125), Dimension.FromInches(21), Dimension.FromInches(21), "", new Dictionary<string, string>(), new("MatA", "MatB", "MatC", "MatD", "Clips", "Slides", "Notches", "Accessory", LogoPosition.None)) {
            ProductionNotes = new() { "A", "B", "C" }
        };
        await InsertAndQueryOrderWithProduct(db);
    }

    [Fact]
    public async Task DeleteOrderWithDovetailDrawerBoxWithProductionNotes() {
        var db = new DovetailDrawerBoxProduct(Guid.NewGuid(), 0M, 1, "", 1, Dimension.FromInches(4.125), Dimension.FromInches(21), Dimension.FromInches(21), "", new Dictionary<string, string>(), new("MatA", "MatB", "MatC", "MatD", "Clips", "Slides", "Notches", "Accessory", LogoPosition.None)) {
            ProductionNotes = new() { "A", "B", "C" }
        };
        await InsertAndDeleteOrderWithProduct(db);
    }

}
