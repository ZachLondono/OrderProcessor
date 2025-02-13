using FluentAssertions;
using OrderLoading.ClosetProCSVCutList;
using OrderLoading.ClosetProCSVCutList.CSVModels;

namespace OrderLoading.Tests.Unit.ClosetProOrderLoading;

public class SupplyTests {

    [Fact]
    public void ShouldNotIncludeDealerProvidedHinges() {

        // Arrange
        PickPart[] pickParts = [
            new PickPart() {
                Type = "Hinges",
                Name = "Dealer Provided Hinges",
                ExportName = "Dealer Provided Hinges",
                Color = "Chrome",
                Height = 0,
                Width = 0,
                Depth = 0,
                Quantity = 4,
                Cost = "(4 provided by dealer)"
            }
        ];

        // Act
        var hinges = ClosetProPartMapper.GetHingesFromPickList(pickParts);

        // Assert
        hinges.Should().BeEmpty();

    }

    [Fact]
    public void ShouldIncludeNonDealerProvidedHinges() {

        // Arrange
        PickPart[] pickParts = [
            new PickPart() {
                Type = "Hinges",
                Name = "Hinge-110",
                ExportName = "110 Std Soft Close",
                Color = "Chrome",
                Height = 0,
                Width = 0,
                Depth = 0,
                Quantity = 4,
                Cost = "$1234.56"
            }
        ];

        // Act
        var hinges = ClosetProPartMapper.GetHingesFromPickList(pickParts);

        // Assert
        hinges.Where(h => h.Description.Contains("Hinge-110")).Select(h => h.Qty).Sum().Should().Be(4);
        hinges.Where(h => h.Description.Contains("Hinge Plate")).Select(h => h.Qty).Sum().Should().Be(4);

    }

}
