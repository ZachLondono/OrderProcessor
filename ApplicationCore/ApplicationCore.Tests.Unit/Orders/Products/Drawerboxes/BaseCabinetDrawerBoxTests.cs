using ApplicationCore.Features.Orders.Shared.Domain;
using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Shared.Domain;
using FluentAssertions;

namespace ApplicationCore.Tests.Unit.Orders.Products.Drawerboxes;

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
                                Quantity = drawerCount,
                                FaceHeight = Dimension.FromMillimeters(157),
                            })
                            .WithBoxOptions(new(CabinetDrawerBoxMaterial.FingerJointBirch, DrawerSlideType.UnderMount))
                            .WithInside(new(0, new RollOutOptions(rollOutPositions, false, RollOutBlockPosition.Both), ShelfDepth.Default))
                            .WithToeType(ToeType.LegLevelers)
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
    [InlineData(457, DrawerSlideType.UnderMount, 409, CabinetMaterialCore.ParticleBoard)]
    [InlineData(457, DrawerSlideType.SideMount, 393, CabinetMaterialCore.ParticleBoard)]
    [InlineData(457, DrawerSlideType.UnderMount, 411.8, CabinetMaterialCore.Plywood)]
    [InlineData(457, DrawerSlideType.SideMount, 395.8, CabinetMaterialCore.Plywood)]
    public void DrawerBoxWidthTest(double cabWidth, DrawerSlideType slideType, double expectedDrawerWidth, CabinetMaterialCore boxMatCore) {

        var cabinet = new BaseCabinetBuilder()
                            .WithDrawers(new() {
                                Quantity = 1,
                                FaceHeight = Dimension.FromMillimeters(157)
                            })
                            .WithBoxOptions(new(CabinetDrawerBoxMaterial.SolidBirch, slideType))
                            .WithInside(new(0, new RollOutOptions(Array.Empty<Dimension>(), false, RollOutBlockPosition.Both), ShelfDepth.Default))
                            .WithToeType(ToeType.LegLevelers)
                            .WithDoors(new() { Quantity = 1 })
                            .WithQty(1)
                            .WithWidth(Dimension.FromMillimeters(cabWidth))
                            .WithHeight(Dimension.FromMillimeters(876))
                            .WithDepth(Dimension.FromMillimeters(610))
                            .WithBoxMaterial(new("White", CabinetMaterialFinishType.Melamine, boxMatCore))
                            .WithFinishMaterial(new("White", CabinetMaterialFinishType.Melamine, boxMatCore))
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
                                Quantity = 0,
                                FaceHeight = Dimension.FromMillimeters(157),
                            })
                            .WithBoxOptions(new(CabinetDrawerBoxMaterial.SolidBirch, slideType))
                            .WithInside(new(0, new RollOutOptions(new Dimension[] { Dimension.FromMillimeters(19) }, false, blockPositions), ShelfDepth.Default))
                            .WithToeType(ToeType.LegLevelers)
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
                                Quantity = 1,
                                FaceHeight = Dimension.FromMillimeters(drawerFaceHeight)
                            })
                            .WithBoxOptions(new(CabinetDrawerBoxMaterial.FingerJointBirch, DrawerSlideType.UnderMount))
                            .WithInside(new(0, new RollOutOptions(Array.Empty<Dimension>(), false, RollOutBlockPosition.Both), ShelfDepth.Default))
                            .WithToeType(ToeType.LegLevelers)
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
                                Quantity = 1,
                                FaceHeight = Dimension.FromMillimeters(157)
                            })
                            .WithBoxOptions(new(CabinetDrawerBoxMaterial.FingerJointBirch, slideType))
                            .WithInside(new(0, new RollOutOptions(Array.Empty<Dimension>(), false, RollOutBlockPosition.Both), ShelfDepth.Default))
                            .WithToeType(ToeType.LegLevelers)
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
                                Quantity = 0,
                                FaceHeight = Dimension.FromMillimeters(157)
                            })
                            .WithBoxOptions(new(CabinetDrawerBoxMaterial.FingerJointBirch, slideType))
                            .WithInside(new(0, new RollOutOptions(new Dimension[] { Dimension.FromMillimeters(19) }, false, RollOutBlockPosition.Both), ShelfDepth.Default))
                            .WithToeType(ToeType.LegLevelers)
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
