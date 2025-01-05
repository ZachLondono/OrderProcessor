using FluentAssertions;
using OrderLoading.ClosetProCSVCutList;
using OrderLoading.ClosetProCSVCutList.CSVModels;

namespace OrderLoading.Tests.Unit.ClosetProOrderLoading;

public class FixedShelfLightingTests {

    [Fact]
    public void FixedShelfNoLED() {

        var part = CreateFixedShelfPart();

        var shelf = ClosetProPartMapper.CreateFixedShelf(part, false, false, RoomNamingStrategy.None);

        shelf.LEDChannel.Should().BeFalse();

    }

    private Part CreateFixedShelfPart() {
        return new Part() {
            WallNum = 1,
            SectionNum = 1,
            PartType = "Material",
            PartName = "Fixed Shelf",
            ExportName = "FixedShelf",
            Color = "White",
            Height = 0.75,
            Width = 24,
            Depth = 12.5,
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
            CornerShelfSizes = "",
            PartCost = "$0.00",
            UnitL = "FALSE",
            UnitR = "",
            PartNum = 1,
            InfoRecords = new() {
                new() {
                    PartName = "Edge Banding",
                    Color = "White" 
                }
            }
        };
    }

}
