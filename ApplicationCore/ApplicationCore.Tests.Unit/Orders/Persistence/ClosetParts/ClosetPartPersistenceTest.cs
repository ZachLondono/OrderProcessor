using ApplicationCore.Features.Products.UpdateClosetPart;
using Domain.Orders.Entities.Products.Closets;
using Domain.Orders.Enums;
using Domain.ValueObjects;

namespace ApplicationCore.Tests.Unit.Orders.Persistence.ClosetParts;

public class ClosetPartPersistenceTest : PersistenceTests {

    [Fact]
    public async Task InsertOrderWithClosetPart() {
        var part = new ClosetPart(Guid.NewGuid(), 1, 0M, 1, "",
                                "PC", Dimension.Zero, Dimension.Zero, new("", ClosetMaterialCore.Plywood), null, "", "", false, new Dictionary<string, string>() {
                                    { "Param1", "Value1"},
                                    { "Param2", "Value2"}
                                });
        await InsertAndQueryOrderWithProduct(part);
    }

    [Fact]
    public async Task DeleteOrderWithClosetPart() {
        var part = new ClosetPart(Guid.NewGuid(), 1, 0M, 1, "",
                                "PC", Dimension.Zero, Dimension.Zero, new("", ClosetMaterialCore.Plywood), null, "", "", false, new Dictionary<string, string>() {
                                    { "Param1", "Value1"},
                                    { "Param2", "Value2"}
                                });
        await InsertAndDeleteOrderWithProduct(part);
    }

    [Fact]
    public async Task InsertOrderWithClosetPartWithProductionNotes() {
        var part = new ClosetPart(Guid.NewGuid(), 1, 0M, 1, "",
                                "PC", Dimension.Zero, Dimension.Zero, new("", ClosetMaterialCore.Plywood), null, "", "", false, new Dictionary<string, string>() {
                                    { "Param1", "Value1"},
                                    { "Param2", "Value2"}
                                }) {
            ProductionNotes = new() { "A", "B", "C" }
        };
        await InsertAndQueryOrderWithProduct(part);
    }

    [Fact]
    public async Task DeleteOrderWithClosetPartWithProductionNotes() {
        var part = new ClosetPart(Guid.NewGuid(), 1, 0M, 1, "",
                                "PC", Dimension.Zero, Dimension.Zero, new("", ClosetMaterialCore.Plywood), null, "", "", false, new Dictionary<string, string>() {
                                    { "Param1", "Value1"},
                                    { "Param2", "Value2"}
                                }) {
            ProductionNotes = new() { "A", "B", "C" }
        };
        await InsertAndDeleteOrderWithProduct(part);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task InsertOrderWithClosetPart_InstallCams(bool installCams) {
        var part = new ClosetPart(Guid.NewGuid(), 1, 0M, 1, "",
                                "PC", Dimension.Zero, Dimension.Zero, new("", ClosetMaterialCore.Plywood), null, "", "", installCams, new Dictionary<string, string>() {
                                    { "Param1", "Value1"},
                                    { "Param2", "Value2"}
                                });
        await InsertAndQueryOrderWithProduct(part);
    }

    [Fact]
    public async Task UpdateClosetPart() {

        // Arrange
        var part = new ClosetPart(Guid.NewGuid(), 1, 0M, 1, "",
                                "PC", Dimension.Zero, Dimension.Zero, new("", ClosetMaterialCore.Plywood), null, "", "", false, new Dictionary<string, string>() {
                                    { "Param1", "Value1"},
                                    { "Param2", "Value2"}
                                });

        var orderId = await InsertOrderWithProduct(part);

        part.Qty = 123;
        part.Room = "New Room Name";
        part.SKU = "New SKU";
        part.Comment = "New Comment";

        // Act
        var sut = new UpdateClosetPart.Handler(Factory);
        await sut.Handle(new(part));

        // Assert
        await VerifyProductExistsInOrder(orderId, part);

    }

}
