using Domain.Orders.Builders;
using Domain.Orders.Entities.Hardware;
using Domain.Orders.Enums;
using Domain.Orders.ValueObjects;
using Domain.ValueObjects;
using FluentAssertions;

namespace ApplicationCore.Tests.Unit.Orders.Products.Supplies;

[Collection("DrawerBoxBuilder")]
public class WallCabinetSuppliesTests {

    private readonly WallCabinetBuilder _builder;

    public WallCabinetSuppliesTests() {

        _builder = new();

    }

    /*
    [Fact]
    public void Should_IncludeOneDoorPullPerCabinet_WhenCabinetHasOneDoor() {

        // Arrange
        var cabinet = _builder.WithDoors(WallCabinetDoors.OneDoor(HingeSide.Left))
                                .WithWidth(Dimension.FromMillimeters(500))
                                .WithHeight(Dimension.FromMillimeters(500))
                                .WithDepth(Dimension.FromMillimeters(500))
                                .WithQty(2)
                                .Build();
        Supply expectedSupply = Supply.DoorPull(cabinet.Qty * 1);

        // Act
        var supplies = cabinet.GetSupplies();

        // Assert
        supplies.Should().Contain(s => SupplyComparer.Compare(s, expectedSupply));

    }

    [Fact]
    public void Should_IncludeTwoDoorPullPerCabinet_WhenCabinetHasTwoDoors() {

        // Arrange
        var cabinet = _builder.WithDoors(WallCabinetDoors.TwoDoors())
                                .WithWidth(Dimension.FromMillimeters(500))
                                .WithHeight(Dimension.FromMillimeters(500))
                                .WithDepth(Dimension.FromMillimeters(500))
                                .WithQty(2)
                                .Build();

        Supply expectedSupply = Supply.DoorPull(cabinet.Qty * 2);

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
        var cabinet = _builder.WithDoors(WallCabinetDoors.OneDoor(HingeSide.Left))
                                .WithInside(new(adjShelfQty, 0))
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
        var cabinet = _builder.WithDoors(WallCabinetDoors.OneDoor(HingeSide.Left))
                                .WithWidth(Dimension.FromMillimeters(500))
                                .WithHeight(Dimension.FromMillimeters(cabHeight))
                                .WithDepth(Dimension.FromMillimeters(500))
                                .WithQty(2)
                                .Build();

        IEnumerable<Supply> expectedSupplies = Supply.StandardHinge(cabinet.Qty * expectedHingeQty);

        // Act
        var supplies = cabinet.GetSupplies();

        // Assert
        foreach (var supply in expectedSupplies) {
            supplies.Should().Contain(s => SupplyComparer.Compare(s, supply));
        }

    }

}
