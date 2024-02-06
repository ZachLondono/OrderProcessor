using ApplicationCore.Features.CNC.ReleasePDF;
using ApplicationCore.Features.Orders.Shared.Domain.Components;
using Domain.ValueObjects;
using ApplicationCore.Tests.Unit.Orders.Products.DoweledDrawerBoxTests;
using FluentAssertions;

namespace ApplicationCore.Tests.Unit.Orders;

public class DoweledDrawerBoxTests {

    [Fact]
    public void BoxBottomSize() {

        // Arrange
        var boxWidth = Dimension.FromInches(10);
        var boxDepth = Dimension.FromInches(10);
        var frontThickness = Dimension.FromMillimeters(20);
        var backThickness = Dimension.FromMillimeters(20);
        var sideThickness = Dimension.FromMillimeters(20);
        var bottomThickness = Dimension.FromMillimeters(6.35);
        var materialName = "Material Name";

        var dadoDepth = Dimension.FromMillimeters(10);
        var bottomUndersize = Dimension.FromMillimeters(1);

        var box = new DoweledDrawerBoxBuilder {
            SideMaterial = new("", sideThickness, false),
            FrontMaterial = new("", frontThickness, false),
            BackMaterial = new("", backThickness, false),
            BottomMaterial = new(materialName, bottomThickness, false),
            Width = boxWidth,
            Depth = boxDepth
        }.Build();

        var construction = new DoweledDrawerBoxConstructionBuilder {
            BottomDadoDepth = dadoDepth,
            BottomUndersize = bottomUndersize,
            DowelPositionsByHeight = DoweledDrawerBox.Construction.DowelPositionsByHeight
        }.Build();

        var expectedBottomWidth = boxDepth - frontThickness - backThickness + 2 * dadoDepth - bottomUndersize;
        var expectedBottomDepth = boxWidth - 2 * sideThickness + 2 * dadoDepth - bottomUndersize;

        // Act
        var part = box.GetBottomPart(construction, 1, "", "");

        // Assert
        part.Width.Should().Be(expectedBottomWidth.AsMillimeters());
        part.Length.Should().Be(expectedBottomDepth.AsMillimeters());
        part.Material.Should().Be(new PSIMaterial(materialName, "Mela", materialName, "Mela", "PB", bottomThickness.AsMillimeters()).GetLongName());

    }

    [Fact]
    public void BoxBackFrontSize() {

        // Arrange
        var boxHeight = Dimension.FromInches(5);
        var boxWidth = Dimension.FromInches(10);
        var boxDepth = Dimension.FromInches(10);
        var frontThickness = Dimension.FromMillimeters(20);
        var backThickness = Dimension.FromMillimeters(20);
        var sideThickness = Dimension.FromMillimeters(20);
        var materialName = "Material Name";

        var dadoDepth = Dimension.FromMillimeters(10);
        var bottomUndersize = Dimension.FromMillimeters(1);

        var box = new DoweledDrawerBoxBuilder {
            SideMaterial = new("", sideThickness, false),
            FrontMaterial = new(materialName, frontThickness, false),
            BackMaterial = new(materialName, backThickness, false),
            BottomMaterial = new("", Dimension.Zero, false),
            Height = boxHeight,
            Width = boxWidth,
            Depth = boxDepth
        }.Build();

        var construction = new DoweledDrawerBoxConstructionBuilder {
            BottomDadoDepth = dadoDepth,
            BottomUndersize = bottomUndersize,
            DowelPositionsByHeight = DoweledDrawerBox.Construction.DowelPositionsByHeight
        }.Build();

        var expectedFrontWidth = boxHeight;
        var expectedFrontLength = boxWidth - 2 * sideThickness;

        // Act
        var part = box.GetFrontPart(construction, 1, "", "");

        // Assert
        part.Width.Should().Be(expectedFrontWidth.AsMillimeters());
        part.Length.Should().Be(expectedFrontLength.AsMillimeters());
        part.Material.Should().Be(new PSIMaterial(materialName, "Mela", materialName, "Mela", "PB", frontThickness.AsMillimeters()).GetLongName());

    }

    [Fact]
    public void BoxSideSize() {

        // Arrange
        var boxWidth = Dimension.FromInches(10);
        var boxDepth = Dimension.FromInches(10);
        var boxHeight = Dimension.FromInches(5);
        var frontThickness = Dimension.FromMillimeters(20);
        var backThickness = Dimension.FromMillimeters(20);
        var sideThickness = Dimension.FromMillimeters(20);
        var materialName = "Material Name";

        var dadoDepth = Dimension.FromMillimeters(10);
        var bottomUndersize = Dimension.FromMillimeters(1);

        var box = new DoweledDrawerBoxBuilder {
            SideMaterial = new(materialName, sideThickness, false),
            FrontMaterial = new("", frontThickness, false),
            BackMaterial = new("", backThickness, false),
            BottomMaterial = new("", Dimension.Zero, false),
            Height = boxHeight,
            Width = boxWidth,
            Depth = boxDepth
        }.Build();

        var construction = new DoweledDrawerBoxConstructionBuilder {
            BottomDadoDepth = dadoDepth,
            BottomUndersize = bottomUndersize,
            DowelPositionsByHeight = DoweledDrawerBox.Construction.DowelPositionsByHeight
        }.Build();

        var expectedSideWidth = boxHeight;
        var expectedSideLength = boxDepth;

        // Act
        var (left, right) = box.GetSideParts(construction, 1, "", "");

        // Assert
        left.Width.Should().Be(expectedSideWidth.AsMillimeters());
        left.Length.Should().Be(expectedSideLength.AsMillimeters());
        left.Material.Should().Be(new PSIMaterial(materialName, "Mela", materialName, "Mela", "PB", sideThickness.AsMillimeters()).GetLongName());

        left.Width.Should().Be(right.Width);
        left.Length.Should().Be(right.Length);
        left.Material.Should().Be(right.Material);

    }

}
