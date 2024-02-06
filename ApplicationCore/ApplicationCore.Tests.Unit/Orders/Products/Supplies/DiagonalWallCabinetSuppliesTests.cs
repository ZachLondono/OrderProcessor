using Domain.Orders.Builders;
using Domain.Orders.Enums;
using Domain.Orders.ValueObjects;
using Domain.Orders.ValueObjects;
using Domain.ValueObjects;
using FluentAssertions;

namespace ApplicationCore.Tests.Unit.Orders.Products.Supplies;

[Collection("DrawerBoxBuilder")]
public class DiagonalWallCabinetSuppliesTests {

    private readonly WallDiagonalCornerCabinetBuilder _builder;

    public DiagonalWallCabinetSuppliesTests() {

        _builder = new();

    }

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
        supplies.Should().ContainEquivalentOf(expectedSupply);

    }

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

}

public class BlindBaseCabinetSuppliesTests {

    private readonly BlindBaseCabinetBuilder _builder;

    public BlindBaseCabinetSuppliesTests() {

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

        Supply expectedSupply = Supply.CabinetLeveler(cabinet.Qty * 4);

        // Act
        var supplies = cabinet.GetSupplies();

        // Assert
        supplies.Should().ContainEquivalentOf(expectedSupply);

    }

    [Theory]
    [InlineData(1, 4)]
    [InlineData(2, 8)]
    public void Should_IncludeFourShelfPegsPerAdjustableShelf(int adjShelfQty, int expectedPegQty) {

        // Arrange
        var cabinet = _builder.WithAdjustableShelves(adjShelfQty)
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
        var cabinet = _builder.WithDoors(new(HingeSide.Left))
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
            supplies.Should().ContainEquivalentOf(supply);
        }

    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 2)]
    public void Should_IncludeOneDoorPullPerDoor(int doorQty, int expectedQty) {

        // Arrange
        var cabinet = _builder.WithDoors(new(doorQty == 1 ? HingeSide.Left : HingeSide.NotApplicable))
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


    [Theory]
    [InlineData(1, 0)]
    [InlineData(1, 1)]
    [InlineData(2, 0)]
    [InlineData(2, 2)]
    public void Should_IncludeOneDrawerPullPerDrawerBox(int drawerQty, int rollOutQty) {

        // Arrange
        Dimension[] positions = new Dimension[rollOutQty];
        for (int i = 0; i < rollOutQty; i++) {
            positions[i] = Dimension.Zero;
        }

        var cabinet = _builder.WithDoors(new())
                                .WithDrawers(new() {
                                    Quantity = drawerQty,
                                    FaceHeight = Dimension.FromMillimeters(157)
                                })
                                .WithWidth(Dimension.FromMillimeters(500))
                                .WithHeight(Dimension.FromMillimeters(500))
                                .WithDepth(Dimension.FromMillimeters(500))
                                .WithQty(2)
                                .Build();

        Supply expectedSupply = Supply.DrawerPull(cabinet.Qty * drawerQty);

        // Act
        var supplies = cabinet.GetSupplies();

        // Assert
        supplies.Should().ContainEquivalentOf(expectedSupply);

    }

    [Theory]
    //[InlineData(1)]
    [InlineData(2)]
    public void Should_IncludeOneDrawerSlidesPerDrawerBox_WhenSlideTypeIsUM(int drawerQty) {

        // Arrange
        var cabinet = _builder.WithDoors(new())
                                .WithDrawers(new() {
                                    Quantity = drawerQty,
                                    FaceHeight = Dimension.FromMillimeters(157)
                                })
                                .WithWidth(Dimension.FromMillimeters(500))
                                .WithHeight(Dimension.FromMillimeters(500))
                                .WithDepth(Dimension.FromMillimeters(500))
                                .WithQty(2)
                                .Build();

        Supply expectedSupplyA = Supply.UndermountSlide(cabinet.Qty * drawerQty, Dimension.FromMillimeters(457));
        DovetailDrawerBoxBuilder.UnderMountDrawerSlideDepths = new Dimension[] {
            Dimension.FromMillimeters(457)
        };

        // Act
        var supplies = cabinet.GetSupplies();

        // Assert
        supplies.Where(s => s.Name == expectedSupplyA.Name).Sum(s => s.Qty).Should().Be(cabinet.Qty * drawerQty);

    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void Should_IncludeOneDrawerSlidesPerDrawerBox_WhenSlideTypeIsSM(int drawerQty) {

        // Arrange

        var cabinet = _builder.WithDoors(new())
                                .WithDrawers(new() {
                                    Quantity = drawerQty,
                                    FaceHeight = Dimension.FromMillimeters(157)
                                })
                                .WithBoxOptions(new(CabinetDrawerBoxMaterial.FingerJointBirch, DrawerSlideType.SideMount))
                                .WithWidth(Dimension.FromMillimeters(500))
                                .WithHeight(Dimension.FromMillimeters(500))
                                .WithDepth(Dimension.FromMillimeters(500))
                                .WithQty(2)
                                .Build();

        Supply expectedSupplyA = Supply.SidemountSlide(cabinet.Qty * drawerQty, Dimension.FromMillimeters(457));
        DovetailDrawerBoxBuilder.UnderMountDrawerSlideDepths = new Dimension[] {
            Dimension.FromMillimeters(457)
        };

        // Act
        var supplies = cabinet.GetSupplies();

        // Assert
        supplies.Where(s => s.Name == expectedSupplyA.Name).Sum(s => s.Qty).Should().Be(cabinet.Qty * drawerQty);

    }

}
