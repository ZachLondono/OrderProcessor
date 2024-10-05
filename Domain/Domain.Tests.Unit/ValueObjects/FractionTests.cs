using Domain.ValueObjects;
using FluentAssertions;

namespace Domain.Tests.Unit.ValueObjects;

public class FractionTests {

    [Theory]
    [InlineData(1, 2, "1/2")]
    [InlineData(2, 2, "1")]
    [InlineData(3, 2, "1 1/2")]
    [InlineData(0, 2, "0")]
    public void ToString_ShouldFormatFractionCorrectly(int numerator, int denominator, string expected) {

        // Arrange
        var frac = new Fraction(numerator, denominator);

        // Act
        var result = frac.ToString();

        // Assert
        result.Should().Be(expected);

    }

}