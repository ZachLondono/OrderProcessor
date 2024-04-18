using Domain.ValueObjects;
using FluentAssertions;
using OrderLoading.ClosetProCSVCutList;
using OrderLoading.ClosetProCSVCutList.CSVModels;
using OrderLoading.ClosetProCSVCutList.Products;

namespace OrderLoading.Tests.Unit.ClosetProOrderLoading;

public class CornerShelfTests {

    [Fact]
    public void MapPartToCornerShelf() {

        // Arrange
        var part = CreatePart();

        // Act
        var shelf = ClosetProPartMapper.CreateFixedShelfFromPart(part, false, false, RoomNamingStrategy.None);

        // Arrange
        shelf.Should().BeOfType<CornerShelf>();
        var cornerShelf = (CornerShelf) shelf;

        cornerShelf.ProductLength.Should().Be(Dimension.FromInches(30));
        cornerShelf.NotchSideLength.Should().Be(Dimension.FromInches(32.125));
        cornerShelf.RightWidth.Should().Be(Dimension.FromInches(14));
        cornerShelf.ProductWidth.Should().Be(Dimension.FromInches(14));

    }

    public static Part CreatePart() {
        return new Part {
            WallNum = 3,
            SectionNum = 0,
            PartType = "Material",
            PartName = "L Fixed Shelf",
            ExportName = "FixedShelf",
            Color = "White",
            Height = 0.75,
            Width = 30,
            Depth = 32.125,
            Quantity = 1,
            VertHand = "",
            VertDrillL = 0,
            VertDrillR = 0,
            BBHeight = 0,
            BBDepth = 0,
            ShoeHeight = 0,
            ShoeDepth = 0,
            DrillLeft1 = "",
            DrillLeft2 = "",
            DrillRight1 = "",
            DrillRight2 = "",
            RailNotch = "N",
            RailNotchElevation = 0,
            CornerShelfSizes = "14|32.125|30|14",
            PartCost = "$34.24",
            UnitL = "",
            UnitR = "",
            PartNum = 13,
            InfoRecords = []
        };
    }

}
