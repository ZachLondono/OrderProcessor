using ApplicationCore.Features.ClosetProCSVCutList;
using ApplicationCore.Features.ClosetProCSVCutList.CSVModels;
using ApplicationCore.Features.ClosetProCSVCutList.Products;
using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Shared.Domain;
using FluentAssertions;

namespace ApplicationCore.Tests.Unit.Orders.ClosetProOrderLoading;

public class ContinuousWallOfProductTests {

    private readonly ClosetProPartMapper _sut;
    private readonly ComponentBuilderFactory _factory;

    public ContinuousWallOfProductTests() {
        _factory = new();
        _sut = new(_factory);
    }

    [Fact]
    public void StandardSection() {

        // Arrange
        List<Part> parts = [
            CreateVerticalPanelPart(80, 12, "White", 1, 1),
            CreateVerticalPanelPart(80, 12, "Black", 1, 2),
            CreateVerticalPanelPart(80, 14, "Gold", 2, 1),
            CreateFixedShelfPart(80, 14, "Red", 2, 1),
            CreateAdjustableShelfPart(80, 14, "Blue", 2, 1),
        ];

        // Act 
        var products = _sut.MapPartsToProducts(parts, Dimension.Zero);

        // Assert
        products.OfType<VerticalPanel>().Should().HaveCount(3);
        products.OfType<Shelf>().Should().HaveCount(2);

    }

    [Fact]
    public void CubbySection() {

        // Arrange
        List<Part> parts = [
            CreateVerticalPanelPart(80, 12, "White", 1, 1),
            CreateFixedShelfPart(12, 12, "White", 1, 1, "Bottom "),
            CreateVerticalCubbyPart(12, 12, "White", 1, 1),
            CreateVerticalCubbyPart(12, 12, "White", 1, 1),
            CreateHorizontalCubbyPart(12, 12, "White", 1, 1),
            CreateHorizontalCubbyPart(12, 12, "White", 1, 1),
            CreateFixedShelfPart(12, 12, "White", 1, 1, "Top ")
        ];

        // Act 
        var products = _sut.MapPartsToProducts(parts, Dimension.Zero);

        // Assert
        products.OfType<VerticalPanel>().Should().HaveCount(1);
        products.OfType<DividerVerticalPanel>().Should().HaveCount(2);
        products.OfType<DividerShelf>().Should().HaveCount(2);
        products.OfType<Shelf>().Sum(p => p.Qty).Should().Be(6);

    }

    [Fact]
    public void TwoPartDoor() {

        // Arrange
        List<Part> parts = [
            CreateDoorRail(10, 10, "White", 1, 1),
            CreateDoorInsert(5, 5, "White", 1, 1),
        ];

        // Act
        var products = _sut.MapPartsToProducts(parts, Dimension.Zero);

        // Assert
        products.OfType<FivePieceFront>().Should().HaveCount(1);

        var door = products.OfType<FivePieceFront>().First();
        door.Type.Should().Be(DoorType.Door);
        door.Width.Should().Be(Dimension.FromInches(10));
        door.Height.Should().Be(Dimension.FromInches(10));
        door.Frame.LeftStile.Should().Be(Dimension.FromInches(2.5));
        door.Frame.RightStile.Should().Be(Dimension.FromInches(2.5));
        door.Frame.TopRail.Should().Be(Dimension.FromInches(2.5));
        door.Frame.BottomRail.Should().Be(Dimension.FromInches(2.5));

    }

    [Fact]
    public void TwoPartDrawer() {

        // Arrange
        List<Part> parts = [
            CreateDrawerFrontRail(7.5, 10, "White", 1, 1),
            CreateDrawerFrontInsert(5, 5, "White", 1, 1),
        ];

        // Act
        var products = _sut.MapPartsToProducts(parts, Dimension.Zero);

        // Assert
        products.OfType<FivePieceFront>().Should().HaveCount(1);

        var door = products.OfType<FivePieceFront>().First();
        door.Width.Should().Be(Dimension.FromInches(10));
        door.Height.Should().Be(Dimension.FromInches(7.5));
        door.Frame.LeftStile.Should().Be(Dimension.FromInches(2.5));
        door.Frame.RightStile.Should().Be(Dimension.FromInches(2.5));
        door.Frame.TopRail.Should().Be(Dimension.FromInches(1.25));
        door.Frame.BottomRail.Should().Be(Dimension.FromInches(1.25));

    }

    [Fact]
    public void WallWithBackPanel() {

        // Arrange
        List<Part> parts = [

            CreateVerticalPanelPart(80, 14, "White", 2, 1),
            CreateFixedShelfPart(20, 14, "White", 2, 1, "Bottom "),
            CreateAdjustableShelfPart(20, 13.75, "White", 2, 1),
            CreateFixedShelfPart(20, 13.75, "White", 2, 1),
            CreateFixedShelfPart(20, 14, "White", 2, 1, "Top "),
            CreateBackingPart(80, 14, "White", 2, 1),

            CreateVerticalPanelPart(80, 14, "White", 2, 2),
            CreateFixedShelfPart(20, 14, "White", 2, 2, "Bottom "),
            CreateAdjustableShelfPart(20, 14, "White", 2, 2),
            CreateFixedShelfPart(20, 14, "White", 2, 2),
            CreateFixedShelfPart(20, 14, "White", 2, 2, "Top "),
            CreateVerticalPanelPart(80, 14, "White", 2, 2),

        ];

        // Act
        var products = _sut.MapPartsToProducts(parts, Dimension.Zero);

        // Assert
        var verticalPanels = products.OfType<VerticalPanel>();
        verticalPanels.Should().HaveCount(3);
        verticalPanels.Where(v => v.ExtendBack).Should().HaveCount(3);

        products.OfType<MiscellaneousClosetPart>()
                .Where(m => m.Type == MiscellaneousType.Backing)
                .Should()
                .HaveCount(1);

        products.OfType<Shelf>()
                .Where(s => s.Type == ShelfType.Fixed && s.ExtendBack)
                .Should()
                .HaveCount(5);

        products.OfType<Shelf>()
                .Where(s => s.Type == ShelfType.Fixed && !s.ExtendBack)
                .Should()
                .HaveCount(1);

        products.OfType<Shelf>()
                .Where(s => s.Type == ShelfType.Adjustable && s.ExtendBack)
                .Should()
                .HaveCount(1);

        products.OfType<Shelf>()
                .Where(s => s.Type == ShelfType.Adjustable && !s.ExtendBack)
                .Should()
                .HaveCount(1);

    }

    private Part CreateVerticalPanelPart(double height, double depth, string materialColor, int wall, int section) {
        return new Part() {
            Depth = depth,
            Height = height,
            Color = materialColor,
            PartCost = "123.45",
            Quantity = 1,
            VertDrillL = depth,
            VertDrillR = depth,
            ExportName = "CPS FM Vert",
            VertHand = "T",
            WallNum = wall,
            SectionNum = section,
            InfoRecords = new() {
                new() {
                    PartName = "Edge Banding",
                    Color = materialColor
                }
            }
        };
    }

    private Part CreateFixedShelfPart(double width, double depth, string materialColor, int wall, int section, string partTypePrefix = "") {
        return new Part() {
            Depth = depth,
            Width = width,
            Color = materialColor,
            PartCost = "123.45",
            Quantity = 1,
            PartName = $"{partTypePrefix}Fixed Shelf",
            ExportName = "FixedShelf",
            WallNum = wall,
            SectionNum = section,
            InfoRecords = new() {
                new() {
                    PartName = "Edge Banding",
                    Color = materialColor
                }
            }
        };
    }

    private Part CreateAdjustableShelfPart(double width, double depth, string materialColor, int wall, int section) {
        return new Part() {
            Depth = depth,
            Width = width,
            Color = materialColor,
            PartCost = "123.45",
            Quantity = 1,
            PartName = "Adjustable Shelf",
            ExportName = "AdjustableShelf",
            WallNum = wall,
            SectionNum = section,
            InfoRecords = new() {
                new() {
                    PartName = "Edge Banding",
                    Color = materialColor
                }
            }
        };
    }

    private Part CreateVerticalCubbyPart(double height, double depth, string materialColor, int wall, int section) {
        return new Part() {
            Height = height,
            Depth = depth,
            Color = materialColor,
            PartCost = "123.45",
            Quantity = 1,
            PartName = "Cubby V Shelf",
            ExportName = "Cubby-V",
            WallNum = wall,
            SectionNum = section,
            InfoRecords = new() {
                new() {
                    PartName = "Edge Banding",
                    Color = materialColor
                }
            }
        };
    }

    private Part CreateHorizontalCubbyPart(double width, double depth, string materialColor, int wall, int section) {
        return new Part() {
            Depth = depth,
            Width = width,
            Color = materialColor,
            PartCost = "123.45",
            Quantity = 1,
            PartName = "Cubby H Shelf",
            ExportName = "Cubby-H",
            WallNum = wall,
            SectionNum = section,
            InfoRecords = new() {
                new() {
                    PartName = "Edge Banding",
                    Color = materialColor
                }
            }
        };
    }

    private Part CreateBackingPart(double height, double width, string materialColor, int wall, int section) {
        return new Part() {
            Height = height,
            Width = width,
            Color = materialColor,
            PartCost = "123.45",
            Quantity = 1,
            PartName = "Backing",
            ExportName = "Backing",
            WallNum = wall,
            SectionNum = section,
            InfoRecords = new() {
                new() {
                    PartName = "Edge Banding",
                    Color = materialColor
                }
            }
        };
    }

    private Part CreateDoorRail(double height, double width, string materialColor, int wall, int section) {
        return new Part() {
            Height = height,
            Width = width,
            Color = materialColor,
            PartCost = "123.45",
            Quantity = 1,
            PartName = "Cab Door Rail",
            ExportName = "Shaker",
            WallNum = wall,
            SectionNum = section
        };
    }

    private Part CreateDoorInsert(double height, double width, string materialColor, int wall, int section) {
        return new Part() {
            Height = height,
            Width = width,
            Color = materialColor,
            PartCost = "123.45",
            Quantity = 1,
            PartName = "Cab Door Insert",
            ExportName = "Shaker",
            WallNum = wall,
            SectionNum = section
        };
    }

    private Part CreateDrawerFrontRail(double height, double width, string materialColor, int wall, int section) {
        return new Part() {
            Height = height,
            Width = width,
            Color = materialColor,
            PartCost = "123.45",
            Quantity = 1,
            PartName = "Drawer XX Small Rail",
            ExportName = "Shaker",
            WallNum = wall,
            SectionNum = section
        };
    }

    private Part CreateDrawerFrontInsert(double height, double width, string materialColor, int wall, int section) {
        return new Part() {
            Height = height,
            Width = width,
            Color = materialColor,
            PartCost = "123.45",
            Quantity = 1,
            PartName = "Drawer XX Small Insert",
            ExportName = "Shaker",
            WallNum = wall,
            SectionNum = section
        };
    }

}
