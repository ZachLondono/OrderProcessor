using Domain.Orders.Builders;
using Domain.Orders.Entities.Products.DrawerBoxes;
using FluentAssertions;
using OrderLoading.LoadAllmoxyOrderData.AllmoxyXMLModels;

namespace OrderLoading.Tests.Unit.AllmoxyOrderLoading;

public class DoweledDrawerBoxMappingTests {

    private readonly ProductBuilderFactory _factory = new();

    [Fact]
    public void MapDoweledDrawerBoxToProduct() {

        // Arrange
        int qty = 5;
        double height = 101;
        double width = 102;
        double depth = 103;
        double boxThickness = 16;
        double bottomThickness = 6;
        double heightAdj = 5;
        var model = new DoweledDrawerBoxModel() {
            GroupNumber = 1,
            LineNumber = 1,
            Qty = qty,
            UnitPrice = 0,
            Height = height,
            Width = width,
            Depth = depth,
            MaterialName = "MaterialName",
            BoxThickness = 16,
            BottomThickness = 6,
            MachineForUMSlides = false,
            FrontBackHeightAdj = 5,
            Room = "Room",
            ProductionNotes = [ "Note" ]
        };

        // Act
        var result = model.CreateProductOrItem(_factory);

        // Assert
        var box = result.AsT0 as DoweledDrawerBoxProduct;
        box.Should().NotBeNull();
        box!.Qty.Should().Be(qty);
        box.Height.AsMillimeters().Should().Be(height);
        box.Width.AsMillimeters().Should().Be(width);
        box.Depth.AsMillimeters().Should().Be(depth);
        box.BottomMaterial.Thickness.AsMillimeters().Should().Be(bottomThickness);
        box.FrontMaterial.Thickness.AsMillimeters().Should().Be(boxThickness);
        box.BackMaterial.Thickness.AsMillimeters().Should().Be(boxThickness);
        box.SideMaterial.Thickness.AsMillimeters().Should().Be(boxThickness);
        box.FrontBackHeightAdjustment.AsMillimeters().Should().Be(heightAdj);

    }

}
