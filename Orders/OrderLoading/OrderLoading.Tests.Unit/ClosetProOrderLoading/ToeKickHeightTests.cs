using Domain.Companies.ValueObjects;
using Domain.Orders.Entities.Products.Closets;
using Domain.ValueObjects;
using FluentAssertions;
using OrderLoading.ClosetProCSVCutList.Products;

namespace OrderLoading.Tests.Unit.ClosetProOrderLoading;

public class ToeKickHeightTests {

    [Theory]
    [InlineData(95.25, 2, 96)]
    [InlineData(63.5, 2, 64)]
    public void ShouldConvertHeightToCompliantHeight_WhenErrorIsWithinTolerances(double inputMM, double errorMM, double expectedMM) {

        var didWork = MiscellaneousClosetPart.TryGetNearest32MMCompliantToeKickHeight(Dimension.FromMillimeters(inputMM), out Dimension output, errorMM);

        didWork.Should().BeTrue();
        output.AsMillimeters().Should().Be(expectedMM);

    }

    [Theory]
    [InlineData(92, 2)]
    [InlineData(60, 2)]
    public void ShouldNotConvertHeightToCompliantHeight_WhenErrorIsNotWithinTolerances(double inputMM, double errorMM) {

        var didWork = MiscellaneousClosetPart.TryGetNearest32MMCompliantToeKickHeight(Dimension.FromMillimeters(inputMM), out _, errorMM);

        didWork.Should().BeFalse();

    }

    [Fact]
    public void ShouldConvertHeight_WhenPartIsToeKick() {

        // Arrange
        var part = new MiscellaneousClosetPart() {
            Type = MiscellaneousType.ToeKick,
            Width = Dimension.FromMillimeters(95.25),
            Length = Dimension.FromInches(20),
            Color = "White",
            EdgeBandingColor = "White",
            Room = "Room",
            UnitPrice = 10,
            PartNumber = 1,
            Qty = 1
        };

        // Act
        var product = part.ToProduct(new ClosetProSettings());

        // Assert
        ((ClosetPart) product).Width.AsMillimeters().Should().Be(96);

    }

    [Fact]
    public void ShouldNotConvertHeight_WhenPartIsToeKick() {

        // Arrange
        var part = new MiscellaneousClosetPart() {
            Type = MiscellaneousType.Backing,
            Width = Dimension.FromMillimeters(95.25),
            Length = Dimension.FromInches(20),
            Color = "White",
            EdgeBandingColor = "White",
            Room = "Room",
            UnitPrice = 10,
            PartNumber = 1,
            Qty = 1
        };

        // Act
        var product = part.ToProduct(new ClosetProSettings());

        // Assert
        ((ClosetPart) product).Width.AsMillimeters().Should().Be(95.25);

    }

}
