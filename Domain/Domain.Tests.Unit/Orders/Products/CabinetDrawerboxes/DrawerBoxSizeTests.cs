using Domain.Orders.Builders;
using Domain.Orders.Enums;
using Domain.ValueObjects;
using FluentAssertions;

namespace Domain.Tests.Unit.Orders.Products.CabinetDrawerboxes;

[Collection("DrawerBoxBuilder")]
public class DrawerBoxSizeTests {

    [Theory]
    [InlineData(150, false, 100)]
    [InlineData(300, false, 200)]
    [InlineData(310, false, 300)]
    [InlineData(150, true, 100)]
    [InlineData(300, true, 200)]
    [InlineData(310, true, 200)]
    public void Should_ReturnLargestSizeUMDrawerBoxThatCanFitInCabinet(double innerCabDepthMM, bool isRollOut, double expectedDepthMM) {

        // Arrange
        DovetailDrawerBoxBuilder.CabinetUnderMountDrawerSlideBoxDepths = new Dimension[] {
            Dimension.FromMillimeters(100),
            Dimension.FromMillimeters(200),
            Dimension.FromMillimeters(300),
            Dimension.FromMillimeters(400)
        };

        DovetailDrawerBoxBuilder.DrawerSlideDepthClearance = new() {
            { DrawerSlideType.UnderMount, Dimension.FromMillimeters(10) }
        };

        DovetailDrawerBoxBuilder.RollOutSetBack = Dimension.FromMillimeters(10);

        // Act
        Dimension actual = DovetailDrawerBoxBuilder.GetDrawerBoxDepthFromInnerCabinetDepth(Dimension.FromMillimeters(innerCabDepthMM), DrawerSlideType.UnderMount, isRollOut);

        // Assert
        actual.Should().BeEquivalentTo(Dimension.FromMillimeters(expectedDepthMM));

    }

    [Fact]
    public void Should_ThrowException_IfNoUnderMountSlideFitsInCabinet() {

        // Arrange
        DovetailDrawerBoxBuilder.CabinetUnderMountDrawerSlideBoxDepths = new Dimension[] {
            Dimension.FromMillimeters(200),
            Dimension.FromMillimeters(300),
        };

        DovetailDrawerBoxBuilder.DrawerSlideDepthClearance = new() {
            { DrawerSlideType.UnderMount, Dimension.FromMillimeters(10) }
        };

        DovetailDrawerBoxBuilder.RollOutSetBack = Dimension.FromMillimeters(10);

        Dimension innerDepth = Dimension.FromMillimeters(100);

        // Act
        Func<Dimension> function = () => DovetailDrawerBoxBuilder.GetDrawerBoxDepthFromInnerCabinetDepth(innerDepth, DrawerSlideType.UnderMount, false);

        // Assert
        function.Should().Throw<ArgumentOutOfRangeException>();

    }

    [Theory]
    [InlineData(578, false, 558.8)]
    [InlineData(628, false, 609.6)]
    public void Should_ReturnSideMountSizedDrawerBox(double innerCabDepthMM, bool isRollOut, double expectedDepthMM) {

        // Arrange
        DovetailDrawerBoxBuilder.DrawerSlideDepthClearance = new() {
            { DrawerSlideType.SideMount, Dimension.FromMillimeters(10) }
        };

        DovetailDrawerBoxBuilder.RollOutSetBack = Dimension.FromMillimeters(10);

        // Act
        Dimension actual = DovetailDrawerBoxBuilder.GetDrawerBoxDepthFromInnerCabinetDepth(Dimension.FromMillimeters(innerCabDepthMM), DrawerSlideType.SideMount, isRollOut);

        // Assert
        actual.AsMillimeters().Should().BeApproximately(expectedDepthMM, 1);

    }

    [Theory]
    [InlineData(157, 105)]
    [InlineData(200, 159)]
    [InlineData(305, 260)]
    public void Should_ReturnCorrectDrawerHeight_ForDrawerFace(double drawerFaceHeightMM, double expectedHeightMM) {

        // Arrange
        DovetailDrawerBoxBuilder.StdHeights = new() {
                Dimension.FromMillimeters(57),
                Dimension.FromMillimeters(64),
                Dimension.FromMillimeters(86),
                Dimension.FromMillimeters(105),
                Dimension.FromMillimeters(137),
                Dimension.FromMillimeters(159),
                Dimension.FromMillimeters(181),
                Dimension.FromMillimeters(210),
                Dimension.FromMillimeters(260),
            };

        DovetailDrawerBoxBuilder.VerticalClearance = Dimension.FromMillimeters(41);

        // Act
        Dimension actual = DovetailDrawerBoxBuilder.GetDrawerBoxHeightFromDrawerFaceHeight(Dimension.FromMillimeters(drawerFaceHeightMM));

        // Assert
        actual.Should().BeEquivalentTo(Dimension.FromMillimeters(expectedHeightMM));

    }

    [Fact]
    public void Should_ThrowException_WhenDrawerFaceIsTooSmall() {

        // Arrange
        DovetailDrawerBoxBuilder.StdHeights = new() {
                Dimension.FromMillimeters(57),
                Dimension.FromMillimeters(64),
                Dimension.FromMillimeters(86),
                Dimension.FromMillimeters(105),
                Dimension.FromMillimeters(137),
                Dimension.FromMillimeters(159),
                Dimension.FromMillimeters(181),
                Dimension.FromMillimeters(210),
                Dimension.FromMillimeters(260),
            };

        DovetailDrawerBoxBuilder.VerticalClearance = Dimension.FromMillimeters(41);

        // Act
        Func<Dimension> function = () => DovetailDrawerBoxBuilder.GetDrawerBoxHeightFromDrawerFaceHeight(Dimension.FromMillimeters(50));

        // Assert
        function.Should().Throw<ArgumentOutOfRangeException>();

    }

    [Theory]
    [InlineData(462, 1, 452)]
    [InlineData(512, 1, 502)]
    public void Should_ReturnValidUMDrawerBoxWidth_ForGivenInnerCabinetWidth(double innerCabWidthMM, int adjacentDrawerCount, double exceptedWidthMM) {

        // Arrange 
        DovetailDrawerBoxBuilder.DividerThickness = Dimension.FromMillimeters(19);
        DovetailDrawerBoxBuilder.DrawerSlideWidthAdjustments = new() {
            { DrawerSlideType.UnderMount, Dimension.FromMillimeters(10) }
        };

        // Act
        Dimension actual = DovetailDrawerBoxBuilder.GetDrawerBoxWidthFromInnerCabinetWidth(Dimension.FromMillimeters(innerCabWidthMM), adjacentDrawerCount, DrawerSlideType.UnderMount);

        // Assert
        actual.AsMillimeters().Should().BeApproximately(exceptedWidthMM, 1);


    }

    [Theory]
    [InlineData(462, 1, 436)]
    [InlineData(512, 1, 486)]
    public void Should_ReturnValidSMDrawerBoxWidth_ForGivenInnerCabinetWidth(double innerCabWidthMM, int adjacentDrawerCount, double exceptedWidthMM) {

        // Arrange 
        DovetailDrawerBoxBuilder.DividerThickness = Dimension.FromMillimeters(19);
        DovetailDrawerBoxBuilder.DrawerSlideWidthAdjustments = new() {
            { DrawerSlideType.SideMount, Dimension.FromMillimeters(26) }
        };

        // Act
        Dimension actual = DovetailDrawerBoxBuilder.GetDrawerBoxWidthFromInnerCabinetWidth(Dimension.FromMillimeters(innerCabWidthMM), adjacentDrawerCount, DrawerSlideType.SideMount);

        // Assert
        actual.AsMillimeters().Should().BeApproximately(exceptedWidthMM, 1);


    }

}
