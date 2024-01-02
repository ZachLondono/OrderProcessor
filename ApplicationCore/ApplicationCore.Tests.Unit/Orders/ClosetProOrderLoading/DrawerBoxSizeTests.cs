using ApplicationCore.Features.AllmoxyOrderExport.Attributes;
using ApplicationCore.Shared.Domain;
using FluentAssertions;

namespace ApplicationCore.Tests.Unit.Orders.ClosetProOrderLoading;

public class DrawerBoxSizeTests {

    [Theory]
    [InlineData(3.75, "2.5")]
    [InlineData(5, "2.5")]
    [InlineData(6.25, "2.5")]
    [InlineData(7.5, "4.125")]
    [InlineData(8.75, "6")]
    [InlineData(10, "6")]
    [InlineData(11.25, "8.25")]
    [InlineData(12.5, "8.25")]
    [InlineData(13.75, "8.25")]
    [InlineData(15, "12")]
    [InlineData(16.25, "12")]
    [InlineData(17.5, "12")]
    [InlineData(18.75, "12")]
    [InlineData(20, "12")]
    [InlineData(25, "12")]
    [InlineData(50, "12")]
    public void DrawerBoxHeights(double faceHeight, string expectedHeight) {

        // Act
        var result = DrawerBoxMaterial.GetStandardHeight(Dimension.FromInches(faceHeight));

        // Assert
        result.Should().Be(expectedHeight);

    }

    [Theory]
    [InlineData(12, 12)]
    [InlineData(14, 14)]
    [InlineData(16, 16)]
    [InlineData(18, 18)]
    [InlineData(20, 18)]
    [InlineData(22, 21)]
    [InlineData(24, 21)]
    [InlineData(26, 21)]
    [InlineData(28, 21)]
    [InlineData(30, 21)]
    [InlineData(32, 21)]
    [InlineData(34, 21)]
    [InlineData(36, 21)]
    [InlineData(38, 21)]
    public void DrawerBoxDepths(double openingDepth, double expectedDepth) {

        // Act
        var result = DrawerBoxMaterial.GetBoxDepthFromOpening(Dimension.FromInches(openingDepth));

        // Assert
        result.Should().Be(Dimension.FromInches(expectedDepth));

    }



}
