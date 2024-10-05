using Domain.Orders.ValueObjects;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Tests.Unit.Orders.ValueObjects;

public class ToeTypeTests {

    [Fact]
    public void FromPSIParameter_ShouldReturnNotch_WhenNotchPSIParameterIsPassed() {

        // Arrange
        var toeType = ToeType.Notched;

        // Act
        var result = ToeType.FromPSIParameter(toeType.PSIParameter);

        // Assert
        result.Should().BeEquivalentTo(toeType);

    }

    [Fact]
    public void FromPSIParameter_ShouldReturnFurnitureBase_WhenFurnitureBasePSIParameterIsPassed() {

        // Arrange
        var toeType = ToeType.FurnitureBase;

        // Act
        var result = ToeType.FromPSIParameter(toeType.PSIParameter);

        // Assert
        result.Should().BeEquivalentTo(toeType);

    }

    [Fact]
    public void FromPSIParameter_ShouldReturnLegLevelers_WhenLegLevelersPSIParameterIsPassed() {

        // Arrange
        var toeType = ToeType.LegLevelers;

        // Act
        var result = ToeType.FromPSIParameter(toeType.PSIParameter);

        // Assert
        result.Should().BeEquivalentTo(toeType);

    }

    [Fact]
    public void FromPSIParameter_ShouldReturnNoToe_WhenNoToePSIParameterIsPassed() {

        // Arrange
        var toeType = ToeType.NoToe;

        // Act
        var result = ToeType.FromPSIParameter(toeType.PSIParameter);

        // Assert
        result.Should().BeEquivalentTo(toeType);

    }

}
