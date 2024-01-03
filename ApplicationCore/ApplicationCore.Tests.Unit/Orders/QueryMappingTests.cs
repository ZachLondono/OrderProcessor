using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using ApplicationCore.Features.Orders.Shared.Domain.Products.DrawerBoxes;
using ApplicationCore.Features.Orders.Shared.State.DataModels;
using FluentAssertions;

namespace ApplicationCore.Tests.Unit.Orders;

public class QueryMappingTests {

    [Fact]
    public void OrderDataModel_ShouldMapToDomainModel() {

        // Arrange
        var model = new OrderDataModel() {
            Name = "Name",
            Number = "Number",
            VendorId = Guid.NewGuid(),
        };

        var orderId = Guid.NewGuid();
        var boxes = new List<DovetailDrawerBoxProduct>() {
            new DrawerBoxBuilder().Build()
        };

        var items = new List<AdditionalItem>() {
            new AdditionalItem(Guid.NewGuid(), 1, "Description", 123)
        };

        // Act
        var order = model.ToDomainModel(orderId, boxes, items);

        // Assert
        order.Id.Should().Be(orderId);
        order.Name.Should().Be(model.Name);
        order.Number.Should().Be(model.Number);
        order.VendorId.Should().Be(model.VendorId);
        order.Products.Should().BeEquivalentTo(boxes);
        order.AdditionalItems.Should().BeEquivalentTo(items);

    }

    [Fact]
    public void AdditionalItemDataModel_ShouldMapToDomainModel() {

        // Arrange
        var model = new AdditionalItemDataModel() {
            Id = Guid.NewGuid(),
            Qty = 2,
            Description = "Description",
            Price = 123.45M
        };

        // Act
        var item = model.ToDomainModel();

        // Assert
        item.Id.Should().Be(model.Id);
        item.Qty.Should().Be(model.Qty);
        item.Description.Should().Be(model.Description);
        item.UnitPrice.Should().Be(model.Price);

    }

}
