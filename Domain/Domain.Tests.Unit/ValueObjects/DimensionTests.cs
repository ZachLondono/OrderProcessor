using Domain.ValueObjects;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Forms;
using System.Diagnostics;

namespace ApplicationCore.Tests.Unit.Shared;

public class DimensionTests {

    [Fact]
    public void FromInches_ShouldEqualAsInches() {

        // Arrange
        double val = 123.456;

        // Act
        var dim = Dimension.FromInches(123.456);

        // Assert
        dim.AsInches().Should().Be(val);

    }

    [Fact]
    public void FromMillimeters_ShouldEqualAsMillimeters() {

        // Arrange
        double val = 123.456;

        // Act
        var dim = Dimension.FromMillimeters(123.456);

        // Assert
        dim.AsMillimeters().Should().Be(val);

    }

    [Fact]
    public void FromMillimeters_ShouldConvert_ToInches() {

        // Arrange
        double val = 123.456;

        // Act
        var dim = Dimension.FromMillimeters(123.456);

        // Assert
        dim.AsInches().Should().Be(val / 25.4);

    }

    [Fact]
    public void FromInches_ShouldConvert_ToMillimeters() {

        // Arrange
        double val = 123.456;

        // Act
        var dim = Dimension.FromInches(123.456);

        // Assert
        dim.AsMillimeters().Should().Be(val * 25.4);

    }

    [Fact]
    public void MultiplicationOfInchesAndInt_ShouldBeCorrect() {

        // Arrange
        double val1 = 123.456;
        var dim1 = Dimension.FromInches(val1);
        int qty = 789;

        // Act
        var result1 = dim1 * qty;
        var result2 = qty * dim1;


        // Assert
        result1.AsInches().Should().Be(val1 * qty);
        result2.AsInches().Should().Be(val1 * qty);

    }

    [Fact]
    public void MultiplicationOfMillimetersAndInt_ShouldBeCorrect() {

        // Arrange
        double val1 = 123.456;
        var dim1 = Dimension.FromMillimeters(val1);
        int qty = 789;

        // Act
        var result1 = dim1 * qty;
        var result2 = qty * dim1;


        // Assert
        result1.AsMillimeters().Should().Be(val1 * qty);
        result2.AsMillimeters().Should().Be(val1 * qty);

    }

    [Fact]
    public void MultiplicationOfInchesAndDouble_ShouldBeCorrect() {

        // Arrange
        double val1 = 123.456;
        var dim1 = Dimension.FromInches(val1);
        double qty = 789.112;

        // Act
        var result1 = dim1 * qty;
        var result2 = qty * dim1;


        // Assert
        result1.AsInches().Should().BeApproximately(val1 * qty, 0.01);
        result2.AsInches().Should().BeApproximately(val1 * qty, 0.01);

    }

    [Fact]
    public void MultiplicationOfMillimetersAndDouble_ShouldBeCorrect() {

        // Arrange
        double val1 = 123.456;
        var dim1 = Dimension.FromMillimeters(val1);
        double qty = 789.112;

        // Act
        var result1 = dim1 * qty;
        var result2 = qty * dim1;


        // Assert
        result1.AsMillimeters().Should().BeApproximately(val1 * qty, 0.01);
        result2.AsMillimeters().Should().BeApproximately(val1 * qty, 0.01);

    }

    [Fact]
    public void DivisionOfInchesAndInches_ShouldBeCorrect() {

        // Arrange
        double val1 = 123.456;
        var dim1 = Dimension.FromInches(val1);
        int qty = 789;

        // Act
        var result1 = dim1 / qty;

        // Assert
        result1.AsInches().Should().Be(val1 / qty);

    }

    [Fact]
    public void DivisionOfMillimetersAndInches_ShouldBeCorrect() {

        // Arrange
        double val1 = 123.456;
        var dim1 = Dimension.FromMillimeters(val1);
        int qty = 789;

        // Act
        var result1 = dim1 / qty;

        // Assert
        result1.AsMillimeters().Should().Be(val1 / qty);

    }

    [Fact]
    public void DivisionOfInchesAndDoubles_ShouldBeCorrect() {

        // Arrange
        double val1 = 123.456;
        var dim1 = Dimension.FromInches(val1);
        int qty = 789;

        // Act
        var result1 = dim1 / qty;

        // Assert
        result1.AsInches().Should().BeApproximately(val1 / qty, 0.01);

    }

    [Fact]
    public void DivisionOfMillimetersAndDoubles_ShouldBeCorrect() {

        // Arrange
        double val1 = 123.456;
        var dim1 = Dimension.FromMillimeters(val1);
        int qty = 789;

        // Act
        var result1 = dim1 / qty;

        // Assert
        result1.AsMillimeters().Should().BeApproximately(val1 / qty, 0.01);

    }

    [Fact]
    public void AdditionOfInches_ShouldBeCorrect() {

        // Arrange
        double val1 = 123.456;
        double val2 = 789.112;
        var dim1 = Dimension.FromInches(val1);
        var dim2 = Dimension.FromInches(val2);

        // Act
        var result = dim1 + dim2;


        // Assert
        result.AsInches().Should().Be(val1 + val2);

    }

    [Fact]
    public void AdditionOfMillimeters_ShouldBeCorrect() {

        // Arrange
        double val1 = 123.456;
        double val2 = 789.112;
        var dim1 = Dimension.FromMillimeters(val1);
        var dim2 = Dimension.FromMillimeters(val2);

        // Act
        var result = dim1 + dim2;


        // Assert
        result.AsMillimeters().Should().Be(val1 + val2);

    }

    [Fact]
    public void SubtractionOfInches_ShouldBeCorrect() {

        // Arrange
        double val1 = 123.456;
        double val2 = 789.112;
        var dim1 = Dimension.FromInches(val1);
        var dim2 = Dimension.FromInches(val2);

        // Act
        var result = dim2 - dim1;


        // Assert
        result.AsInches().Should().Be(val2 - val1);

    }

    [Fact]
    public void SubtractionOfMillimeters_ShouldBeCorrect() {

        // Arrange
        double val1 = 123.456;
        double val2 = 789.112;
        var dim1 = Dimension.FromMillimeters(val1);
        var dim2 = Dimension.FromMillimeters(val2);

        // Act
        var result = dim2 - dim1;


        // Assert
        result.AsMillimeters().Should().Be(val2 - val1);

    }

    [Fact]
    public void Dimensions_ShouldThrowException_WhenValueIsNegative() {

        // Arrange
        double val1 = -123.456;

        // Act
        var action = () => Dimension.FromMillimeters(val1);


        // Assert
        action.Should().Throw<ArgumentOutOfRangeException>();

    }

    [Theory]
    [InlineData(0.5, 1, 2)]
    [InlineData(0.25, 1, 4)]
    [InlineData(0.125, 1, 8)]
    [InlineData(0.0625, 1, 16)]
    [InlineData(0.03125, 1, 32)]
    [InlineData(0.75, 3, 4)]
    [InlineData(0.375, 3, 8)]
    [InlineData(0.1875, 3, 16)]
    [InlineData(0.09375, 3, 32)]
    public void AsInchFraction_ShouldBeAccurate(double inches, int numerator, int denominator) {

        // Arrange
        var dim = Dimension.FromInches(inches);

        // Act
        var result = dim.AsInchFraction();

        // Assert
        Debug.WriteLine(result.ToString());
        result.N.Should().Be(numerator);
        result.D.Should().Be(denominator);


    }

    [Theory]
    [InlineData(0.015625, (double)1 / 16, 0)]          //   1/64 -> 0
    [InlineData(0.03125, (double)1 / 16, 0.0625)]      //   2/64 -> 1/16
    [InlineData(0.046875, (double)1 / 16, 0.0625)]     //   3/64 -> 1/16
    [InlineData(0.0625, (double)1 / 16, 0.0625)]       //   4/64 -> 1/16
    [InlineData(0.078125, (double)1 / 16, 0.0625)]     //   5/64 -> 1/16
    [InlineData(0.09375, (double)1 / 16, 0.0625)]      //   6/64 -> 1/16
    [InlineData(0.109375, (double)1 / 16, 0.125)]      //   7/64 -> 1/8
    [InlineData(0.125, (double)1 / 16, 0.125)]        //   8/64 -> 1/8
    public void ManualTest(double raw, double denominator, double expected) {

        // Arrange
        var dim = Dimension.FromInches(raw);

        // Act
        dim = dim.RoundToInchMultiple(denominator);

        // Assert
        dim.AsInches().Should().Be(expected);


    }

    [Theory]
    [InlineData(0.5, 0.015625, 1, 2)]
    [InlineData(0.25, 0.015625, 1, 4)]
    [InlineData(0.125, 0.015625, 1, 8)]
    [InlineData(0.0625, 0.015625, 1, 16)]
    [InlineData(0.03125, 0.015625, 1, 32)]
    [InlineData(0.75, 0.015625, 3, 4)]
    [InlineData(0.375, 0.015625, 3, 8)]
    [InlineData(0.1875, 0.015625, 3, 16)]
    [InlineData(0.09375, 0.015625, 3, 32)]
    public void AsInchFraction_ShouldBeAccurate_WithLowerAccuracy(double inches, double accuracy, int numerator, int denominator) {

        // Arrange
        var dim = Dimension.FromInches(inches);

        // Act
        var result = dim.AsInchFraction(accuracy);

        // Assert
        Debug.WriteLine(result.ToString());
        result.N.Should().Be(numerator);
        result.D.Should().Be(denominator);


    }

    [Fact]
    public void DifferentInstances_ShouldBeEqual_WhenValueIsEqual() {

        // Arrange
        var dimA = Dimension.FromInches(1);
        var dimB = Dimension.FromMillimeters(25.4);

        // Assert
        Assert.True(dimA == dimB);
        Assert.True(dimB == dimA);

    }

    [Fact]
    public void Equals_Should_Work() {

        // Arrange
        var dimA = Dimension.FromInches(1);
        var dimB = Dimension.FromInches(1);

        // Assert
        Assert.True(dimA == dimB);
        Assert.True(dimA >= dimB);
        Assert.True(dimA <= dimB);

        Assert.True(dimB == dimA);
        Assert.True(dimB >= dimA);
        Assert.True(dimB <= dimA);

    }

    [Fact]
    public void GreaterThan_Should_Work() {

        // Arrange
        var dimA = Dimension.FromInches(2);
        var dimB = Dimension.FromInches(1);

        // Assert
        Assert.True(dimA >= dimB);
        Assert.True(dimA > dimB);

        Assert.False(dimB >= dimA);
        Assert.False(dimB > dimA);

    }

    [Fact]
    public void LessThan_Should_Work() {

        // Arrange
        var dimA = Dimension.FromInches(1);
        var dimB = Dimension.FromInches(2);

        // Assert
        Assert.True(dimA <= dimB);
        Assert.True(dimA < dimB);

        Assert.False(dimB <= dimA);
        Assert.False(dimB < dimA);

    }

    [Fact]
    public void Sort_Should_Work() {

        // Arrange
        var dims = new List<Dimension>() {
            Dimension.FromInches(3),
            Dimension.FromInches(2),
            Dimension.FromInches(1),
            Dimension.FromInches(4),
            Dimension.FromInches(0),
        };

        // Act
        dims.Sort();

        // Assert
        Assert.True(dims.First() == Dimension.Zero);
        Assert.True(dims.Skip(1).First() == Dimension.FromInches(1));
        Assert.True(dims.Skip(2).First() == Dimension.FromInches(2));
        Assert.True(dims.Skip(3).First() == Dimension.FromInches(3));
        Assert.True(dims.Skip(4).First() == Dimension.FromInches(4));

    }


}