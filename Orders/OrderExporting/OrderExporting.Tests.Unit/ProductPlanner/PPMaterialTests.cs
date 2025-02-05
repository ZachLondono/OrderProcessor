using Domain.ProductPlanner;
using FluentAssertions;

namespace OrderExporting.Tests.Unit.ProductPlanner;

public class PPMaterialTests {

    [Fact]
    public void Color_ShouldRemoveParenthesesFromName() {

        // Arrange
        string color = "Color (A) Name";

        // Act
        var material = new PPMaterial("Material", color);

        // Assert
        material.Color.Should().Be("Color A Name");

    }

    [Fact]
    public void ShortenedColor_ShouldTrimLongColorName() {

        // Arrange
        string color = "Really long material color name";

        // Act
        var material = new PPMaterial("Material", color);

        // Assert
        material.ShortenedColor.Should().Be("Really long material");

    }

}
