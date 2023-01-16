using ApplicationCore.Features.Orders.Shared.Domain;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
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
            CustomerId = Guid.NewGuid(),
            VendorId = Guid.NewGuid(),
        };

        var orderId = Guid.NewGuid();
        var boxes = new List<DovetailDrawerBoxProduct>() {
            new DrawerBoxBuilder().Build()
        };

        var items = new List<AdditionalItem>() {
            new AdditionalItem(Guid.NewGuid(), "Description", 123)
        };

        // Act
        var order = model.AsDomainModel(orderId, boxes, items);

        // Assert
        order.Id.Should().Be(orderId);
        order.Name.Should().Be(model.Name);
        order.Number.Should().Be(model.Number);
        order.CustomerId.Should().Be(model.CustomerId);
        order.VendorId.Should().Be(model.VendorId);
        order.Products.Should().BeEquivalentTo(boxes);
        order.AdditionalItems.Should().BeEquivalentTo(items);

    }

    [Fact]
    public void AdditionalItemDataModel_ShouldMapToDomainModel() {

        // Arrange
        var model = new AdditionalItemDataModel() {
            Id = Guid.NewGuid(),
            Description = "Description",
            Price = 123.45M
        };

        // Act
        var item = model.AsDomainModel();

        // Assert
        item.Id.Should().Be(model.Id);
        item.Description.Should().Be(model.Description);
        item.Price.Should().Be(model.Price);

    }

}
