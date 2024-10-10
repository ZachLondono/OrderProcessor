using Domain.Orders.Builders;
using Domain.Orders.Enums;
using Domain.Orders.ValueObjects;
using Domain.ValueObjects;
using FluentAssertions;

namespace Domain.Tests.Unit.Orders.Products.CabinetDrawerboxes;

[Collection("DrawerBoxBuilder")]
public class DrawerBaseCabinetDrawerBoxTests {

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
                                FaceHeights = faceHeights
                            })
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
    [InlineData(457, DrawerSlideType.UnderMount, 409, CabinetMaterialCore.ParticleBoard)]
    [InlineData(457, DrawerSlideType.SideMount, 393, CabinetMaterialCore.ParticleBoard)]
    [InlineData(457, DrawerSlideType.UnderMount, 411.8, CabinetMaterialCore.Plywood)]
    [InlineData(457, DrawerSlideType.SideMount, 395.8, CabinetMaterialCore.Plywood)]
    public void DrawerBoxWidthTest(double cabWidth, DrawerSlideType slideType, double expectedDrawerWidth, CabinetMaterialCore boxMatCore) {

        var cabinet = new DrawerBaseCabinetBuilder()
                            .WithDrawers(new() {
                                FaceHeights = new[] { Dimension.FromMillimeters(157) }
                            })
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
        var drawers = cabinet.GetDovetailDrawerBoxes(new DBBuilderFactoryFactory().CreateBuilderFactory());

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
                                FaceHeights = new[] { Dimension.FromMillimeters(drawerFaceHeight) }
                            })
                            .WithToeType(ToeType.LegLevelers)
                            .WithQty(1)
                            .WithWidth(Dimension.FromMillimeters(457))
                            .WithHeight(Dimension.FromMillimeters(876))
                            .WithDepth(Dimension.FromMillimeters(610))
                            .Build();

        // Act
        var drawers = cabinet.GetDovetailDrawerBoxes(new DBBuilderFactoryFactory().CreateBuilderFactory());

        // Assert
        drawers.Should().NotBeEmpty();
        drawers.First().Height.Should().Be(Dimension.FromMillimeters(expectedDrawerHeight));

    }

    [Fact]
    public void DrawerBoxHeight_WhenCabinetHasThreeDrawerBoxesA() {

        // Arrange
        Dimension cabinetHeight = Dimension.FromInches(30);
        Dimension[] faceHeights = [
            Dimension.FromInches(7),
            Dimension.FromInches(7),
            Dimension.FromInches(19.8125),
        ];
        Dimension[] expectedHeights = [
            Dimension.FromMillimeters(105),
            Dimension.FromMillimeters(137),
            Dimension.FromMillimeters(260),
        ];

        CompareExpectedDrawerBoxHeights(cabinetHeight, faceHeights, expectedHeights);

    }

    [Fact]
    public void DrawerBoxHeight_WhenCabinetHasThreeDrawerBoxesB() {

        Dimension cabinetHeight = Dimension.FromMillimeters(762);
        Dimension[] faceHeights = [
            Dimension.FromInches(7),
            Dimension.FromInches(7),
            Dimension.FromMillimeters(291.40),
        ];
        Dimension[] expectedHeights = [
            Dimension.FromMillimeters(105),
            Dimension.FromMillimeters(137),
            Dimension.FromMillimeters(210),
        ];

        CompareExpectedDrawerBoxHeights(cabinetHeight, faceHeights, expectedHeights);

    }

    [Fact]
    public void DrawerBoxHeight_WhenCabinetHasFourDrawerBoxes() {

        Dimension cabinetHeight = Dimension.FromMillimeters(762);
        Dimension[] faceHeights = [
            Dimension.FromInches(7),
            Dimension.FromInches(7),
            Dimension.FromInches(7),
            Dimension.FromMillimeters(224.6),
        ];
        Dimension[] expectedHeights = [
            Dimension.FromMillimeters(105),
            Dimension.FromMillimeters(137),
            Dimension.FromMillimeters(137),
            Dimension.FromMillimeters(159),
        ];

        CompareExpectedDrawerBoxHeights(cabinetHeight, faceHeights, expectedHeights);

    }

    private void CompareExpectedDrawerBoxHeights(Dimension cabinetHeight, Dimension[] faceHeights, Dimension[] expectedDrawerBoxHeights) {

        // Arrange
        var cabinet = new DrawerBaseCabinetBuilder()
                            .WithDrawers(new() {
                                FaceHeights = faceHeights 
                            })
                            .WithToeType(ToeType.NoToe)
                            .WithQty(1)
                            .WithHeight(cabinetHeight)
                            .WithWidth(Dimension.FromMillimeters(457))
                            .WithDepth(Dimension.FromMillimeters(610))
                            .Build();

        // Act
        var drawers = cabinet.GetDovetailDrawerBoxes(new DBBuilderFactoryFactory().CreateBuilderFactory())
                            .Select(db => db.Height);

        // Assert
        drawers.Should().ContainInOrder(expectedDrawerBoxHeights);
        drawers.Should().HaveCount(expectedDrawerBoxHeights.Length);

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
                                FaceHeights = new[] { Dimension.FromMillimeters(157) }
                            })
                            .WithBoxOptions(new(CabinetDrawerBoxMaterial.FingerJointBirch, slideType))
                            .WithToeType(ToeType.LegLevelers)
                            .WithQty(1)
                            .WithWidth(Dimension.FromMillimeters(457))
                            .WithHeight(Dimension.FromMillimeters(876))
                            .WithDepth(Dimension.FromMillimeters(cabDepth))
                            .Build();

        // Act
        var drawers = cabinet.GetDovetailDrawerBoxes(new DBBuilderFactoryFactory().CreateBuilderFactory(() => {
            DovetailDrawerBoxBuilder.CabinetUnderMountDrawerSlideBoxDepths = new Dimension[] {
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