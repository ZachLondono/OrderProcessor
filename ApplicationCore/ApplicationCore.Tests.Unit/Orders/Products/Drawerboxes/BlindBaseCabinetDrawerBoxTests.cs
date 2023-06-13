using ApplicationCore.Features.Orders.Shared.Domain;
using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Shared.Domain;
using FluentAssertions;

namespace ApplicationCore.Tests.Unit.Orders.Products.DrawerBoxes;

[Collection("DrawerBoxBuilder")]
public class BlindBaseCabinetDrawerBoxTests {

    [Theory]
    [InlineData(1, 1, 1)]
    [InlineData(2, 1, 2)]
    [InlineData(1, 2, 2)]
    [InlineData(2, 2, 4)]
    public void GetDrawerBoxes_ShouldReturnCorrectQuantityOfDrawerBoxes(int drawerCount, int cabQty, int expectedDrawerCount) {

        // Arrange
        var cabinet = new BlindBaseCabinetBuilder()
                            .WithDrawers(new() {
                                Quantity = drawerCount,
                                FaceHeight = Dimension.FromMillimeters(157)
                            })
                            .WithToeType(ToeType.LegLevelers)
                            .WithDoors(new() { Quantity = drawerCount })
                            .WithBlindWidth(Dimension.FromMillimeters(635))
                            .WithQty(cabQty)
                            .WithWidth(Dimension.FromMillimeters(1067))
                            .WithHeight(Dimension.FromMillimeters(876))
                            .WithDepth(Dimension.FromMillimeters(600))
                            .Build();


        // Act
        var drawers = cabinet.GetDrawerBoxes(new DBBuilderFactoryFactory().CreateBuilderFactory());

        // Assert
        drawers.Sum(d => d.Qty).Should().Be(expectedDrawerCount);

    }

    [Theory]
    [InlineData(1067, DrawerSlideType.UnderMount, 384, CabinetMaterialCore.ParticleBoard)]
    [InlineData(1067, DrawerSlideType.SideMount, 368, CabinetMaterialCore.ParticleBoard)]
    [InlineData(1067, DrawerSlideType.UnderMount, 386.8, CabinetMaterialCore.Plywood)]
    [InlineData(1067, DrawerSlideType.SideMount, 370.8, CabinetMaterialCore.Plywood)]
    public void DrawerBoxWidthTest(double cabWidth, DrawerSlideType slideType, double expectedDrawerWidth, CabinetMaterialCore boxMatCore) {

        var cabinet = new BlindBaseCabinetBuilder()
                            .WithDrawers(new() {
                                Quantity = 1,
                                FaceHeight = Dimension.FromMillimeters(157)
                            })
                            .WithBoxOptions(new(CabinetDrawerBoxMaterial.FingerJointBirch, slideType))
                            .WithToeType(ToeType.LegLevelers)
                            .WithDoors(new() { Quantity = 1 })
                            .WithBlindWidth(Dimension.FromMillimeters(635))
                            .WithQty(1)
                            .WithWidth(Dimension.FromMillimeters(cabWidth))
                            .WithHeight(Dimension.FromMillimeters(876))
                            .WithDepth(Dimension.FromMillimeters(610))
                            .WithBoxMaterial(new("White", CabinetMaterialFinishType.Melamine, boxMatCore))
                            .WithFinishMaterial(new("White", CabinetMaterialFinishType.Melamine, boxMatCore))
                            .Build();

        // Act
        var drawers = cabinet.GetDrawerBoxes(new DBBuilderFactoryFactory().CreateBuilderFactory());

        // Assert
        drawers.Should().NotBeEmpty();
        drawers.First().Width.Should().Be(Dimension.FromMillimeters(expectedDrawerWidth));

    }

    [Theory]
    [InlineData(105, 64)]
    [InlineData(157, 105)]
    [InlineData(200, 159)]
    public void DrawerBoxHeightTest(double drawerFaceHeight, double expectedDrawerHeight) {

        var cabinet = new BlindBaseCabinetBuilder()
                            .WithDrawers(new() {
                                Quantity = 1,
                                FaceHeight = Dimension.FromMillimeters(drawerFaceHeight)
                            })
                            .WithToeType(ToeType.LegLevelers)
                            .WithDoors(new() { Quantity = 1 })
                            .WithBlindWidth(Dimension.FromMillimeters(635))
                            .WithQty(1)
                            .WithWidth(Dimension.FromMillimeters(1067))
                            .WithHeight(Dimension.FromMillimeters(876))
                            .WithDepth(Dimension.FromMillimeters(610))
                            .Build();

        // Act
        var drawers = cabinet.GetDrawerBoxes(new DBBuilderFactoryFactory().CreateBuilderFactory());

        // Assert
        drawers.Should().NotBeEmpty();
        drawers.First().Height.Should().Be(Dimension.FromMillimeters(expectedDrawerHeight));

    }

    [Theory]
    [InlineData(800, DrawerSlideType.UnderMount, 533)]
    [InlineData(700, DrawerSlideType.UnderMount, 533)]
    [InlineData(600, DrawerSlideType.UnderMount, 533)]
    [InlineData(500, DrawerSlideType.UnderMount, 457)]
    [InlineData(400, DrawerSlideType.UnderMount, 305)]
    [InlineData(800, DrawerSlideType.SideMount, 762, 2)]
    [InlineData(700, DrawerSlideType.SideMount, 660.4, 2)]
    [InlineData(600, DrawerSlideType.SideMount, 558, 2)]
    [InlineData(500, DrawerSlideType.SideMount, 457.2, 2)]
    [InlineData(400, DrawerSlideType.SideMount, 355.6, 2)]
    public void DrawerBoxDepthTest(double cabDepth, DrawerSlideType slideType, double expectedDrawerDepth, int accurracy = 0) {

        var cabinet = new BlindBaseCabinetBuilder()
                            .WithDrawers(new() {
                                Quantity = 1,
                                FaceHeight = Dimension.FromMillimeters(157),
                            })
                            .WithBoxOptions(new(CabinetDrawerBoxMaterial.FingerJointBirch, slideType))
                            .WithToeType(ToeType.LegLevelers)
                            .WithDoors(new() { Quantity = 1 })
                            .WithBlindWidth(Dimension.FromMillimeters(635))
                            .WithQty(1)
                            .WithWidth(Dimension.FromMillimeters(1067))
                            .WithHeight(Dimension.FromMillimeters(876))
                            .WithDepth(Dimension.FromMillimeters(cabDepth))
                            .Build();

        // Act
        var drawers = cabinet.GetDrawerBoxes(new DBBuilderFactoryFactory().CreateBuilderFactory(() => {
            DovetailDrawerBoxBuilder.UnderMountDrawerSlideDepths = new Dimension[] {
                Dimension.FromMillimeters(533),
                Dimension.FromMillimeters(457),
                Dimension.FromMillimeters(305),
            };
        }));

        // Assert
        drawers.Should().NotBeEmpty();
        drawers.First().Depth.AsMillimeters().Should().BeInRange(expectedDrawerDepth - accurracy / 2, expectedDrawerDepth + accurracy / 2);

    }

}
