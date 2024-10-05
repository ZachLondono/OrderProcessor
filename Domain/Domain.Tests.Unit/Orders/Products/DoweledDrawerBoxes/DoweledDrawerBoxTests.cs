using CADCodeProxy.Machining.Tokens;
using Domain.Orders.Components;
using Domain.ValueObjects;
using FluentAssertions;

namespace Domain.Tests.Unit.Orders.Products.DoweledDrawerBoxes;

public class DoweledDrawerBoxPartTests {

    private readonly DoweledDrawerBoxBuilder _builder;
    private readonly DoweledDrawerBoxConstructionBuilder _constructionBuilder;

    public DoweledDrawerBoxPartTests() {

        var boxWidth = Dimension.FromInches(10);
        var boxDepth = Dimension.FromInches(10);
        var frontThickness = Dimension.FromMillimeters(20);
        var backThickness = Dimension.FromMillimeters(20);
        var sideThickness = Dimension.FromMillimeters(20);
        var bottomThickness = Dimension.FromMillimeters(6.35);
        var materialName = "Material Name";

        var dadoDepth = Dimension.FromMillimeters(10);
        var bottomUndersize = Dimension.FromMillimeters(1);

        _builder = new DoweledDrawerBoxBuilder {
            SideMaterial = new("", sideThickness, false),
            FrontMaterial = new("", frontThickness, false),
            BackMaterial = new("", backThickness, false),
            BottomMaterial = new(materialName, bottomThickness, false),
            Width = boxWidth,
            Depth = boxDepth
        };

        _constructionBuilder = new DoweledDrawerBoxConstructionBuilder {
            BottomDadoDepth = dadoDepth,
            BottomUndersize = bottomUndersize,
            DowelPositionsByHeight = DoweledDrawerBox.Construction.DowelPositionsByHeight
        };

    }

    [Fact]
    public void BoxBottomSize() {

        // Arrange
        var sut = _builder.Build();
        var construction = _constructionBuilder.Build();

        var expectedBottomWidth = sut.Depth - sut.FrontMaterial.Thickness - sut.BackMaterial.Thickness + 2 * construction.BottomDadoDepth - construction.BottomUndersize;
        var expectedBottomDepth = sut.Width - 2 * sut.SideMaterial.Thickness + 2 * construction.BottomDadoDepth - construction.BottomUndersize;

        // Act
        var part = sut.GetBottomPart(construction);

        // Assert
        part.Width.Should().Be(expectedBottomWidth.AsMillimeters());
        part.Length.Should().Be(expectedBottomDepth.AsMillimeters());
        part.Material.Should().Be(new PSIMaterial(sut.BottomMaterial.Name, "Mela", sut.BottomMaterial.Name, "Mela", "PB", sut.BottomMaterial.Thickness.AsMillimeters()).GetLongName());

    }

    [Fact]
    public void GetBottomPart_ShouldChangeBottomSize_WhenBottomUndersizeChanges() {

        // Arrange
        var sut = _builder.Build();

        var construction1 = new DoweledDrawerBoxConstructionBuilder {
            BottomUndersize = Dimension.Zero
        }.Build();

        var construction2 = new DoweledDrawerBoxConstructionBuilder {
            BottomUndersize = Dimension.FromMillimeters(1.5)
        }.Build();

        // Arrange / Act
        var bottom1 = sut.GetBottomPart(construction1);
        var bottom2 = sut.GetBottomPart(construction2);

        // Assert
        var botWidthDiff = bottom2.Width - bottom1.Width;
        var expectedWidthDiff = construction1.BottomUndersize.AsMillimeters() - construction2.BottomUndersize.AsMillimeters();
        botWidthDiff.Should().Be(expectedWidthDiff);

        var botLengthDiff = bottom2.Length - bottom1.Length;
        var expectedLengthDiff = construction1.BottomUndersize.AsMillimeters() - construction2.BottomUndersize.AsMillimeters();
        botLengthDiff.Should().Be(expectedLengthDiff);

    }

    [Fact]
    public void BoxBackFrontSize() {

        // Arrange
        var sut = _builder.Build();
        var construction = _constructionBuilder.Build();

        var expectedFrontWidth = sut.Height;
        var expectedFrontLength = sut.Width - 2 * sut.SideMaterial.Thickness;

        // Act
        var front = sut.GetFrontPart(construction);
        var back = sut.GetBackPart(construction);

        // Assert
        front.Width.Should().Be(expectedFrontWidth.AsMillimeters());
        front.Length.Should().Be(expectedFrontLength.AsMillimeters());
        front.Material.Should().Be(new PSIMaterial(sut.FrontMaterial.Name, "Mela", sut.FrontMaterial.Name, "Mela", "PB", sut.FrontMaterial.Thickness.AsMillimeters()).GetLongName());

        back.Width.Should().Be(back.Width);
        back.Length.Should().Be(back.Length);
        back.Material.Should().Be(back.Material);

    }

    [Fact]
    public void GetFrontPart_ShouldChangeLength_WhenWidthUndersizeChanges() {

        // Arrange
        _constructionBuilder.WidthUnderSize = Dimension.Zero;
        var construction1 = _constructionBuilder.Build();

        _constructionBuilder.WidthUnderSize = Dimension.FromMillimeters(1.5);
        var construction2 = _constructionBuilder.Build();

        var sut = _builder.Build();

        // Arrange / Act
        var front1 = sut.GetFrontPart(construction1);
        var back1 = sut.GetFrontPart(construction1);
        var front2 = sut.GetFrontPart(construction2);
        var back2 = sut.GetFrontPart(construction2);

        // Assert
        front1.Length.Should().Be(back1.Length);
        front2.Length.Should().Be(back2.Length);

        var expectedDiff = construction1.WidthUndersize.AsMillimeters() - construction2.WidthUndersize.AsMillimeters();

        (front2.Length - front1.Length).Should().Be(expectedDiff);

    }

    [Fact]
    public void BoxSideSize() {

        // Arrange
        var sut = _builder.Build();
        var expectedSideWidth = sut.Height;
        var expectedSideLength = sut.Depth;

        // Act
        var (left, right) = sut.GetSideParts(_constructionBuilder.Build());

        // Assert
        left.Width.Should().Be(expectedSideWidth.AsMillimeters());
        left.Length.Should().Be(expectedSideLength.AsMillimeters());
        left.Material.Should().Be(new PSIMaterial(sut.SideMaterial.Name, "Mela", sut.SideMaterial.Name, "Mela", "PB", sut.SideMaterial.Thickness.AsMillimeters()).GetLongName());

        left.Width.Should().Be(right.Width);
        left.Length.Should().Be(right.Length);
        left.Material.Should().Be(right.Material);

    }

    [Fact]
    public void GetSideParts_ShouldIncludeThicknessPocket_WhenThicknessIsOverMinimum() {

        // Arrange
        var sideThickness = Dimension.FromMillimeters(19);
        var minUMThickness = Dimension.FromMillimeters(16);

        _constructionBuilder.UMSlideMaxDistanceOffOutsideFace = minUMThickness;
        var construction = _constructionBuilder.Build();

        _builder.SideMaterial = new("", sideThickness, false);
        _builder.MachineThicknessForUMSlides = true;
        var box = _builder.Build();

        // Act
        var (left, right) = box.GetSideParts(construction);

        // Assert
        left.PrimaryFace
            .Tokens
            .OfType<Pocket>()
            .Should()
            .Contain(p =>
                 p.StartDepth == (sideThickness - minUMThickness).AsMillimeters()
                 &&
                 p.ToolName == construction.UMSlidePocketToolName
            );

        right.PrimaryFace
            .Tokens
            .OfType<Pocket>()
            .Should()
            .Contain(p =>
                 p.StartDepth == (sideThickness - minUMThickness).AsMillimeters()
                 &&
                 p.ToolName == construction.UMSlidePocketToolName
            );

    }

    [Fact]
    public void GetSideParts_ShouldNotIncludeThicknessPocket_WhenThicknessIsNotOverMinimum() {

        // Arrange
        var sideThickness = Dimension.FromMillimeters(16);
        var minUMThickness = Dimension.FromMillimeters(19);

        _constructionBuilder.UMSlideMaxDistanceOffOutsideFace = minUMThickness;
        var construction = _constructionBuilder.Build();

        _builder.SideMaterial = new("", sideThickness, false);
        _builder.MachineThicknessForUMSlides = true;
        var box = _builder.Build();

        // Act
        var (left, right) = box.GetSideParts(construction);

        // Assert
        left.PrimaryFace
            .Tokens
            .OfType<Pocket>()
            .Should()
            .NotContain(p =>
                 p.StartDepth == (sideThickness - minUMThickness).AsMillimeters()
                 ||
                 p.ToolName == construction.UMSlidePocketToolName
            );

        right.PrimaryFace
            .Tokens
            .OfType<Pocket>()
            .Should()
            .NotContain(p =>
                 p.StartDepth == (sideThickness - minUMThickness).AsMillimeters()
                 ||
                 p.ToolName == construction.UMSlidePocketToolName
            );

    }

    [Fact]
    public void GetSideParts_ShouldNotIncludeThicknessPocket_WhenSettingDisabled() {

        // Arrange
        var sideThickness = Dimension.FromMillimeters(19);
        var minUMThickness = Dimension.FromMillimeters(16);

        _constructionBuilder.UMSlideMaxDistanceOffOutsideFace = minUMThickness;
        var construction = _constructionBuilder.Build();

        _builder.SideMaterial = new("", sideThickness, false);
        _builder.MachineThicknessForUMSlides = false;
        var box = _builder.Build();

        // Act
        var (left, right) = box.GetSideParts(construction);

        // Assert
        left.PrimaryFace
            .Tokens
            .OfType<Pocket>()
            .Should()
            .NotContain(p =>
                 p.StartDepth == (sideThickness - minUMThickness).AsMillimeters()
                 ||
                 p.ToolName == construction.UMSlidePocketToolName
            );

        right.PrimaryFace
            .Tokens
            .OfType<Pocket>()
            .Should()
            .NotContain(p =>
                 p.StartDepth == (sideThickness - minUMThickness).AsMillimeters()
                 ||
                 p.ToolName == construction.UMSlidePocketToolName
            );

    }

    [Fact]
    public void GetFrontPart_ShouldIncludeThicknessPocket_WhenThicknessIsOverMinimum() {

        // Arrange
        var frontThickness = Dimension.FromMillimeters(19);
        var minUMThickness = Dimension.FromMillimeters(16);

        _constructionBuilder.UMSlideMaxDistanceOffOutsideFace = minUMThickness;
        var construction = _constructionBuilder.Build();

        _builder.FrontMaterial = new("", frontThickness, false);
        _builder.MachineThicknessForUMSlides = true;
        var box = _builder.Build();

        // Act
        var front = box.GetFrontPart(construction);

        // Assert
        front.PrimaryFace
            .Tokens
            .OfType<Pocket>()
            .Should()
            .Contain(p =>
                 p.StartDepth == (frontThickness - minUMThickness).AsMillimeters()
                 &&
                 p.ToolName == construction.UMSlidePocketToolName
            );

    }

    [Fact]
    public void GetFrontPart_ShouldNotIncludeThicknessPocket_WhenThicknessIsNotOverMinimum() {

        // Arrange
        var frontThickness = Dimension.FromMillimeters(16);
        var minUMThickness = Dimension.FromMillimeters(19);

        _constructionBuilder.UMSlideMaxDistanceOffOutsideFace = minUMThickness;
        var construction = _constructionBuilder.Build();

        _builder.FrontMaterial = new("", frontThickness, false);
        _builder.MachineThicknessForUMSlides = true;
        var box = _builder.Build();

        // Act
        var front = box.GetFrontPart(construction);

        // Assert
        front.PrimaryFace
            .Tokens
            .OfType<Pocket>()
            .Should()
            .NotContain(p =>
                 p.StartDepth == (frontThickness - minUMThickness).AsMillimeters()
                 ||
                 p.ToolName == construction.UMSlidePocketToolName
            );

    }

    [Fact]
    public void GetFrontPart_ShouldNotIncludeThicknessPocket_WhenSettingDisabled() {

        // Arrange
        var frontThickness = Dimension.FromMillimeters(19);
        var minUMThickness = Dimension.FromMillimeters(16);

        _constructionBuilder.UMSlideMaxDistanceOffOutsideFace = minUMThickness;
        var construction = _constructionBuilder.Build();

        _builder.FrontMaterial = new("", frontThickness, false);
        _builder.MachineThicknessForUMSlides = false;
        var box = _builder.Build();

        // Act
        var front = box.GetFrontPart(construction);

        // Assert
        front.PrimaryFace
            .Tokens
            .OfType<Pocket>()
            .Should()
            .NotContain(p =>
                 p.StartDepth == (frontThickness - minUMThickness).AsMillimeters()
                 ||
                 p.ToolName == construction.UMSlidePocketToolName
            );

    }

}
