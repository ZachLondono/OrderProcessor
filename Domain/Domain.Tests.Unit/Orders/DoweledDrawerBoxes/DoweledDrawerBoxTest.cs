using Domain.Orders.Components;
using Domain.ValueObjects;
using FluentAssertions;

namespace Domain.Tests.Unit.Orders.DoweledDrawerBoxes;

public class DoweledDrawerBoxTest {

    [Theory]
    [InlineData(3, 0)]
    [InlineData(4, 0)]
    [InlineData(4.5, 1)]
    [InlineData(5, 1)]
    [InlineData(5.5, 2)]
    [InlineData(6, 2)]
    public void GetDowelPositions_Should_ReturnCorrectPositionArray_WhenHeightIsValid(double height, int expectedArray) {

        // Arrange
        var arrays = new Dimension[][] {
            new Dimension[] { Dimension.FromInches(1) },
            new Dimension[] { Dimension.FromInches(2) },
            new Dimension[] { Dimension.FromInches(3) },
        };

        var positionMap = new Dictionary<Dimension, Dimension[]>() {
            { Dimension.FromInches(4), arrays[0] },
            { Dimension.FromInches(5), arrays[1] },
            { Dimension.FromInches(6), arrays[2] },
        };

        var boxHeight = Dimension.FromInches(height);

        // Act
        var positions = DoweledDrawerBox.GetDowelPositions(positionMap, boxHeight);

        // Assert
        positions.Should().BeEquivalentTo(arrays[expectedArray]);

    }

    [Fact]
    public void GetDowelPositions_Should_ThrowException_WhenHeightIsInvalid() {

        // Arrange
        var arrays = new Dimension[][] {
            new Dimension[] { Dimension.FromInches(1) },
            new Dimension[] { Dimension.FromInches(2) },
            new Dimension[] { Dimension.FromInches(3) },
        };

        var positionMap = new Dictionary<Dimension, Dimension[]>() {
            { Dimension.FromInches(4), arrays[0] },
            { Dimension.FromInches(5), arrays[1] },
            { Dimension.FromInches(6), arrays[2] },
        };

        var boxHeight = Dimension.FromInches(7);

        // Act
        var action = () => DoweledDrawerBox.GetDowelPositions(positionMap, boxHeight);

        // Assert
        action.Should().Throw<InvalidOperationException>();

    }


}