using Domain.Orders.Builders;
using Domain.Orders.Entities.Hardware;
using Domain.ValueObjects;
using FluentAssertions;

namespace Domain.Tests.Unit.Orders.Products.CabinetSupplies;

[Collection("DrawerBoxBuilder")]
public class DiagonalWallCabinetSuppliesTests {

    private readonly WallDiagonalCornerCabinetBuilder _builder;

    public DiagonalWallCabinetSuppliesTests() {

        _builder = new();

    }

    /*
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void Should_IncludeOneDoorPullPerDoor(int doorQty) {

        // Arrange
        var cabinet = _builder.WithDoorQty(doorQty)
                                .WithWidth(Dimension.FromMillimeters(500))
                                .WithHeight(Dimension.FromMillimeters(500))
                                .WithDepth(Dimension.FromMillimeters(500))
                                .WithQty(2)
                                .Build();
        Supply expectedSupply = Supply.DoorPull(cabinet.Qty * doorQty);

        // Act
        var supplies = cabinet.GetSupplies();

        // Assert
        supplies.Should().Contain(s => SupplyComparer.Compare(s, expectedSupply));

    }
    */

    [Theory]
    [InlineData(1, 4)]
    [InlineData(2, 8)]
    public void Should_IncludeFourShelfPegsPerAdjustableShelf(int adjShelfQty, int expectedPegQty) {

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
        supplies.Should().Contain(s => SupplyComparer.Compare(s, expectedSupply));

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
            supplies.Should().Contain(s => SupplyComparer.Compare(s, supply));
        }

    }

}
