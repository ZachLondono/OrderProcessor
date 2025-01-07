using FluentAssertions;
using OrderLoading.ClosetProCSVCutList;
using OrderLoading.ClosetProCSVCutList.CSVModels;

namespace OrderLoading.Tests.Unit.ClosetProOrderLoading;

public class GlassShelvesTests {

    [Fact]
    public void GlassShelf() {

        int qty = 2;
        var unitPrice = 5m;
        var parts = new BuyOutPart[] {
            new() {
                WallNum = 1,
                SectionNum = 1,
                PartType = "Glass",
                PartName = "Glass Shelf",
                ExportName = "GlassShelf",
                Color = "Clear",
                Height = 0.375,
                Width = 24,
                Depth = 12,
                Quantity = qty,
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
                PartCost = $"${unitPrice * qty}",
                UnitL = "",
                UnitR = "",
                PartNum = 1,
            }
        };

        var items = ClosetProPartMapper.GetBuyOutGlassParts(parts);

        items.Should().HaveCount(1);
        var shelf = items.First();
        shelf.Qty.Should().Be(qty);
        shelf.UnitPrice.Should().Be(unitPrice);
        shelf.Description.Should().Contain("Glass Shelf");

    }

}
