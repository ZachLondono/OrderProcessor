using ApplicationCore.Features.Orders.Shared.Domain;
using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Shared.Domain;
using FluentAssertions;

namespace ApplicationCore.Tests.Unit.Orders.Products.Supplies;

[Collection("DrawerBoxBuilder")]
public class DiagonalBaseCabinetSuppliesTests {

    private readonly BaseDiagonalCornerCabinetBuilder _builder;

    public DiagonalBaseCabinetSuppliesTests() {

        _builder = new();

    }

    [Fact]
    public void Should_IncludeFourLegLevelersPerCabinet_WhenToeTypeIsLegLeveler() {

        // Arrange
        var cabinet = _builder.WithToeType(ToeType.LegLevelers)
                                .WithWidth(Dimension.FromMillimeters(500))
                                .WithHeight(Dimension.FromMillimeters(500))
                                .WithDepth(Dimension.FromMillimeters(500))
                                .WithQty(2)
                                .Build();

        Supply expectedSupply = Supply.CabinetLeveler(cabinet.Qty * 5);

        // Act
        var supplies = cabinet.GetSupplies();

        // Assert
        supplies.Should().ContainEquivalentOf(expectedSupply);

    }

    [Theory]
    [InlineData(1, 5)]
    [InlineData(2, 10)]
    public void Should_IncludeFiveShelfPegsPerAdjustableShelf(int adjShelfQty, int expectedPegQty) {

        // Arrange
        var cabinet = _builder.WithDoorQty(1)
                                .WithAdjustableShelves(adjShelfQty)
                                .WithWidth(Dimension.FromMillimeters(500))
                                .WithHeight(Dimension.FromMillimeters(500))
                                .WithDepth(Dimension.FromMillimeters(500))
                                .WithQty(2)
                                .Build();

        Supply expectedSupply = Supply.LockingShelfPeg(cabinet.Qty * expectedPegQty);

        // Act
        var supplies = cabinet.GetSupplies();

        // Assert
        supplies.Should().ContainEquivalentOf(expectedSupply);

    }

    [Theory]
    [InlineData(500, 2)]
    [InlineData(Supply.TWO_HINGE_MAX + 10, 3)]
    [InlineData(Supply.THREE_HINGE_MAX + 10, 4)]
    [InlineData(Supply.FOUR_HINGE_MAX + 10, 5)]
    public void Should_IncludeHingeAndHingePlatePerHingePosition(double cabHeight, int expectedHingeQty) {

        // Arrange
        var cabinet = _builder.WithDoorQty(1)
                                .WithWidth(Dimension.FromMillimeters(500))
                                .WithHeight(Dimension.FromMillimeters(cabHeight))
                                .WithDepth(Dimension.FromMillimeters(500))
                                .WithQty(2)
                                .Build();

        IEnumerable<Supply> expectedSupplies = Supply.CrossCornerHinge(cabinet.Qty * expectedHingeQty);

        // Act
        var supplies = cabinet.GetSupplies();

        // Assert
        foreach (var supply in expectedSupplies) {
            supplies.Should().ContainEquivalentOf(supply);
        }

    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 2)]
    public void Should_IncludeOneDoorPullPerDoor(int doorQty, int expectedQty) {

        // Arrange
        var cabinet = _builder.WithDoorQty(doorQty)
                                .WithWidth(Dimension.FromMillimeters(500))
                                .WithHeight(Dimension.FromMillimeters(500))
                                .WithDepth(Dimension.FromMillimeters(500))
                                .WithQty(2)
                                .Build();

        Supply expectedSupply = Supply.DoorPull(cabinet.Qty * expectedQty);

        // Act
        var supplies = cabinet.GetSupplies();

        // Assert
        supplies.Should().ContainEquivalentOf(expectedSupply);

    }

}
