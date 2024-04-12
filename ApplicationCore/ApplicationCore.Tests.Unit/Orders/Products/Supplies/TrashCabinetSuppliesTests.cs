using Domain.Orders.Builders;
using Domain.Orders.Entities.Hardware;
using Domain.Orders.Enums;
using Domain.Orders.ValueObjects;
using Domain.ValueObjects;
using FluentAssertions;

namespace ApplicationCore.Tests.Unit.Orders.Products.Supplies;

[Collection("DrawerBoxBuilder")]
public class TrashCabinetSuppliesTests {

    private readonly TrashCabinetBuilder _builder;

    public TrashCabinetSuppliesTests() {

        _builder = new();

    }

    [Fact]
    public void Should_IncludeSingleTrashCan_WhenTrashConfigurationIsOneCan() {

        // Arrange
        var cabinet = _builder.WithTrashPulloutConfiguration(TrashPulloutConfiguration.OneCan)
                                .WithQty(2)
                                .WithWidth(Dimension.FromMillimeters(500))
                                .WithHeight(Dimension.FromMillimeters(500))
                                .WithDepth(Dimension.FromMillimeters(500))
                                .Build();
        var expectedSupply = Supply.SingleTrashPullout(cabinet.Qty);

        // Act
        var supplies = cabinet.GetSupplies();

        // Assert
        supplies.Should().ContainEquivalentOf(expectedSupply);

    }

    [Fact]
    public void Should_IncludeDoubleTrashCan_WhenTrashConfigurationIsTwoCans() {

        // Arrange
        var cabinet = _builder.WithTrashPulloutConfiguration(TrashPulloutConfiguration.TwoCans)
                                .WithQty(2)
                                .WithWidth(Dimension.FromMillimeters(500))
                                .WithHeight(Dimension.FromMillimeters(500))
                                .WithDepth(Dimension.FromMillimeters(500))
                                .Build();
        var expectedSupply = Supply.DoubleTrashPullout(cabinet.Qty);

        // Act
        var supplies = cabinet.GetSupplies();

        // Assert
        supplies.Should().ContainEquivalentOf(expectedSupply);

    }

    [Fact]
    public void Should_IncludeFourLegLevelersPerCabinet() {

        // Arrange
        var cabinet = _builder.WithToeType(ToeType.LegLevelers)
                                .WithQty(2)
                                .WithWidth(Dimension.FromMillimeters(500))
                                .WithHeight(Dimension.FromMillimeters(500))
                                .WithDepth(Dimension.FromMillimeters(500))
                                .Build();
        var expectedSupply = Supply.CabinetLeveler(4 * cabinet.Qty);

        // Act
        var supplies = cabinet.GetSupplies();

        // Assert
        supplies.Should().ContainEquivalentOf(expectedSupply);

    }

    [Fact]
    public void Should_IncludeOneDoorAndOneDrawerPullPerCabinet() {

        // Arrange
        var cabinet = _builder.WithQty(2)
                            .WithWidth(Dimension.FromMillimeters(500))
                            .WithHeight(Dimension.FromMillimeters(500))
                            .WithDepth(Dimension.FromMillimeters(500))
                            .Build();
        var expectedSupplyA = Supply.DoorPull(cabinet.Qty);
        var expectedSupplyB = Supply.DrawerPull(cabinet.Qty);

        // Act
        var supplies = cabinet.GetSupplies();

        // Assert
        supplies.Should().ContainEquivalentOf(expectedSupplyA);
        supplies.Should().ContainEquivalentOf(expectedSupplyB);

    }

    [Fact]
    public void Should_IncludeOneUMSlidePerCabinet_WhenSlidesAreUM() {

        // Arrange
        var cabinet = _builder.WithBoxOptions(new(CabinetDrawerBoxMaterial.FingerJointBirch, DrawerSlideType.UnderMount))
                                .WithQty(2)
                                .WithWidth(Dimension.FromMillimeters(500))
                                .WithHeight(Dimension.FromMillimeters(500))
                                .WithDepth(Dimension.FromMillimeters(500))
                                .Build();
        var expectedSupply = Supply.UndermountSlide(cabinet.Qty, Dimension.FromMillimeters(457));
        DovetailDrawerBoxBuilder.CabinetUnderMountDrawerSlideBoxDepths = new Dimension[] {
            Dimension.FromMillimeters(457)
        };

        // Act
        var supplies = cabinet.GetSupplies();

        // Assert
        supplies.Should().ContainEquivalentOf(expectedSupply);

    }

    [Fact]
    public void Should_IncludeOneSMSlidePerCabinet_WhenSlidesAreSM() {

        // Arrange
        var cabinet = _builder.WithBoxOptions(new(CabinetDrawerBoxMaterial.FingerJointBirch, DrawerSlideType.SideMount))
                                .WithQty(2)
                                .WithWidth(Dimension.FromMillimeters(500))
                                .WithHeight(Dimension.FromMillimeters(500))
                                .WithDepth(Dimension.FromMillimeters(500))
                                .Build();
        var expectedSupply = Supply.SidemountSlide(cabinet.Qty, Dimension.FromMillimeters(457));

        // Act
        var supplies = cabinet.GetSupplies();

        // Assert
        supplies.Should().ContainEquivalentOf(expectedSupply);

    }

}
