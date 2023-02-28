using ApplicationCore.Features.Orders.Loader.Providers.AllmoxyXMLModels;
using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using FluentAssertions;
using NSubstitute;

namespace ApplicationCore.Tests.Unit.Orders.AllmoxyOrderLoading;

public class AllmoxyCabinetMappingTests {

    private readonly CabinetModelBase _sut = Substitute.For<CabinetModelBase>();

    public AllmoxyCabinetMappingTests() {
        _sut.LineNumber = 1;
        _sut.GroupNumber = 1;
        _sut.Cabinet = new() {
            BoxMaterial = new() {
                Finish = "",
                Type = ""
            },
            FinishMaterial = new() {
                Finish = "",
                Type = ""
            },
            Fronts = new() {
                Color = "",
                Style = "",
                Type = ""
            },
            EdgeBandColor = "",
            Assembled = "",
            Qty = 1,
            UnitPrice = "",
            LeftSide = "",
            RightSide = "",
            Room = "",
            Height = 1,
            Width = 1,
            Depth = 1
        };
    }

    [Theory]
    [InlineData("", 0)]
    [InlineData("1", 1)]
    [InlineData("1000", 1000)]
    [InlineData("$1,000", 1000)]
    public void UnitPriceTest(string input, decimal expected) {

        // Arrange
        _sut.Cabinet.UnitPrice = input;

        // Act
        var builder = _sut.InitilizeBuilder<TestBuilder, Cabinet>(new());

        // Assert
        builder.UnitPrice.Should().Be(expected);

    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void QuantityTest(int input) {

        // Arrange
        _sut.Cabinet.Qty = input;

        // Act
        var builder = _sut.InitilizeBuilder<TestBuilder, Cabinet>(new());

        // Assert
        builder.Qty.Should().Be(input);

    }

    [Fact]
    public void DimensionTests() {

        // Arrange
        _sut.Cabinet.Height = 123;
        _sut.Cabinet.Width = 456;
        _sut.Cabinet.Depth = 789;

        // Act
        var builder = _sut.InitilizeBuilder<TestBuilder, Cabinet>(new());

        // Assert
        builder.Height.AsMillimeters().Should().Be(_sut.Cabinet.Height);
        builder.Width.AsMillimeters().Should().Be(_sut.Cabinet.Width);
        builder.Depth.AsMillimeters().Should().Be(_sut.Cabinet.Depth);

    }

    [Theory]
    [InlineData("Yes", true)]
    [InlineData("No", false)]
    [InlineData("Random", false)]
    [InlineData("yes", false)]
    public void AssembledTests(string input, bool expected) {

        // Arrange
        _sut.Cabinet.Assembled = input;

        // Act
        var builder = _sut.InitilizeBuilder<TestBuilder, Cabinet>(new());

        // Assert
        builder.Assembled.Should().Be(expected);

    }

    [Fact]
    public void RoomTests() {

        // Arrange
        _sut.Cabinet.Room = "Test Text";

        // Act
        var builder = _sut.InitilizeBuilder<TestBuilder, Cabinet>(new());

        // Assert
        builder.Room.Should().Be(_sut.Cabinet.Room);

    }

    [Fact]
    public void EdgeBandingTests() {

        // Arrange
        _sut.Cabinet.EdgeBandColor = "Test Text";

        // Act
        var builder = _sut.InitilizeBuilder<TestBuilder, Cabinet>(new());

        // Assert
        builder.EdgeBandingColor.Should().Be(_sut.Cabinet.EdgeBandColor);

    }

    [Theory]
    [InlineData("Unfinished", CabinetSideType.Unfinished)]
    [InlineData("Finished", CabinetSideType.Finished)]
    [InlineData("Integrated", CabinetSideType.IntegratedPanel)]
    [InlineData("Applied", CabinetSideType.AppliedPanel)]
    public void CabinetSideTests(string input, CabinetSideType expected) {

        // Arrange
        _sut.Cabinet.LeftSide = input;
        _sut.Cabinet.RightSide = input;

        // Act
        var builder = _sut.InitilizeBuilder<TestBuilder, Cabinet>(new());

        // Assert
        builder.LeftSideType.Should().Be(expected);

        builder.RightSideType.Should().Be(expected);

    }


    [Theory]
    [InlineData("pb", "pb", CabinetMaterialCore.Flake, CabinetMaterialCore.Flake)]
    [InlineData("ply", "ply", CabinetMaterialCore.Plywood, CabinetMaterialCore.Plywood)]
    [InlineData("pb", "paint", CabinetMaterialCore.Flake, CabinetMaterialCore.Flake)]
    [InlineData("ply", "paint", CabinetMaterialCore.Plywood, CabinetMaterialCore.Plywood)]
    public void MaterialCoreTests(string boxCore, string finishCore, CabinetMaterialCore expectedBoxCore, CabinetMaterialCore expectedFinishCore) {

        // Arrange
        _sut.Cabinet.BoxMaterial = new() {
            Type = boxCore
        };
        _sut.Cabinet.FinishMaterial = new() {
            Type = finishCore
        };

        // Act
        var builder = _sut.InitilizeBuilder<TestBuilder, Cabinet>(new());

        // Assert
        builder.BoxMaterial.Core.Should().Be(expectedBoxCore);

        builder.FinishMaterial.Core.Should().Be(expectedFinishCore);

    }

    [Fact]
    public void MaterialColorTests() {

        // Arrange
        _sut.Cabinet.BoxMaterial = new() {
            Finish = "Finish1"
        };
        _sut.Cabinet.FinishMaterial = new() {
            Finish = "Finish2"
        };

        // Act
        var builder = _sut.InitilizeBuilder<TestBuilder, Cabinet>(new());

        // Assert
        builder.BoxMaterial.Finish.Should().Be(_sut.Cabinet.BoxMaterial.Finish);
        builder.FinishMaterial.Finish.Should().Be(_sut.Cabinet.FinishMaterial.Finish);

    }

    [Fact]
    public void FinshAndBoxColor_ShouldMatch_WhenFinishTypeIsPaint() {

        // Arrange
        _sut.Cabinet.BoxMaterial = new() {
            Finish = "Finish1",
            Type = "pb"
        };
        _sut.Cabinet.FinishMaterial = new() {
            Finish = "2hsiniF",
            Type = "paint"
        };

        // Act
        var builder = _sut.InitilizeBuilder<TestBuilder, Cabinet>(new());

        // Assert
        string color = _sut.Cabinet.BoxMaterial.Finish;
        builder.BoxMaterial.Finish.Should().Be(color);
        builder.FinishMaterial.Finish.Should().Be(color);

    }

    [Fact]
    public void EdgeBandingColor_ShouldMatchFinish_WhenEdgeBandingColorIsMatchFinishAndFinishTypeIsNotPaint() {

        // Arrange
        _sut.Cabinet.EdgeBandColor = "Match Finish";
        _sut.Cabinet.FinishMaterial = new() {
            Finish = "Finish",
            Type = "not paint"
        };

        // Act
        var builder = _sut.InitilizeBuilder<TestBuilder, Cabinet>(new());

        // Assert
        builder.EdgeBandingColor.Should().Be(_sut.Cabinet.FinishMaterial.Finish);

    }

    [Fact]
    public void EdgeBandingColor_ShouldMatchBox_WhenEdgeBandingColorIsMatchFinishAndFinishTypeIsPaint() {

        // Arrange
        _sut.Cabinet.EdgeBandColor = "Match Finish";
        _sut.Cabinet.BoxMaterial = new() {
            Finish = "Finish1",
            Type = "pb"
        };
        _sut.Cabinet.FinishMaterial = new() {
            Finish = "Finish2",
            Type = "paint"
        };

        // Act
        var builder = _sut.InitilizeBuilder<TestBuilder, Cabinet>(new());

        // Assert
        builder.EdgeBandingColor.Should().Be(_sut.Cabinet.BoxMaterial.Finish);

    }

    [Fact]
    public void EdgeBandingColor_ShouldEqualEdgeBandingColor() {

        // Arrange
        _sut.Cabinet.EdgeBandColor = "Do Not Match Finish";
        _sut.Cabinet.FinishMaterial = new() {
            Finish = "Finish",
            Type = "paint"
        };

        // Act
        var builder = _sut.InitilizeBuilder<TestBuilder, Cabinet>(new());

        // Assert
        builder.EdgeBandingColor.Should().Be(_sut.Cabinet.EdgeBandColor);

    }

    class TestBuilder : CabinetBuilder<Cabinet> {
        public override Cabinet Build() => throw new NotImplementedException();
    }

}