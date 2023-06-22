using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Shared.Domain;
using FluentAssertions;

namespace ApplicationCore.Tests.Unit.Orders.Products.CustomVP;

public class ClosetHoles {

    [Theory]
    [InlineData(2131, 0, 0)]
    [InlineData(2131, 10, 2121.5)]
    [InlineData(2131, 20, 2121.5)]
    [InlineData(2131, 30, 2121.5)]
    [InlineData(2131, 40, 2121.5)]
    [InlineData(2131, 50, 2089.5)]
    [InlineData(2131, 41.5, 2089.5)]
    public void GetValidHolePositionFromTop_ShouldReturnLowestHoleThatIsNoLowerThanDistanceFromTop(double panelLength, double distanceFromTop, double expectedHolePosition) {

        // Arrange
        Dimension distance = Dimension.FromMillimeters(distanceFromTop);
        Dimension length = Dimension.FromMillimeters(panelLength);

        // Act
        var result = CustomDrilledVerticalPanel.GetValidHolePositionFromTop(length, distance);

        // Assert
        result.AsMillimeters().Should().Be(expectedHolePosition);

    }

    [Theory]
    [InlineData(2131, 5, 0)]
    [InlineData(2131, 10, 9.5)]
    [InlineData(2131, 20, 9.5)]
    [InlineData(2131, 30, 9.5)]
    [InlineData(2131, 40, 9.5)]
    [InlineData(2131, 50, 41.5)]
    [InlineData(2131, 41.5, 41.5)]
    public void GetValidHolePositionFromBottom_ShouldReturnHighestHoleThatIsNoHigherThanDistanceFromBottom(double panelLength, double distanceFromBottom, double expectedHolePosition) {

        // Arrange
        Dimension distance = Dimension.FromMillimeters(distanceFromBottom);
        Dimension length = Dimension.FromMillimeters(panelLength);

        // Act
        Dimension result = CustomDrilledVerticalPanel.GetValidHolePositionFromBottom(length, distance);

        // Assert
        result.AsMillimeters().Should().Be(expectedHolePosition);

    }

}
