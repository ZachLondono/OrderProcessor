using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Shared.Domain;
using FluentAssertions;

namespace ApplicationCore.Tests.Unit.Orders.Products.Doors;

public class BaseCabinetDrawerBoxTests {

    private readonly Func<DovetailDrawerBoxBuilder> _dovetailDBBuilderFactory;

    public BaseCabinetDrawerBoxTests() {

        _dovetailDBBuilderFactory = () => new();

    }

    [Theory]
    [InlineData(0, 0, 1, 0)]
    [InlineData(1, 0, 1, 1)]
    [InlineData(0, 3, 1, 3)]
    [InlineData(2, 2, 1, 4)]
    [InlineData(1, 0, 3, 3)]
    [InlineData(0, 3, 3, 9)]
    [InlineData(2, 2, 3, 12)]
    public void GetDrawerBoxes_ShouldReturnCorrectQuantityOfDrawerBoxes(int drawerCount, int rolloutCount, int cabQty, int expectedDrawerCount) {

        // Arrange

        var rollOutPositions = new Dimension[rolloutCount];
        for (int i = 0; i < rolloutCount; i++) {
            rollOutPositions[i] = Dimension.FromMillimeters(10);
        }

        var cabinet = new BaseCabinetBuilder()
                            .WithDrawers(new() {
                                BoxMaterial = CabinetDrawerBoxMaterial.FingerJointBirch,
                                Quantity = drawerCount,
                                SlideType = DrawerSlideType.UnderMount,
                                FaceHeight = Dimension.FromMillimeters(157)
                            })
                            .WithInside(new(0, new RollOutOptions(rollOutPositions, false, RollOutBlockPosition.Both, DrawerSlideType.UnderMount, CabinetDrawerBoxMaterial.FingerJointBirch), ShelfDepth.Default))
                            .WithToeType(new LegLevelers())
                            .WithDoors(new() { Quantity = drawerCount })
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

        var cabinet = new BaseCabinetBuilder()
                            .WithDrawers(new() {
                                BoxMaterial = CabinetDrawerBoxMaterial.FingerJointBirch,
                                Quantity = 1,
                                SlideType = slideType,
                                FaceHeight = Dimension.FromMillimeters(157)
                            })
                            .WithInside(new(0, new RollOutOptions(Array.Empty<Dimension>(), false, RollOutBlockPosition.Both, DrawerSlideType.UnderMount, CabinetDrawerBoxMaterial.FingerJointBirch), ShelfDepth.Default))
                            .WithToeType(new LegLevelers())
                            .WithDoors(new() { Quantity = 1 })
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
    [InlineData(457, DrawerSlideType.UnderMount, RollOutBlockPosition.None, 409)]
    [InlineData(457, DrawerSlideType.UnderMount, RollOutBlockPosition.Left, 384)]
    [InlineData(457, DrawerSlideType.UnderMount, RollOutBlockPosition.Right, 384)]
    [InlineData(457, DrawerSlideType.UnderMount, RollOutBlockPosition.Both, 359)]
    [InlineData(457, DrawerSlideType.SideMount, RollOutBlockPosition.None, 393)]
    [InlineData(457, DrawerSlideType.SideMount, RollOutBlockPosition.Left, 368)]
    [InlineData(457, DrawerSlideType.SideMount, RollOutBlockPosition.Right, 368)]
    [InlineData(457, DrawerSlideType.SideMount, RollOutBlockPosition.Both, 343)]
    public void RollOutDrawerBoxWidthTest(double cabWidth, DrawerSlideType slideType, RollOutBlockPosition blockPositions, double expectedDrawerWidth) {

        var cabinet = new BaseCabinetBuilder()
                            .WithDrawers(new() {
                                BoxMaterial = CabinetDrawerBoxMaterial.FingerJointBirch,
                                Quantity = 0,
                                SlideType = slideType,
                                FaceHeight = Dimension.FromMillimeters(157)
                            })
                            .WithInside(new(0, new RollOutOptions(new Dimension[] { Dimension.FromMillimeters(19) }, false, blockPositions, slideType, CabinetDrawerBoxMaterial.FingerJointBirch), ShelfDepth.Default))
                            .WithToeType(new LegLevelers())
                            .WithDoors(new() { Quantity = 1 })
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

        var cabinet = new BaseCabinetBuilder()
                            .WithDrawers(new() {
                                BoxMaterial = CabinetDrawerBoxMaterial.FingerJointBirch,
                                Quantity = 1,
                                SlideType = DrawerSlideType.UnderMount,
                                FaceHeight = Dimension.FromMillimeters(drawerFaceHeight)
                            })
                            .WithInside(new(0, new RollOutOptions(Array.Empty<Dimension>(), false, RollOutBlockPosition.Both, DrawerSlideType.UnderMount, CabinetDrawerBoxMaterial.FingerJointBirch), ShelfDepth.Default))
                            .WithToeType(new LegLevelers())
                            .WithDoors(new() { Quantity = 1 })
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

        var cabinet = new BaseCabinetBuilder()
                            .WithDrawers(new() {
                                BoxMaterial = CabinetDrawerBoxMaterial.FingerJointBirch,
                                Quantity = 1,
                                SlideType = slideType,
                                FaceHeight = Dimension.FromMillimeters(157)
                            })
                            .WithInside(new(0, new RollOutOptions(Array.Empty<Dimension>(), false, RollOutBlockPosition.Both, DrawerSlideType.UnderMount, CabinetDrawerBoxMaterial.FingerJointBirch), ShelfDepth.Default))
                            .WithToeType(new LegLevelers())
                            .WithDoors(new() { Quantity = 1 })
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

        var cabinet = new BaseCabinetBuilder()
                            .WithDrawers(new() {
                                BoxMaterial = CabinetDrawerBoxMaterial.FingerJointBirch,
                                Quantity = 0,
                                SlideType = slideType,
                                FaceHeight = Dimension.FromMillimeters(157)
                            })
                            .WithInside(new(0, new RollOutOptions(new Dimension[] { Dimension.FromMillimeters(19) }, false, RollOutBlockPosition.Both, slideType, CabinetDrawerBoxMaterial.FingerJointBirch), ShelfDepth.Default))
                            .WithToeType(new LegLevelers())
                            .WithDoors(new() { Quantity = 1 })
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

public class BlindBaseCabinetDrawerBoxTests {

    private readonly Func<DovetailDrawerBoxBuilder> _dovetailDBBuilderFactory;

    public BlindBaseCabinetDrawerBoxTests() {

        _dovetailDBBuilderFactory = () => new();

    }

    [Theory]
    [InlineData(1, 1, 1)]
    [InlineData(2, 1, 2)]
    [InlineData(1, 2, 2)]
    [InlineData(2, 2, 4)]
    public void GetDrawerBoxes_ShouldReturnCorrectQuantityOfDrawerBoxes(int drawerCount, int cabQty, int expectedDrawerCount) {

        // Arrange
        var cabinet = new BlindBaseCabinetBuilder()
                            .WithDrawers(new() {
                                BoxMaterial = CabinetDrawerBoxMaterial.FingerJointBirch,
                                Quantity = drawerCount,
                                SlideType = DrawerSlideType.UnderMount,
                                FaceHeight = Dimension.FromMillimeters(157)
                            })
                            .WithToeType(new LegLevelers())
                            .WithDoors(new() { Quantity = drawerCount })
                            .WithBlindWidth(Dimension.FromMillimeters(635))
                            .WithQty(cabQty)
                            .WithWidth(Dimension.FromMillimeters(1067))
                            .WithHeight(Dimension.FromMillimeters(876))
                            .WithDepth(Dimension.FromMillimeters(600))
                            .Build();


        // Act
        var drawers = cabinet.GetDrawerBoxes(_dovetailDBBuilderFactory);

        // Assert
        drawers.Sum(d => d.Qty).Should().Be(expectedDrawerCount);

    }

    [Theory]
    [InlineData(1067, DrawerSlideType.UnderMount, 384)]
    [InlineData(1067, DrawerSlideType.SideMount, 368)]
    public void DrawerBoxWidthTest(double cabWidth, DrawerSlideType slideType, double expectedDrawerWidth) {

        var cabinet = new BlindBaseCabinetBuilder()
                            .WithDrawers(new() {
                                BoxMaterial = CabinetDrawerBoxMaterial.FingerJointBirch,
                                Quantity = 1,
                                SlideType = slideType,
                                FaceHeight = Dimension.FromMillimeters(157)
                            })
                            .WithToeType(new LegLevelers())
                            .WithDoors(new() { Quantity = 1 })
                            .WithBlindWidth(Dimension.FromMillimeters(635))
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

        var cabinet = new BlindBaseCabinetBuilder()
                            .WithDrawers(new() {
                                BoxMaterial = CabinetDrawerBoxMaterial.FingerJointBirch,
                                Quantity = 1,
                                SlideType = DrawerSlideType.UnderMount,
                                FaceHeight = Dimension.FromMillimeters(drawerFaceHeight)
                            })
                            .WithToeType(new LegLevelers())
                            .WithDoors(new() { Quantity = 1 })
                            .WithBlindWidth(Dimension.FromMillimeters(635))
                            .WithQty(1)
                            .WithWidth(Dimension.FromMillimeters(1067))
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

        var cabinet = new BlindBaseCabinetBuilder()
                            .WithDrawers(new() {
                                BoxMaterial = CabinetDrawerBoxMaterial.FingerJointBirch,
                                Quantity = 1,
                                SlideType = slideType,
                                FaceHeight = Dimension.FromMillimeters(157)
                            })
                            .WithToeType(new LegLevelers())
                            .WithDoors(new() { Quantity = 1 })
                            .WithBlindWidth(Dimension.FromMillimeters(635))
                            .WithQty(1)
                            .WithWidth(Dimension.FromMillimeters(1067))
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
