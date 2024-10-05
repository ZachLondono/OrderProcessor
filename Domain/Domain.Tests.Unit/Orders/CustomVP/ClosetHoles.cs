using Domain.ValueObjects;
using FluentAssertions;
using static Domain.Orders.Entities.Products.Closets.CustomDrilledVerticalPanel;

namespace Domain.Tests.Unit.Orders.CustomVP;

public class ClosetHoles {

    [Theory]
    [InlineData(2131, 0, 2121.5)]
    [InlineData(2131, 5, 2121.5)]
    [InlineData(2131, 10, 2121.5)]
    [InlineData(2131, 20, 2121.5)]
    [InlineData(2131, 30, 2121.5)]
    [InlineData(2131, 40, 2089.5)]
    [InlineData(2131, 50, 2089.5)]
    [InlineData(2131, 41.5, 2089.5)]
    public void GetValidHolePositionFromTop_ShouldReturnLowestHoleThatIsNoLowerThanDistanceFromTop(double panelLength, double distanceFromTop, double expectedHolePosition) {

        // Arrange
        Dimension distance = Dimension.FromMillimeters(distanceFromTop);
        Dimension length = Dimension.FromMillimeters(panelLength);

        // Act
        var result = GetValidHolePositionFromTop(length, distance);

        // Assert
        result.AsMillimeters().Should().Be(expectedHolePosition);

    }

    [Theory]
    [InlineData(2131, 5, 9.5)]
    [InlineData(2131, 10, 9.5)]
    [InlineData(2131, 20, 9.5)]
    [InlineData(2131, 30, 9.5)]
    [InlineData(2131, 40, 41.5)]
    [InlineData(2131, 50, 41.5)]
    [InlineData(2131, 41.5, 41.5)]
    public void GetValidHolePositionFromBottom_ShouldReturnHighestHoleThatIsNoHigherThanDistanceFromBottom(double panelLength, double distanceFromBottom, double expectedHolePosition) {

        // Arrange
        Dimension distance = Dimension.FromMillimeters(distanceFromBottom);
        Dimension length = Dimension.FromMillimeters(panelLength);

        // Act
        Dimension result = GetValidHolePositionFromBottom(length, distance);

        // Assert
        result.AsMillimeters().Should().Be(expectedHolePosition);

    }

    [Fact]
    public void GetDrillingOperations_ShouldContainOnlyOneDrillingOperation_WhenAllValuesAreZero() {

        // Arrange
        var sut = new CustomVPBuilder() {
            Width = Dimension.FromInches(14),
            Length = Dimension.FromMillimeters(2131),
            HoleDimensionFromBottom = Dimension.Zero,
            HoleDimensionFromTop = Dimension.Zero,
            TransitionHoleDimensionFromBottom = Dimension.Zero,
            TransitionHoleDimensionFromTop = Dimension.Zero,
        }.Build();

        // Act
        var operations = sut.GetDrillingOperations();

        // Assert
        operations.Should().HaveCount(1);
        operations.Should().Contain(
            new VPDrillingOperation(sut.Length - Dimension.FromMillimeters(9.5),
                                    Dimension.FromMillimeters(9.5),
                                    VPDrillingDepth.Stopped)
        );

    }

    [Fact]
    public void GetDrillingOperations_ShouldContainTwoDrillingOperations_WhenDrillingFromTopAndBottomAreSet() {

        // Arrange
        var sut = new CustomVPBuilder() {
            Width = Dimension.FromInches(14),
            Length = Dimension.FromMillimeters(2131),
            HoleDimensionFromBottom = Dimension.FromInches(2.875),
            HoleDimensionFromTop = Dimension.FromInches(12),
            TransitionHoleDimensionFromBottom = Dimension.Zero,
            TransitionHoleDimensionFromTop = Dimension.Zero,
        }.Build();

        // Act
        var operations = sut.GetDrillingOperations();

        // Assert
        operations.Should().HaveCount(2);
        operations.Should().Contain(
            new VPDrillingOperation(Dimension.FromMillimeters(73.5),
                                    Dimension.FromMillimeters(9.5),
                                    VPDrillingDepth.Stopped)
        );
        operations.Should().Contain(
            new VPDrillingOperation(sut.Length - Dimension.FromMillimeters(9.5),
                                    sut.Length - Dimension.FromMillimeters(297.5),
                                    VPDrillingDepth.Stopped)
        );

    }

    [Fact]
    public void GetDrillingOperations_ShouldContainOneDrillingOperation_WhenDrillingOnlyFromTopIsSet() {

        // Arrange
        var sut = new CustomVPBuilder() {
            Width = Dimension.FromInches(14),
            Length = Dimension.FromInches(80),
            HoleDimensionFromBottom = Dimension.FromInches(0),
            HoleDimensionFromTop = Dimension.FromInches(12),
            TransitionHoleDimensionFromBottom = Dimension.Zero,
            TransitionHoleDimensionFromTop = Dimension.Zero,
        }.Build();

        // Act
        var operations = sut.GetDrillingOperations();

        // Assert
        operations.Should().HaveCount(1);
        operations.Should().Contain(
            new VPDrillingOperation(sut.Length - Dimension.FromMillimeters(9.5),
                                    sut.Length - Dimension.FromMillimeters(297.5),
                                    VPDrillingDepth.Stopped)
        );

    }

    [Fact]
    public void GetDrillingOperations_ShouldContainThreeDrillingOperations_WhenPanelIsFullyDrilledWithTransitionHolesOnTopAndBottom() {

        // Arrange
        var sut = new CustomVPBuilder() {
            Width = Dimension.FromInches(14),
            Length = Dimension.FromMillimeters(2131),
            HoleDimensionFromBottom = Dimension.Zero,
            HoleDimensionFromTop = Dimension.Zero,
            TransitionHoleDimensionFromBottom = Dimension.FromInches(2.875),
            TransitionHoleDimensionFromTop = Dimension.FromInches(12),
        }.Build();

        // Act
        var operations = sut.GetDrillingOperations();

        // Assert
        operations.Should().HaveCount(3);
        operations.Should().Contain(
            new VPDrillingOperation(Dimension.FromMillimeters(73.5),
                                    Dimension.FromMillimeters(9.5),
                                    VPDrillingDepth.Through)
        );
        operations.Should().Contain(
            new VPDrillingOperation(sut.Length - Dimension.FromMillimeters(9.5),
                                    sut.Length - Dimension.FromMillimeters(297.5),
                                    VPDrillingDepth.Through)
        );
        operations.Should().Contain(
            new VPDrillingOperation(sut.Length - Dimension.FromMillimeters(297.5),
                                    Dimension.FromMillimeters(73.5),
                                    VPDrillingDepth.Stopped)
        );

    }


    [Fact]
    public void GetDrillingOperations_ShouldContainFourDrillingOperations_WhenPanelIsPartiallyDrilledWithTransitionHolesOnTopAndBottom() {

        // Arrange
        var sut = new CustomVPBuilder() {
            Width = Dimension.FromInches(14),
            Length = Dimension.FromMillimeters(2131),
            HoleDimensionFromBottom = Dimension.FromInches(8),
            HoleDimensionFromTop = Dimension.FromInches(15),
            TransitionHoleDimensionFromBottom = Dimension.FromInches(2.875),
            TransitionHoleDimensionFromTop = Dimension.FromInches(12),
        }.Build();

        // Act
        var operations = sut.GetDrillingOperations();

        // Assert
        operations.Should().HaveCount(4);
        operations.Should().Contain(
            new VPDrillingOperation(Dimension.FromMillimeters(73.5),
                                    Dimension.FromMillimeters(9.5),
                                    VPDrillingDepth.Through)
        );
        operations.Should().Contain(
            new VPDrillingOperation(sut.Length - Dimension.FromMillimeters(9.5),
                                    sut.Length - Dimension.FromMillimeters(297.5),
                                    VPDrillingDepth.Through)
        );
        operations.Should().Contain(
            new VPDrillingOperation(sut.Length - Dimension.FromMillimeters(297.5 + 32),
                                    sut.Length - Dimension.FromMillimeters(361.5),
                                    VPDrillingDepth.Stopped)
        );
        operations.Should().Contain(
            new VPDrillingOperation(Dimension.FromMillimeters(201.5),
                                    Dimension.FromMillimeters(73.5 + 32),
                                    VPDrillingDepth.Stopped)
        );

    }



}
