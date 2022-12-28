using ApplicationCore.Features.Orders.Domain;
using ApplicationCore.Features.Orders.Domain.Products;
using ApplicationCore.Features.Orders.Domain.ValueObjects;
using ApplicationCore.Features.Orders.Queries.DataModels;
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
        var boxes = new List<DovetailDrawerBox>() {
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

    [Fact]
    public void DrawerBoxDataModel_ShouldMapToDomainModel() {

        throw new NotImplementedException();

        /*// Arrange
        var model = new DrawerBoxDataModel() {
            Id = Guid.NewGuid(),
            LineInOrder = 1,
            UnitPrice = 123.45M,
            Qty = 123,
            Height = Dimension.FromMillimeters(25.4),
            Width = Dimension.FromMillimeters(25.4),
            Depth = Dimension.FromMillimeters(25.4),
            Note = "Note",
            PostFinish = true,
            ScoopFront = true,
            BoxMaterialId = Guid.NewGuid(),
            BottomMaterialId = Guid.NewGuid(),
            ClipsName = "Clips",
            NotchesName = "Notches",
            AccessoryName = "Accessory"
        };

        // Act
        var box = model.AsDomainModel();

        // Assert
        box.Id.Should().Be(model.Id);
        box.LineInOrder.Should().Be(model.LineInOrder);
        box.UnitPrice.Should().Be(model.UnitPrice);
        box.Qty.Should().Be(model.Qty);
        box.Height.Should().Be(model.Height);
        box.Width.Should().Be(model.Width);
        box.Depth.Should().Be(model.Depth);
        box.Note.Should().Be(model.Note);
        box.Options.PostFinish.Should().Be(model.PostFinish);
        box.Options.ScoopFront.Should().Be(model.ScoopFront);
        box.Options.BoxMaterialId.Id.Should().Be(model.BoxMaterialId);
        box.Options.BoxMaterialId.Name.Should().Be(model.BoxMaterialName);
        box.Options.BottomMaterial.Id.Should().Be(model.BottomMaterialId);
        box.Options.BottomMaterial.Name.Should().Be(model.BottomMaterialName);
        box.Options.Clips.Id.Should().Be(model.ClipsId);
        box.Options.Clips.Name.Should().Be(model.ClipsName);
        box.Options.Notches.Id.Should().Be(model.NotchesId);
        box.Options.Notches.Name.Should().Be(model.NotchesName);
        box.Options.Accessory.Id.Should().Be(model.AccessoryId);
        box.Options.Accessory.Name.Should().Be(model.AccessoryName);
*/

    }

}
