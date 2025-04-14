using Domain.Orders.Builders;
using Domain.Orders.Entities.Hardware;
using Domain.Orders.Enums;
using Domain.Orders.ValueObjects;
using Domain.ValueObjects;
using FluentAssertions;

namespace Domain.Tests.Unit.Orders.Products.CabinetSupplies;

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
        supplies.Should().Contain(s => SupplyComparer.Compare(s, expectedSupply));

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
        supplies.Should().Contain(s => SupplyComparer.Compare(s, expectedSupply));

    }

    [Theory]
    [InlineData(500, 2)]
    [InlineData(Supply.TWO_HINGE_MAX + 10, 3)]
    [InlineData(Supply.THREE_HINGE_MAX + 10, 4)]
    [InlineData(Supply.FOUR_HINGE_MAX + 10, 5)]
    public void Should_IncludeHingeAndHingePlatePerHingePosition(double cabHeight, int expectedHingeQty) {

        // Arrange
        var cabinet = _builder.WithDoors(new(HingeSide.Left))
                                .WithBlindSide(BlindSide.Right)
                                .WithWidth(Dimension.FromMillimeters(500))
                                .WithHeight(Dimension.FromMillimeters(cabHeight))
                                .WithDepth(Dimension.FromMillimeters(500))
                                .WithQty(2)
                                .Build();

        IEnumerable<Supply> expectedSupplies = Supply.FullOverlayHinge(cabinet.Qty * expectedHingeQty);

        // Act
        var supplies = cabinet.GetSupplies();

        // Assert
        foreach (var supply in expectedSupplies) {
            supplies.Should().Contain(s => SupplyComparer.Compare(s, supply));
        }

    }

    [Theory]
    [InlineData(HingeSide.Left, BlindSide.Left)]
    [InlineData(HingeSide.Right, BlindSide.Right)]
    [InlineData(HingeSide.NotApplicable, BlindSide.Left)]
    [InlineData(HingeSide.NotApplicable, BlindSide.Right)]
    public void Should_IncludeBlindHinge_WhenDoorIsHingedOnBlindSide(HingeSide hingeSide, BlindSide blindSide) {

        // Arrange
        var cabinet = _builder.WithDoors(new(hingeSide))
                                .WithBlindSide(blindSide)
                                .WithWidth(Dimension.FromMillimeters(500))
                                .WithHeight(Dimension.FromMillimeters(500))
                                .WithDepth(Dimension.FromMillimeters(500))
                                .WithQty(2)
                                .Build();

        IEnumerable<Supply> expectedSupplies = Supply.BlindCornerHinge(cabinet.Qty * 2);

        // Act
        var supplies = cabinet.GetSupplies();

        // Assert
        foreach (var supply in expectedSupplies) {
            supplies.Should().Contain(s => SupplyComparer.Compare(s, supply));
        }

    }

    [Theory]
    [InlineData(HingeSide.Left, BlindSide.Right)]
    [InlineData(HingeSide.Right, BlindSide.Left)]
    public void Should_NotIncludeBlindHinge_WhenDoorIsNotHingedOnBlindSide(HingeSide hingeSide, BlindSide blindSide) {

        // Arrange
        var cabinet = _builder.WithDoors(new(hingeSide))
                                .WithBlindSide(blindSide)
                                .WithWidth(Dimension.FromMillimeters(500))
                                .WithHeight(Dimension.FromMillimeters(500))
                                .WithDepth(Dimension.FromMillimeters(500))
                                .WithQty(2)
                                .Build();

        var hingeSupply = Supply.BlindCornerHinge(0).First();

        // Act
        var supplies = cabinet.GetSupplies();

        // Assert
        supplies.Where(s => s.Description.Equals(hingeSupply.Description)).Should().BeEmpty();

    }

    /*
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
        supplies.Should().Contain(s => SupplyComparer.Compare(s, expectedSupply));

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
        supplies.Should().Contain(s => SupplyComparer.Compare(s, expectedSupply));

    }
    */

    /*
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
        DovetailDrawerBoxBuilder.CabinetUnderMountDrawerSlideBoxDepths = new Dimension[] {
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
        DovetailDrawerBoxBuilder.CabinetUnderMountDrawerSlideBoxDepths = new Dimension[] {
            Dimension.FromMillimeters(457)
        };

        // Act
        var supplies = cabinet.GetSupplies();

        // Assert
        supplies.Where(s => s.Name == expectedSupplyA.Name).Sum(s => s.Qty).Should().Be(cabinet.Qty * drawerQty);

    }
    */

}
