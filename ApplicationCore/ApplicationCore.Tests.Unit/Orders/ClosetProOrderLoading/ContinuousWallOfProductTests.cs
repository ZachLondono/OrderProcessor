using ApplicationCore.Features.ClosetProCSVCutList;
using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.Components;
using ApplicationCore.Features.Orders.Shared.Domain.Products.Closets;
using ApplicationCore.Shared;
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
        List<Part> parts = new() {
            CreateVerticalPanelPart(80, 12, "White", 1, 1),
            CreateVerticalPanelPart(80, 12, "Black", 1, 2),
            CreateVerticalPanelPart(80, 14, "Gold", 2, 1),
            CreateFixedShelfPart(80, 14, "Red", 2, 1),
            CreateAdjustableShelfPart(80, 14, "Blue", 2, 1),
        };


        // Act 
        var products = _sut.MapPartsToProducts(parts);

        // Assert
        var closetParts = products.Where(p => p is ClosetPart)
                                    .Cast<ClosetPart>();
        closetParts.Should().Contain(p => p.SKU == "PC" && p.Material.Finish == "White");
        closetParts.Should().Contain(p => p.SKU == "PC" && p.Material.Finish == "Black");
        closetParts.Should().Contain(p => p.SKU == "PC" && p.Material.Finish == "Gold");
        closetParts.Should().Contain(p => p.SKU == _sut.Settings.FixedShelfSKU && p.Material.Finish == "Red");
        closetParts.Should().Contain(p => p.SKU == _sut.Settings.AdjustableShelfSKU && p.Material.Finish == "Blue");

    }

    [Fact]
    public void CubbySection() {

        // Arrange
        //_sut.Settings.DividerShelfDrillingType = HorizontalDividerPanelEndDrillingType.DoubleCams;

        List<Part> parts = new() {
            CreateVerticalPanelPart(80, 12, "White", 1, 1),
            CreateFixedShelfPart(12, 12, "White", 1, 1, "Bottom "),
            CreateVerticalCubbyPart(12, 12, "White", 1, 1),
            CreateVerticalCubbyPart(12, 12, "White", 1, 1),
            CreateHorizontalCubbyPart(12, 12, "White", 1, 1),
            CreateHorizontalCubbyPart(12, 12, "White", 1, 1),
            CreateFixedShelfPart(12, 12, "White", 1, 1, "Top ")
        };


        // Act 
        var products = _sut.MapPartsToProducts(parts);

        // Assert
        var closetParts = products.Where(p => p is ClosetPart)
                                    .Cast<ClosetPart>();

        closetParts.Should().Contain(p => p.SKU == "PC" && p.Qty == 1);
        closetParts.Should().Contain(p => p.SKU == "SF-D2T-D" && p.Qty == 1);
        closetParts.Should().Contain(p => p.SKU == "SF-D2B-D" && p.Qty == 1);

        var fixedShelves = closetParts.Where(p => p.SKU == "SF");
        fixedShelves.Sum(p => p.Qty).Should().Be(6);
        fixedShelves.Where(p => p.Length == Dimension.FromInches(3.5)).Sum(p => p.Qty).Should().Be(6);

        closetParts.Should().Contain(p => p.SKU == "PCDV-CAM-D" && p.Qty == 1);

    }

    [Fact]
    public void TwoPartDoor() {

        // Arrange
        List<Part> parts = new() {
            CreateDoorRail(10, 10, "White", 1, 1),
            CreateDoorInsert(5, 5, "White", 1, 1),
        };

        // Act
        var products = _sut.MapPartsToProducts(parts);

        // Assert
        var doors = products.Where(p => p is FivePieceDoor);
        doors.Should().HaveCount(1);

        var door = doors.First() as FivePieceDoor;
        door.Width.Should().Be(Dimension.FromInches(10));
        door.Height.Should().Be(Dimension.FromInches(10));

        door.FrameSize.LeftStile.Should().Be(Dimension.FromInches(2.5));
        door.FrameSize.RightStile.Should().Be(Dimension.FromInches(2.5));
        door.FrameSize.TopRail.Should().Be(Dimension.FromInches(2.5));
        door.FrameSize.BottomRail.Should().Be(Dimension.FromInches(2.5));

    }

    [Fact]
    public void TwoPartDrawer() {

        // Arrange
        List<Part> parts = new() {
            CreateDrawerFrontRail(7.5, 10, "White", 1, 1),
            CreateDrawerFrontInsert(5, 5, "White", 1, 1),
        };

        // Act
        var products = _sut.MapPartsToProducts(parts);

        // Assert
        var doors = products.Where(p => p is FivePieceDoor);
        doors.Should().HaveCount(1);

        var door = doors.First() as FivePieceDoor;
        door.Width.Should().Be(Dimension.FromInches(10));
        door.Height.Should().Be(Dimension.FromInches(7.5));

        door.FrameSize.LeftStile.Should().Be(Dimension.FromInches(2.5));
        door.FrameSize.RightStile.Should().Be(Dimension.FromInches(2.5));
        door.FrameSize.TopRail.Should().Be(Dimension.FromInches(1.25));
        door.FrameSize.BottomRail.Should().Be(Dimension.FromInches(1.25));

    }

    [Fact]
    public void WallWithBackPanel() {

        // Arrange
        List<Part> parts = new() {

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

        };

        // Act
        var products = _sut.MapPartsToProducts(parts);

        // Assert
        var closetParts = products.Where(p => p is ClosetPart).Cast<ClosetPart>();

        closetParts.Where(p => p.SKU == "PC")
                    .Where(p => p.Parameters.ContainsKey("ExtendBack"))
                    .Where(p => p.Parameters["ExtendBack"] == "19.05")
                    .Should()
                    .HaveCount(3);

        closetParts.Where(p => p.SKU == _sut.Settings.FixedShelfSKU)
                    .Where(p => !p.Parameters.ContainsKey("ExtendBack"))
                    .Should()
                    .HaveCount(1);

        closetParts.Where(p => p.SKU == _sut.Settings.FixedShelfSKU)
                    .Where(p => p.Parameters.ContainsKey("ExtendBack"))
                    .Where(p => p.Parameters["ExtendBack"] == "19.05")
                    .Should()
                    .HaveCount(5);

        closetParts.Where(p => p.SKU == _sut.Settings.AdjustableShelfSKU)
                    .Where(p => !p.Parameters.ContainsKey("ExtendBack"))
                    .Should()
                    .HaveCount(1);

        closetParts.Where(p => p.SKU == _sut.Settings.AdjustableShelfSKU)
                    .Where(p => p.Parameters.ContainsKey("ExtendBack"))
                    .Where(p => p.Parameters["ExtendBack"] == "19.05")
                    .Should()
                    .HaveCount(1);

        closetParts.Where(p => p.SKU == "BK34")
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

    private Part CreateVerticalCubbyPart(double width, double height, string materialColor, int wall, int section) {
        return new Part() {
            Height = height,
            Width = width,
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
