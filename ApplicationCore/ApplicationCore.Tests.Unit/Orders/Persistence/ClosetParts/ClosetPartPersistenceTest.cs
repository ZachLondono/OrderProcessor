using ApplicationCore.Features.Orders.Shared.Domain.Products.Closets;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Tests.Unit.Orders.Persistence.ClosetParts;

public class ClosetPartPersistenceTest : PersistenceTests {

    [Fact]
    public void InsertOrderWithClosetPart() {
        var part = new ClosetPart(Guid.NewGuid(), 1, 0M, 1, "",
                                "PC", Dimension.Zero, Dimension.Zero, new("", Features.Orders.Shared.Domain.Enums.ClosetMaterialCore.Plywood), null, "", "", new Dictionary<string, string>() {
                                    { "Param1", "Value1"},
                                    { "Param2", "Value2"}
                                });
        InsertAndQueryOrderWithProduct(part);
    }

    [Fact]
    public void DeleteOrderWithClosetPart() {
        var part = new ClosetPart(Guid.NewGuid(), 1, 0M, 1, "",
                                "PC", Dimension.Zero, Dimension.Zero, new("", Features.Orders.Shared.Domain.Enums.ClosetMaterialCore.Plywood), null, "", "", new Dictionary<string, string>() {
                                    { "Param1", "Value1"},
                                    { "Param2", "Value2"}
                                });
        InsertAndDeleteOrderWithProduct(part);
    }

}
