using Domain.Orders.Builders;
using Domain.Orders.Entities.Hardware;
using Domain.Orders.Enums;
using Domain.Orders.ValueObjects;
using Domain.ValueObjects;
using FluentAssertions;

namespace ApplicationCore.Tests.Unit.Orders.Products.Supplies;

[Collection("DrawerBoxBuilder")]
public class BaseCabinetSuppliesTests {

    private readonly BaseCabinetBuilder _builder;

    public BaseCabinetSuppliesTests() {

        _builder = new();

    }

    [Fact]
    public void Should_IncludeOneDoorPullPerCabinet_WhenCabinetHasOneDoor() {

        // Arrange
        var cabinet = _builder.WithDoors(BaseCabinetDoors.OneDoor(HingeSide.Left))
                                .WithBoxOptions(new(CabinetDrawerBoxMaterial.FingerJointBirch, DrawerSlideType.UnderMount))
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
        var cabinet = _builder.WithDoors(BaseCabinetDoors.TwoDoors())
                                .WithBoxOptions(new(CabinetDrawerBoxMaterial.FingerJointBirch, DrawerSlideType.UnderMount))
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

        var cabinet = _builder.WithDrawers(new() {
            Quantity = drawerQty,
            FaceHeight = Dimension.FromMillimeters(157)
        })
                                .WithBoxOptions(new(CabinetDrawerBoxMaterial.FingerJointBirch, DrawerSlideType.UnderMount))
                                .WithInside(new(0, new RollOutOptions(positions, true, RollOutBlockPosition.None), ShelfDepth.Default))
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

    /*
    [Theory]
    [InlineData(1, 0)]
    [InlineData(1, 1)]
    [InlineData(2, 0)]
    [InlineData(2, 2)]
    public void Should_IncludeOneDrawerSlidesPerDrawerBox_WhenSlideTypeIsUM(int drawerQty, int rollOutQty) {

        // Arrange
        Dimension[] positions = new Dimension[rollOutQty];
        for (int i = 0; i < rollOutQty; i++) {
            positions[i] = Dimension.Zero;
        }

        var cabinet = _builder.WithDoors(BaseCabinetDoors.TwoDoors())
                                .WithDrawers(new() {
                                    Quantity = drawerQty,
                                    FaceHeight = Dimension.FromMillimeters(157)
                                })
                                .WithBoxOptions(new(CabinetDrawerBoxMaterial.FingerJointBirch, DrawerSlideType.UnderMount))
                                .WithInside(new(0, new RollOutOptions(positions, true, RollOutBlockPosition.None), ShelfDepth.Default))
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
        supplies.Where(s => s.Name == expectedSupplyA.Name).Sum(s => s.Qty).Should().Be(cabinet.Qty * (drawerQty + rollOutQty));

    }
    */

    /*
    [Theory]
    [InlineData(1, 0)]
    [InlineData(1, 1)]
    [InlineData(2, 0)]
    [InlineData(2, 2)]
    public void Should_IncludeOneDrawerSlidesPerDrawerBox_WhenSlideTypeIsSM(int drawerQty, int rollOutQty) {

        // Arrange
        Dimension[] positions = new Dimension[rollOutQty];
        for (int i = 0; i < rollOutQty; i++) {
            positions[i] = Dimension.Zero;
        }

        var cabinet = _builder.WithDrawers(new() {
            Quantity = drawerQty,
            FaceHeight = Dimension.FromMillimeters(157)
        })
                                .WithBoxOptions(new(CabinetDrawerBoxMaterial.FingerJointBirch, DrawerSlideType.SideMount))
                                .WithInside(new(0, new RollOutOptions(positions, true, RollOutBlockPosition.None), ShelfDepth.Default))
                                .WithDoors(BaseCabinetDoors.TwoDoors())
                                .WithWidth(Dimension.FromMillimeters(500))
                                .WithHeight(Dimension.FromMillimeters(500))
                                .WithDepth(Dimension.FromMillimeters(500))
                                .WithQty(2)
                                .Build();

        Supply expectedSupplyA = Supply.SidemountSlide(cabinet.Qty * (drawerQty + rollOutQty), Dimension.FromMillimeters(450));

        // Act
        var supplies = cabinet.GetSupplies();

        // Assert
        supplies.Where(s => s.Name == expectedSupplyA.Name).Sum(s => s.Qty).Should().Be(cabinet.Qty * (drawerQty + rollOutQty));

    }
    */

    [Fact]
    public void Should_IncludeFourLegLevelersPerCabinet_WhenToeTypeIsLegLeveler() {

        // Arrange
        var cabinet = _builder.WithDoors(BaseCabinetDoors.OneDoor(HingeSide.Left))
                                .WithBoxOptions(new(CabinetDrawerBoxMaterial.FingerJointBirch, DrawerSlideType.UnderMount))
                                .WithToeType(ToeType.LegLevelers)
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
        var cabinet = _builder.WithDoors(BaseCabinetDoors.OneDoor(HingeSide.Left))
                                .WithInside(new(adjShelfQty, 0, ShelfDepth.Default))
                                .WithBoxOptions(new(CabinetDrawerBoxMaterial.FingerJointBirch, DrawerSlideType.UnderMount))
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
        var cabinet = _builder.WithDoors(BaseCabinetDoors.OneDoor(HingeSide.Left))
                                .WithBoxOptions(new(CabinetDrawerBoxMaterial.FingerJointBirch, DrawerSlideType.UnderMount))
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

    [Theory]
    [InlineData(1, RollOutBlockPosition.Left, 1)]
    [InlineData(1, RollOutBlockPosition.Right, 1)]
    [InlineData(1, RollOutBlockPosition.Both, 2)]
    [InlineData(2, RollOutBlockPosition.Left, 2)]
    [InlineData(2, RollOutBlockPosition.Right, 2)]
    [InlineData(2, RollOutBlockPosition.Both, 4)]
    public void Should_IncludeRollOutBlockForEachSide_WhenEnabled(int rollOutQty, RollOutBlockPosition rollOutBlockPosition, int expectedQty) {

        // Arrange
        Dimension[] positions = new Dimension[rollOutQty];
        for (int i = 0; i < rollOutQty; i++) {
            positions[i] = Dimension.Zero;
        }

        var cabinet = _builder.WithInside(new(0, new RollOutOptions(positions, true, rollOutBlockPosition), ShelfDepth.Default))
                                .WithBoxOptions(new(CabinetDrawerBoxMaterial.FingerJointBirch, DrawerSlideType.UnderMount))
                                .WithDoors(BaseCabinetDoors.TwoDoors())
                                .WithWidth(Dimension.FromMillimeters(500))
                                .WithHeight(Dimension.FromMillimeters(500))
                                .WithDepth(Dimension.FromMillimeters(500))
                                .WithQty(2)
                                .Build();

        Supply expectedSupply = Supply.PullOutBlock(expectedQty * cabinet.Qty);

        // Act
        var supplies = cabinet.GetSupplies();

        // Assert
        supplies.Should().Contain(s => SupplyComparer.Compare(s, expectedSupply));

    }

}
