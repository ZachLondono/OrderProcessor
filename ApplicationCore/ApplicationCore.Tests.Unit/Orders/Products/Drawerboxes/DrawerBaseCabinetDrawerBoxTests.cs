using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Shared.Domain;
using FluentAssertions;

namespace ApplicationCore.Tests.Unit.Orders.Products.Drawerboxes;

public class DrawerBaseCabinetDrawerBoxTests {

    private readonly Func<DovetailDrawerBoxBuilder> _dovetailDBBuilderFactory;

    public DrawerBaseCabinetDrawerBoxTests() {

        _dovetailDBBuilderFactory = () => new();

    }

    [Theory]
    [InlineData(0, 1, 0)]
    [InlineData(1, 1, 1)]
    [InlineData(2, 1, 2)]
    [InlineData(3, 1, 3)]
    [InlineData(0, 3, 0)]
    [InlineData(1, 3, 3)]
    [InlineData(2, 3, 6)]
    [InlineData(3, 3, 9)]
    public void GetDrawerBoxes_ShouldReturnCorrectQuantityOfDrawerBoxes(int drawerCount, int cabQty, int expectedDrawerCount) {

        // Arrange

        var faceHeights = new Dimension[drawerCount];
        for (int i = 0; i < drawerCount; i++) {
            faceHeights[i] = Dimension.FromMillimeters(157);
        }

        var cabinet = new DrawerBaseCabinetBuilder()
                            .WithDrawers(new() {
                                BoxMaterial = CabinetDrawerBoxMaterial.FingerJointBirch,
                                SlideType = DrawerSlideType.UnderMount,
                                FaceHeights = faceHeights
                            })
                            .WithToeType(new LegLevelers())
                            .WithQty(cabQty)
                            .WithWidth(Dimension.FromMillimeters(456))
                            .WithHeight(Dimension.FromMillimeters(876))
                            .WithDepth(Dimension.FromMillimeters(610))
                            .Build();


        // Act
        var drawers = cabinet.GetDrawerBoxes(_dovetailDBBuilderFactory);

        // Assert
        drawers.Sum(d => d.Qty).Should().Be(expectedDrawerCount);

    }

    [Theory]
    [InlineData(457, DrawerSlideType.UnderMount, 409)]
    [InlineData(457, DrawerSlideType.SideMount, 393)]
    public void DrawerBoxWidthTest(double cabWidth, DrawerSlideType slideType, double expectedDrawerWidth) {

        var cabinet = new DrawerBaseCabinetBuilder()
                            .WithDrawers(new() {
                                BoxMaterial = CabinetDrawerBoxMaterial.FingerJointBirch,
                                SlideType = slideType,
                                FaceHeights = new[] { Dimension.FromMillimeters(157) }
                            })
                            .WithToeType(new LegLevelers())
                            .WithQty(1)
                            .WithWidth(Dimension.FromMillimeters(cabWidth))
                            .WithHeight(Dimension.FromMillimeters(876))
                            .WithDepth(Dimension.FromMillimeters(610))
                            .Build();

        // Act
        var drawers = cabinet.GetDrawerBoxes(_dovetailDBBuilderFactory);

        // Assert
        drawers.Should().NotBeEmpty();
        drawers.First().Width.Should().Be(Dimension.FromMillimeters(expectedDrawerWidth));

    }

    [Theory]
    [InlineData(105, 64)]
    [InlineData(157, 105)]
    [InlineData(200, 159)]
    public void DrawerBoxHeightTest(double drawerFaceHeight, double expectedDrawerHeight) {

        var cabinet = new DrawerBaseCabinetBuilder()
                            .WithDrawers(new() {
                                BoxMaterial = CabinetDrawerBoxMaterial.FingerJointBirch,
                                SlideType = DrawerSlideType.UnderMount,
                                FaceHeights = new[] { Dimension.FromMillimeters(drawerFaceHeight) }
                            })
                            .WithToeType(new LegLevelers())
                            .WithQty(1)
                            .WithWidth(Dimension.FromMillimeters(457))
                            .WithHeight(Dimension.FromMillimeters(876))
                            .WithDepth(Dimension.FromMillimeters(610))
                            .Build();

        // Act
        var drawers = cabinet.GetDrawerBoxes(_dovetailDBBuilderFactory);

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

        var cabinet = new DrawerBaseCabinetBuilder()
                            .WithDrawers(new() {
                                BoxMaterial = CabinetDrawerBoxMaterial.FingerJointBirch,
                                SlideType = slideType,
                                FaceHeights = new[] { Dimension.FromMillimeters(157) }
                            })
                            .WithToeType(new LegLevelers())
                            .WithQty(1)
                            .WithWidth(Dimension.FromMillimeters(457))
                            .WithHeight(Dimension.FromMillimeters(876))
                            .WithDepth(Dimension.FromMillimeters(cabDepth))
                            .Build();

        // Act
        var drawers = cabinet.GetDrawerBoxes(_dovetailDBBuilderFactory);

        // Assert
        drawers.Should().NotBeEmpty();
        drawers.First().Depth.AsMillimeters().Should().BeInRange(expectedDrawerDepth - accurracy / 2, expectedDrawerDepth + accurracy / 2);

    }

}