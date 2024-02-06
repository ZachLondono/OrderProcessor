using Domain.Orders.Builders;
using Domain.Orders.Enums;
using Domain.Orders.ValueObjects;
using Domain.Orders.ValueObjects;
using Domain.ValueObjects;
using FluentAssertions;

namespace ApplicationCore.Tests.Unit.Orders.Products.Supplies;

[Collection("DrawerBoxBuilder")]
public class DrawerBaseCabinetSuppliesTests {

    private readonly DrawerBaseCabinetBuilder _builder;

    public DrawerBaseCabinetSuppliesTests() {

        _builder = new();

    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void Should_IncludeOneDrawerPullPerDrawer_AndOneSMSlidePerDrawer_WhenSlidesAreSM(int drawerQty) {

        // Arrange
        Dimension[] faceHeights = new Dimension[drawerQty];
        for (int i = 0; i < faceHeights.Length; i++) {
            faceHeights[i] = Dimension.FromMillimeters(157);
        }
        var cabinet = _builder.WithDrawers(new() {
            FaceHeights = faceHeights
        })
                                .WithBoxOptions(new(CabinetDrawerBoxMaterial.FingerJointBirch, DrawerSlideType.SideMount))
                                .WithWidth(Dimension.FromMillimeters(500))
                                .WithHeight(Dimension.FromMillimeters(157 * drawerQty + 10))
                                .WithDepth(Dimension.FromMillimeters(500))
                                .WithQty(2)
                                .Build();
        Supply expectedSupplyA = Supply.DrawerPull(cabinet.Qty * drawerQty);
        Supply expectedSupplyB = Supply.SidemountSlide(cabinet.Qty * drawerQty, Dimension.FromMillimeters(457));
        DovetailDrawerBoxBuilder.UnderMountDrawerSlideDepths = new Dimension[] {
            Dimension.FromMillimeters(457)
        };

        // Act
        var supplies = cabinet.GetSupplies();

        // Assert
        supplies.Should().ContainEquivalentOf(expectedSupplyA);
        supplies.Should().ContainEquivalentOf(expectedSupplyB);

    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void Should_IncludeOneDrawerPullPerDrawer_AndOneUMSlidePerDrawer_WhenSlidesAreUM(int drawerQty) {

        // Arrange
        Dimension[] faceHeights = new Dimension[drawerQty];
        for (int i = 0; i < faceHeights.Length; i++) {
            faceHeights[i] = Dimension.FromMillimeters(157);
        }
        var cabinet = _builder.WithDrawers(new() {
            FaceHeights = faceHeights
        })
                                .WithWidth(Dimension.FromMillimeters(500))
                                .WithHeight(Dimension.FromMillimeters(157 * drawerQty + 10))
                                .WithDepth(Dimension.FromMillimeters(500))
                                .WithQty(2)
                                .Build();

        Supply expectedSupplyA = Supply.DrawerPull(cabinet.Qty * drawerQty);
        Supply expectedSupplyB = Supply.UndermountSlide(cabinet.Qty * drawerQty, Dimension.FromMillimeters(457));
        DovetailDrawerBoxBuilder.UnderMountDrawerSlideDepths = new Dimension[] {
            Dimension.FromMillimeters(457)
        };

        // Act
        var supplies = cabinet.GetSupplies();

        // Assert
        supplies.Should().ContainEquivalentOf(expectedSupplyA);
        supplies.Should().ContainEquivalentOf(expectedSupplyB);

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

}
