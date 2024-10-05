using Domain.ValueObjects;
using FluentAssertions;

namespace Domain.Tests.Unit.ValueObjects;

public class AreaTests {

    [Fact]
    public void FromSquareMillimeters_ShouldThrowException_WhenValueIsOutOfRange() {

        // Arrange
        var action = () => Area.FromSquareMillimeters(-1);

        // Assert
        action.Should().Throw<ArgumentOutOfRangeException>();

    }

    [Fact]
    public void FromSquareInches_ShouldThrowException_WhenValueIsOutOfRange() {

        // Arrange
        var action = () => Area.FromSquareInches(-1);

        // Assert
        action.Should().Throw<ArgumentOutOfRangeException>();

    }

    [Fact]
    public void Sqrt_ShouldReturnCorrectDimension() {

        // Arrange
        var area = Area.FromSquareMillimeters(9);

        // Act
        Dimension result = Area.Sqrt(area);

        // Assert
        result.AsMillimeters().Should().Be(3);

    }

    [Fact]
    public void Addition_ShouldReturnCorrectArea() {

        // Arrange
        var area1 = Area.FromSquareInches(9);
        var area2 = Area.FromSquareInches(9);

        // Act
        var result = area1 + area2;

        // Assert
        result.AsSquareInches().Should().Be(18);

    }

}