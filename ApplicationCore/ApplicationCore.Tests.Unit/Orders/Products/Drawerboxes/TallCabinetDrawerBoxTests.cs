using ApplicationCore.Features.Orders.Shared.Domain;
using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using Domain.ValueObjects;
using FluentAssertions;

namespace ApplicationCore.Tests.Unit.Orders.Products.DrawerBoxes;

[Collection("DrawerBoxBuilder")]
public class TallCabinetDrawerBoxTests {

    [Theory]
    [InlineData(0, 1, 0)]
    [InlineData(3, 1, 3)]
    [InlineData(2, 1, 2)]
    [InlineData(0, 3, 0)]
    [InlineData(3, 3, 9)]
    [InlineData(2, 3, 6)]
    public void GetDrawerBoxes_ShouldReturnCorrectQuantityOfDrawerBoxes(int rolloutCount, int cabQty, int expectedDrawerCount) {

        // Arrange

        var rollOutPositions = new Dimension[rolloutCount];
        for (int i = 0; i < rolloutCount; i++) {
            rollOutPositions[i] = Dimension.FromMillimeters(10);
        }

        var cabinet = new TallCabinetBuilder()
                            .WithInside(new(0, 0, 0, new RollOutOptions(rollOutPositions, false, RollOutBlockPosition.Both)))
                            .WithToeType(ToeType.LegLevelers)
                            .WithQty(cabQty)
                            .WithWidth(Dimension.FromMillimeters(456))
                            .WithHeight(Dimension.FromMillimeters(876))
                            .WithDepth(Dimension.FromMillimeters(610))
                            .Build();


        // Act
        var drawers = cabinet.GetDovetailDrawerBoxes(new DBBuilderFactoryFactory().CreateBuilderFactory());

        // Assert
        drawers.Sum(d => d.Qty).Should().Be(expectedDrawerCount);

    }

    [Theory]
    [InlineData(457, DrawerSlideType.UnderMount, RollOutBlockPosition.None, 409)]
    [InlineData(457, DrawerSlideType.UnderMount, RollOutBlockPosition.Left, 384)]
    [InlineData(457, DrawerSlideType.UnderMount, RollOutBlockPosition.Right, 384)]
    [InlineData(457, DrawerSlideType.UnderMount, RollOutBlockPosition.Both, 359)]
    [InlineData(457, DrawerSlideType.SideMount, RollOutBlockPosition.None, 393)]
    [InlineData(457, DrawerSlideType.SideMount, RollOutBlockPosition.Left, 368)]
    [InlineData(457, DrawerSlideType.SideMount, RollOutBlockPosition.Right, 368)]
    [InlineData(457, DrawerSlideType.SideMount, RollOutBlockPosition.Both, 343)]
    public void RollOutDrawerBoxWidthTest(double cabWidth, DrawerSlideType slideType, RollOutBlockPosition blockPositions, double expectedDrawerWidth) {

        var cabinet = new TallCabinetBuilder()
                            .WithInside(new(0, 0, 0, new RollOutOptions(new Dimension[] { Dimension.FromMillimeters(19) }, false, blockPositions)))
                            .WithBoxOptions(new(CabinetDrawerBoxMaterial.FingerJointBirch, slideType))
                            .WithToeType(ToeType.LegLevelers)
                            .WithQty(1)
                            .WithWidth(Dimension.FromMillimeters(cabWidth))
                            .WithHeight(Dimension.FromMillimeters(876))
                            .WithDepth(Dimension.FromMillimeters(610))
                            .Build();

        // Act
        var drawers = cabinet.GetDovetailDrawerBoxes(new DBBuilderFactoryFactory().CreateBuilderFactory());

        // Assert
        drawers.Should().NotBeEmpty();
        drawers.First().Width.Should().Be(Dimension.FromMillimeters(expectedDrawerWidth));

    }

    [Theory]
    [InlineData(800, DrawerSlideType.UnderMount, 533)]
    [InlineData(700, DrawerSlideType.UnderMount, 533)]
    [InlineData(600, DrawerSlideType.UnderMount, 533)]
    [InlineData(500, DrawerSlideType.UnderMount, 457)]
    [InlineData(400, DrawerSlideType.UnderMount, 305)]
    [InlineData(800, DrawerSlideType.SideMount, 750, 2)]
    [InlineData(700, DrawerSlideType.SideMount, 650, 2)]
    [InlineData(600, DrawerSlideType.SideMount, 550, 2)]
    [InlineData(500, DrawerSlideType.SideMount, 450, 2)]
    [InlineData(400, DrawerSlideType.SideMount, 350, 2)]
    public void RollOutDrawerBoxDepthTest(double cabDepth, DrawerSlideType slideType, double expectedDrawerDepth, int accurracy = 0) {

        var cabinet = new TallCabinetBuilder()
                            .WithInside(new(0, 0, 0, new RollOutOptions(new Dimension[] { Dimension.FromMillimeters(19) }, false, RollOutBlockPosition.Both)))
                            .WithBoxOptions(new(CabinetDrawerBoxMaterial.FingerJointBirch, slideType))
                            .WithToeType(ToeType.LegLevelers)
                            .WithQty(1)
                            .WithWidth(Dimension.FromMillimeters(457))
                            .WithHeight(Dimension.FromMillimeters(876))
                            .WithDepth(Dimension.FromMillimeters(cabDepth))
                            .Build();

        // Act
        var drawers = cabinet.GetDovetailDrawerBoxes(new DBBuilderFactoryFactory().CreateBuilderFactory(() => {
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