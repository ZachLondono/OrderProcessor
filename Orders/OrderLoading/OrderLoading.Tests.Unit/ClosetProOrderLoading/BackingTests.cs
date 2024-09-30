using Domain.Orders.Builders;
using Domain.ValueObjects;
using FluentAssertions;
using OrderLoading.ClosetProCSVCutList;
using OrderLoading.ClosetProCSVCutList.CSVModels;
using OrderLoading.ClosetProCSVCutList.Products.Shelves;

namespace OrderLoading.Tests.Unit.ClosetProOrderLoading;

public class BackingTests {

    private readonly ClosetProPartMapper _sut;
    private readonly ComponentBuilderFactory _factory;

    public BackingTests() {
        _factory = new();
        _sut = new(_factory);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Bottom ")]
    [InlineData("Top ")]
    public void MapPartsToProducts_ShouldEnableExtendBackForFullDepthFixedShelves_WhenWallHasBackPanel(string prefix) {

        // Arrange
        List<Part> parts = [

            CreateVerticalPanelPart(80, 12, "White", 1, 1),

            CreateFixedShelfPart(30, 12, "White", 1, 1, prefix),

            CreateBackingPart(40, 30, "White", 1, 2),

        ];

        // Act 
        var products = _sut.MapPartsToProducts(parts, Dimension.Zero);

        // Assert
        products.OfType<Shelf>()
            .Should()
            .AllSatisfy(s =>
                s.ExtendBack.Should().BeTrue("Full depth fixed shelves should have 'Extended Back' enabled, so the holes line up with the vertical panels")
            );

    }

    [Theory]
    [InlineData("")]
    [InlineData("Bottom ")]
    [InlineData("Top ")]
    public void MapPartsToProducts_ShouldNotEnableExtendBackForPartialDepthFixedShelves_WhenWallHasBackPanel(string prefix) {

        // Arrange
        List<Part> parts = [

            CreateVerticalPanelPart(80, 12, "White", 1, 1),

            CreateBackingPart(40, 30, "White", 1, 2),

            CreateFixedShelfPart(30, 11.25, "White", 1, 1, prefix),

        ];

        // Act 
        var products = _sut.MapPartsToProducts(parts, Dimension.Zero);

        // Assert
        products.OfType<Shelf>()
            .Should()
            .AllSatisfy(s =>
                s.ExtendBack.Should().BeFalse("Fixed shelves in front of back panel should not have 'Extended Back' enabled")
            );

    }

    [Fact]
    public void MapPartsToProducts_ShouldNotEnableExtendBackForPartialDepthAdjShelves_WhenWallHasBackPanel() {

        // Arrange
        List<Part> parts = [

            CreateVerticalPanelPart(80, 12, "White", 1, 1),

            CreateBackingPart(40, 30, "White", 1, 2),

            CreateAdjustableShelfPart(30, 11.25, "White", 1, 1),

        ];

        // Act 
        var products = _sut.MapPartsToProducts(parts, Dimension.Zero);

        // Assert
        products.OfType<Shelf>()
            .Should()
            .AllSatisfy(s =>
                s.ExtendBack.Should().BeFalse("Adjustable shelves in front of back panel should not have 'Extended Back' enabled")
            );

    }

    [Fact]
    public void MapPartsToProducts_ShouldEnableExtendBackForFullDepthAdjShelves_WhenWallHasBackPanel() {

        // Arrange
        List<Part> parts = [

            CreateVerticalPanelPart(80, 12, "White", 1, 1),

            CreateBackingPart(40, 30, "White", 1, 2),

            CreateAdjustableShelfPart(30, 12, "White", 1, 1),

        ];

        // Act 
        var products = _sut.MapPartsToProducts(parts, Dimension.Zero);

        // Assert
        products.OfType<Shelf>()
            .Should()
            .AllSatisfy(s =>
                s.ExtendBack.Should().BeTrue("Full depth adjustable shelves should have 'Extended Back' enabled, so the holes line up with the vertical panels")
            );

    }

    [Fact]
    public void MapPartsToProducts_ShouldThrowException_WhenUnitContainsLFixedShelf() {

        // Arrange
        List<Part> parts = [

            CreateVerticalPanelPart(80, 12, "White", 1, 1),
            CreateVerticalPanelPart(80, 12, "White", 1, 2),

            CreateBackingPart(40, 30, "White", 1, 1),

            CreateLFixedShelfPart(0, 0, 0, "White", 1, 1),

        ];

        // Act 
        var action = () => _sut.MapPartsToProducts(parts, Dimension.Zero);

        // Assert
        action.Should().Throw<UnsupportedDesignException>();

    }

    [Fact]
    public void MapPartsToProducts_ShouldThrowException_WhenUnitContainsLAdjShelf() {

        // Arrange
        List<Part> parts = [

            CreateVerticalPanelPart(80, 12, "White", 1, 1),
            CreateVerticalPanelPart(80, 12, "White", 1, 2),

            CreateBackingPart(40, 30, "White", 1, 1),

            CreateLAdjShelfPart(0, 0, 0, "White", 1, 1),

        ];

        // Act 
        var action = () => _sut.MapPartsToProducts(parts, Dimension.Zero);

        // Assert
        action.Should().Throw<UnsupportedDesignException>();

    }

    [Fact]
    public void MapPartsToProducts_ShouldThrowException_WhenUnitContainsPieFixedShelf() {

        // Arrange
        List<Part> parts = [

            CreateVerticalPanelPart(80, 12, "White", 1, 1),
            CreateVerticalPanelPart(80, 12, "White", 1, 2),

            CreateBackingPart(40, 30, "White", 1, 1),

            CreatePieFixedShelfPart(0, 0, 0, "White", 1, 1),

        ];

        // Act 
        var action = () => _sut.MapPartsToProducts(parts, Dimension.Zero);

        // Assert
        action.Should().Throw<UnsupportedDesignException>();

    }

    [Fact]
    public void MapPartsToProducts_ShouldThrowException_WhenUnitContainsPieAdjShelf() {

        // Arrange
        List<Part> parts = [

            CreateVerticalPanelPart(80, 12, "White", 1, 1),
            CreateVerticalPanelPart(80, 12, "White", 1, 2),

            CreateBackingPart(40, 30, "White", 1, 1),

            CreatePieAdjShelfPart(0, 0, 0, "White", 1, 1),

        ];

        // Act 
        var action = () => _sut.MapPartsToProducts(parts, Dimension.Zero);

        // Assert
        action.Should().Throw<UnsupportedDesignException>();

    }



    /*
     * Factory methods for arranging tests
     */

    private static Part CreateVerticalPanelPart(double height, double depth, string materialColor, int wall, int section) {
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

    private static Part CreateFixedShelfPart(double width, double depth, string materialColor, int wall, int section, string partTypePrefix = "") {
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

    private static Part CreateAdjustableShelfPart(double width, double depth, string materialColor, int wall, int section) {
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

    private static Part CreateBackingPart(double height, double width, string materialColor, int wall, int section) {
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

    private static Part CreateLFixedShelfPart(double height, double width, double depth, string materialColor, int wall, int section) {
        return new Part() {
            Depth = depth,
            Height = height,
            Width = width,
            Color = materialColor,
            PartCost = "123.45",
            Quantity = 1,
            VertDrillL = depth,
            VertDrillR = depth,
            ExportName = "FixedShelf",
            PartName = "L Fixed Shelf",
            VertHand = "",
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

    private static Part CreateLAdjShelfPart(double height, double width, double depth, string materialColor, int wall, int section) {
        return new Part() {
            Depth = depth,
            Height = height,
            Width = width,
            Color = materialColor,
            PartCost = "123.45",
            Quantity = 1,
            VertDrillL = depth,
            VertDrillR = depth,
            ExportName = "AdjustableShelf",
            PartName = "L Adj Shelf",
            VertHand = "",
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

    private static Part CreatePieFixedShelfPart(double height, double width, double depth, string materialColor, int wall, int section) {
        return new Part() {
            Depth = depth,
            Height = height,
            Width = width,
            Color = materialColor,
            PartCost = "123.45",
            Quantity = 1,
            VertDrillL = depth,
            VertDrillR = depth,
            ExportName = "FixedShelf",
            PartName = "Pie Fixed Shelf",
            VertHand = "",
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

    private static Part CreatePieAdjShelfPart(double height, double width, double depth, string materialColor, int wall, int section) {
        return new Part() {
            Depth = depth,
            Height = height,
            Width = width,
            Color = materialColor,
            PartCost = "123.45",
            Quantity = 1,
            VertDrillL = depth,
            VertDrillR = depth,
            ExportName = "AdjustableShelf",
            PartName = "Pie Adj Shelf",
            VertHand = "",
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

}
