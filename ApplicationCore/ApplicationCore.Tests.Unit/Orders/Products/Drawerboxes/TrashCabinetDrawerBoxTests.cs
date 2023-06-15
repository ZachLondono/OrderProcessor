using ApplicationCore.Features.Orders.Shared.Domain;
using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Shared.Domain;
using FluentAssertions;

namespace ApplicationCore.Tests.Unit.Orders.Products.DrawerBoxes;

[Collection("DrawerBoxBuilder")]
public class TrashCabinetDrawerBoxTests {

    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 2)]
    public void GetDrawerBoxes_ShouldReturnCorrectQuantityOfDrawerBoxes(int cabQty, int expectedDrawerCount) {

        // Arrange
        var cabinet = new TrashCabinetBuilder()
                            .WithToeType(ToeType.LegLevelers)
                            .WithQty(cabQty)
                            .WithWidth(Dimension.FromMillimeters(456))
                            .WithHeight(Dimension.FromMillimeters(876))
                            .WithDepth(Dimension.FromMillimeters(610))
                            .Build();


        // Act
        var drawers = cabinet.GetDrawerBoxes(new DBBuilderFactoryFactory().CreateBuilderFactory());

        // Assert
        drawers.Sum(d => d.Qty).Should().Be(expectedDrawerCount);

    }

    [Theory]
    [InlineData(457, DrawerSlideType.UnderMount, 409, CabinetMaterialCore.ParticleBoard)]
    [InlineData(457, DrawerSlideType.SideMount, 393, CabinetMaterialCore.ParticleBoard)]
    [InlineData(457, DrawerSlideType.UnderMount, 411.8, CabinetMaterialCore.Plywood)]
    [InlineData(457, DrawerSlideType.SideMount, 395.8, CabinetMaterialCore.Plywood)]
    public void DrawerBoxWidthTest(double cabWidth, DrawerSlideType slideType, double expectedDrawerWidth, CabinetMaterialCore boxMatCore) {

        var cabinet = new TrashCabinetBuilder()
                            .WithBoxOptions(new(CabinetDrawerBoxMaterial.FingerJointBirch, slideType))
                            .WithToeType(ToeType.LegLevelers)
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

        var cabinet = new TrashCabinetBuilder()
                            .WithDrawerFaceHeight(Dimension.FromMillimeters(drawerFaceHeight))
                            .WithToeType(ToeType.LegLevelers)
                            .WithQty(1)
                            .WithWidth(Dimension.FromMillimeters(457))
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

        var cabinet = new TrashCabinetBuilder()
                            .WithBoxOptions(new(CabinetDrawerBoxMaterial.FingerJointBirch, slideType))
                            .WithToeType(ToeType.LegLevelers)
                            .WithQty(1)
                            .WithWidth(Dimension.FromMillimeters(457))
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
