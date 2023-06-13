using ApplicationCore.Features.Orders.Shared.Domain;
using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Shared.Domain;
using FluentAssertions;

namespace ApplicationCore.Tests.Unit.Orders.Products.Supplies;

[Collection("DrawerBoxBuilder")]
public class TallCabinetSuppliesTests {

    private readonly TallCabinetBuilder _builder;

    public TallCabinetSuppliesTests() {

        _builder = new();

    }


    [Fact]
    public void Should_IncludeOneDoorPullPerCabinet_WhenCabinetHasOneDoor() {

        // Arrange
        var cabinet = _builder.WithDoors(TallCabinetDoors.OneDoor(HingeSide.Left))
                                .WithWidth(Dimension.FromMillimeters(500))
                                .WithHeight(Dimension.FromMillimeters(500))
                                .WithDepth(Dimension.FromMillimeters(500))
                                .WithQty(2)
                                .Build();
        Supply expectedSupply = Supply.DoorPull(cabinet.Qty * 1);

        // Act
        var supplies = cabinet.GetSupplies();

        // Assert
        supplies.Should().ContainEquivalentOf(expectedSupply);

    }

    [Fact]
    public void Should_IncludeTwoDoorPullPerCabinet_WhenCabinetHasTwoDoors() {

        // Arrange
        var cabinet = _builder.WithDoors(TallCabinetDoors.TwoDoors())
                                .WithWidth(Dimension.FromMillimeters(500))
                                .WithHeight(Dimension.FromMillimeters(500))
                                .WithDepth(Dimension.FromMillimeters(500))
                                .WithQty(2)
                                .Build();

        Supply expectedSupply = Supply.DoorPull(cabinet.Qty * 2);

        // Act
        var supplies = cabinet.GetSupplies();

        // Assert
        supplies.Should().ContainEquivalentOf(expectedSupply);

    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void Should_IncludeOneDrawerSlidesPerDrawerBox_WhenSlideTypeIsUM(int rollOutQty) {

        // Arrange
        Dimension[] positions = new Dimension[rollOutQty];
        for (int i = 0; i < rollOutQty; i++) {
            positions[i] = Dimension.Zero;
        }

        var cabinet = _builder.WithDoors(TallCabinetDoors.TwoDoors())
                                .WithInside(new(0, 0, 0, new RollOutOptions(positions, true, RollOutBlockPosition.None)))
                                .WithWidth(Dimension.FromMillimeters(500))
                                .WithHeight(Dimension.FromMillimeters(500))
                                .WithDepth(Dimension.FromMillimeters(500))
                                .WithQty(2)
                                .Build();

        Supply expectedSupply = Supply.UndermountSlide(cabinet.Qty * rollOutQty, Dimension.FromMillimeters(457));
        DovetailDrawerBoxBuilder.UnderMountDrawerSlideDepths = new Dimension[] {
            Dimension.FromMillimeters(457)
        };

        // Act
        var supplies = cabinet.GetSupplies();

        // Assert
        supplies.Where(s => s.Name == expectedSupply.Name).Sum(s => s.Qty).Should().Be(cabinet.Qty * rollOutQty);

    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void Should_IncludeOneDrawerSlidesPerDrawerBox_WhenSlideTypeIsSM(int rollOutQty) {

        // Arrange
        Dimension[] positions = new Dimension[rollOutQty];
        for (int i = 0; i < rollOutQty; i++) {
            positions[i] = Dimension.Zero;
        }

        var cabinet = _builder.WithInside(new(0, 0, 0, new RollOutOptions(positions, true, RollOutBlockPosition.None)))
                                .WithBoxOptions(new(CabinetDrawerBoxMaterial.FingerJointBirch, DrawerSlideType.SideMount))
                                .WithDoors(TallCabinetDoors.TwoDoors())
                                .WithWidth(Dimension.FromMillimeters(500))
                                .WithHeight(Dimension.FromMillimeters(500))
                                .WithDepth(Dimension.FromMillimeters(500))
                                .WithQty(2)
                                .Build();

        Supply expectedSupply = Supply.SidemountSlide(0, Dimension.FromMillimeters(450));

        // Act
        var supplies = cabinet.GetSupplies();

        // Assert
        supplies.Where(s => s.Name == expectedSupply.Name).Sum(s => s.Qty).Should().Be(cabinet.Qty * rollOutQty);

    }

    [Fact]
    public void Should_IncludeFourLegLevelersPerCabinet_WhenToeTypeIsLegLeveler() {

        // Arrange
        var cabinet = _builder.WithDoors(new(HingeSide.Left))
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
        supplies.Should().ContainEquivalentOf(expectedSupply);

    }

    [Theory]
    [InlineData(1, 4)]
    [InlineData(2, 8)]
    public void Should_IncludeFourShelfPegsPerAdjustableShelf(int adjShelfQty, int expectedPegQty) {

        // Arrange
        var cabinet = _builder.WithDoors(new(HingeSide.Left))
                                .WithInside(new(adjShelfQty, 0, 0, 0))
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

        var cabinet = _builder.WithInside(new(0, 0, 0, new RollOutOptions(positions, true, rollOutBlockPosition)))
                                .WithDoors(TallCabinetDoors.NoDoors())
                                .WithWidth(Dimension.FromMillimeters(500))
                                .WithHeight(Dimension.FromMillimeters(500))
                                .WithDepth(Dimension.FromMillimeters(500))
                                .WithQty(2)
                                .Build();

        Supply expectedSupply = Supply.PullOutBlock(expectedQty * cabinet.Qty);

        // Act
        var supplies = cabinet.GetSupplies();

        // Assert
        supplies.Should().ContainEquivalentOf(expectedSupply);

    }


}