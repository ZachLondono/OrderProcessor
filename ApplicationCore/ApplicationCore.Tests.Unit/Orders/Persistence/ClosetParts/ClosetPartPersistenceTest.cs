using ApplicationCore.Features.Orders.Shared.Domain.Products.Closets;
using ApplicationCore.Features.Products.UpdateClosetPart;
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

    [Fact]
    public void InsertOrderWithClosetPartWithProductionNotes() {
        var part = new ClosetPart(Guid.NewGuid(), 1, 0M, 1, "",
                                "PC", Dimension.Zero, Dimension.Zero, new("", Features.Orders.Shared.Domain.Enums.ClosetMaterialCore.Plywood), null, "", "", new Dictionary<string, string>() {
                                    { "Param1", "Value1"},
                                    { "Param2", "Value2"}
                                }) {
            ProductionNotes = new() { "A", "B", "C" }
        };
        InsertAndQueryOrderWithProduct(part);
    }

    [Fact]
    public void DeleteOrderWithClosetPartWithProductionNotes() {
        var part = new ClosetPart(Guid.NewGuid(), 1, 0M, 1, "",
                                "PC", Dimension.Zero, Dimension.Zero, new("", Features.Orders.Shared.Domain.Enums.ClosetMaterialCore.Plywood), null, "", "", new Dictionary<string, string>() {
                                    { "Param1", "Value1"},
                                    { "Param2", "Value2"}
                                }) {
            ProductionNotes = new() { "A", "B", "C" }
        };
        InsertAndDeleteOrderWithProduct(part);
    }

    [Fact]
    public void UpdateClosetPart() {

        // Arrange
        var part = new ClosetPart(Guid.NewGuid(), 1, 0M, 1, "",
                                "PC", Dimension.Zero, Dimension.Zero, new("", Features.Orders.Shared.Domain.Enums.ClosetMaterialCore.Plywood), null, "", "", new Dictionary<string, string>() {
                                    { "Param1", "Value1"},
                                    { "Param2", "Value2"}
                                });

        var orderId = InsertOrderWithProduct(part);

        part.Qty = 123;
        part.Room = "New Room Name";
        part.SKU = "New SKU";
        part.Comment = "New Comment";

        // Act
        var sut = new UpdateClosetPart.Handler(Factory);
        sut.Handle(new(part)).Wait();

        // Assert
        VerifyProductExistsInOrder(orderId, part);

    }

}
