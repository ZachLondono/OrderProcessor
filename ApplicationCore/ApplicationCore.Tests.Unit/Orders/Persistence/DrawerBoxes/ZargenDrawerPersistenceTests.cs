using Domain.Orders.Products.Closets;
using Domain.ValueObjects;

namespace ApplicationCore.Tests.Unit.Orders.Persistence.DrawerBoxes;

public class ZargenDrawerPersistenceTests : PersistenceTests {

    [Fact]
    public void InsertOrderWithZargenDrawer() {
        var db = new ZargenDrawer(Guid.NewGuid(), 1, 0M, 1, "",
                                "D93", Dimension.Zero, Dimension.Zero, Dimension.Zero, new("", Features.Orders.Shared.Domain.Enums.ClosetMaterialCore.Plywood), null, "", "", new Dictionary<string, string>() {
                                    { "Param1", "Value1"},
                                    { "Param2", "Value2"}
                                });
        InsertAndQueryOrderWithProduct(db);
    }

    [Fact]
    public void DeleteOrderWithZargenDrawer() {
        var db = new ZargenDrawer(Guid.NewGuid(), 1, 0M, 1, "",
                                "D93", Dimension.Zero, Dimension.Zero, Dimension.Zero, new("", Features.Orders.Shared.Domain.Enums.ClosetMaterialCore.Plywood), null, "", "", new Dictionary<string, string>() {
                                    { "Param1", "Value1"},
                                    { "Param2", "Value2"}
                                });
        InsertAndDeleteOrderWithProduct(db);
    }

    [Fact]
    public void InsertOrderWithZargenDrawerWithProductionNotes() {
        var db = new ZargenDrawer(Guid.NewGuid(), 1, 0M, 1, "",
                                "D93", Dimension.Zero, Dimension.Zero, Dimension.Zero, new("", Features.Orders.Shared.Domain.Enums.ClosetMaterialCore.Plywood), null, "", "", new Dictionary<string, string>() {
                                    { "Param1", "Value1"},
                                    { "Param2", "Value2"}
                                }) {
            ProductionNotes = new() { "A", "B", "C" }
        };
        InsertAndQueryOrderWithProduct(db);
    }

    [Fact]
    public void DeleteOrderWithZargenDrawerWithProductionNotes() {
        var db = new ZargenDrawer(Guid.NewGuid(), 1, 0M, 1, "",
                                "D93", Dimension.Zero, Dimension.Zero, Dimension.Zero, new("", Features.Orders.Shared.Domain.Enums.ClosetMaterialCore.Plywood), null, "", "", new Dictionary<string, string>() {
                                    { "Param1", "Value1"},
                                    { "Param2", "Value2"}
                                }) {
            ProductionNotes = new() { "A", "B", "C" }
        };
        InsertAndDeleteOrderWithProduct(db);
    }
}