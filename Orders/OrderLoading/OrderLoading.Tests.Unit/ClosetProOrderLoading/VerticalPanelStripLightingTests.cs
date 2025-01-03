using Domain.ValueObjects;
using FluentAssertions;
using OrderLoading.ClosetProCSVCutList;
using OrderLoading.ClosetProCSVCutList.CSVModels;
using OrderLoading.ClosetProCSVCutList.Products.Verticals;

namespace OrderLoading.Tests.Unit.ClosetProOrderLoading;

public class VerticalPanelStripLightingTests {

    [Fact]
    public void NoStripLighting() {

        var part = CreateVerticalPanelPart("");

        var panel = ClosetProPartMapper.CreateVerticalPanel(part, false, RoomNamingStrategy.None);

        panel.LEDChannel.Should().Be(VerticalPanelLEDChannel.None);

    }

    [Fact]
    public void LeftStripLighting() {

        var part = CreateVerticalPanelPart("L");

        var panel = ClosetProPartMapper.CreateVerticalPanel(part, false, RoomNamingStrategy.None);

        panel.LEDChannel.Should().Be(VerticalPanelLEDChannel.Left);

    }

    [Fact]
    public void RightStripLighting() {

        var part = CreateVerticalPanelPart("R");

        var panel = ClosetProPartMapper.CreateVerticalPanel(part, false, RoomNamingStrategy.None);

        panel.LEDChannel.Should().Be(VerticalPanelLEDChannel.Right);

    }

    [Fact]
    public void BothSidesStripLighting() {

        var part = CreateVerticalPanelPart("B");

        var panel = ClosetProPartMapper.CreateVerticalPanel(part, false, RoomNamingStrategy.None);

        panel.LEDChannel.Should().Be(VerticalPanelLEDChannel.Both);

    }

    [Fact]
    public void UnexpectedValue() {

        var part = CreateVerticalPanelPart("Any Other Value");

        var action = () => ClosetProPartMapper.CreateVerticalPanelFromPart(part, false, RoomNamingStrategy.None);

        action.Should().Throw<InvalidOperationException>();

    }

    [Fact]
    public void NoStripLighting_TransitionPanel() {

        var part = CreateTransitionPanelPart("");

        var panel = ClosetProPartMapper.CreateTransitionVerticalPanel(part, false, RoomNamingStrategy.None);

        panel.LEDChannel.Should().Be(VerticalPanelLEDChannel.None);

    }

    [Fact]
    public void LeftStripLighting_TransitionPanel() {

        var part = CreateTransitionPanelPart("L");

        var panel = ClosetProPartMapper.CreateTransitionVerticalPanel(part, false, RoomNamingStrategy.None);

        panel.LEDChannel.Should().Be(VerticalPanelLEDChannel.Left);

    }

    [Fact]
    public void RightStripLighting_TransitionPanel() {

        var part = CreateTransitionPanelPart("R");

        var panel = ClosetProPartMapper.CreateTransitionVerticalPanel(part, false, RoomNamingStrategy.None);

        panel.LEDChannel.Should().Be(VerticalPanelLEDChannel.Right);

    }

    [Fact]
    public void BothSidesStripLighting_TransitionPanel() {

        var part = CreateTransitionPanelPart("B");

        var panel = ClosetProPartMapper.CreateTransitionVerticalPanel(part, false, RoomNamingStrategy.None);

        panel.LEDChannel.Should().Be(VerticalPanelLEDChannel.Both);

    }

    [Fact]
    public void UnexpectedValue_TransitionPanel() {

        var part = CreateTransitionPanelPart("Any Other Value");

        var action = () => ClosetProPartMapper.CreateTransitionVerticalPanel(part, false, RoomNamingStrategy.None);

        action.Should().Throw<InvalidOperationException>();

    }

    [Fact]
    public void NoStripLighting_HutchPanel() {

        var part = CreateHutchPanelPart("");

        var panel = ClosetProPartMapper.CreateHutchVerticalPanel(part, false, RoomNamingStrategy.None);

        panel.LEDChannel.Should().Be(VerticalPanelLEDChannel.None);

    }

    [Fact]
    public void LeftStripLighting_HutchPanel() {

        var part = CreateHutchPanelPart("L");

        var panel = ClosetProPartMapper.CreateHutchVerticalPanel(part, false, RoomNamingStrategy.None);

        panel.LEDChannel.Should().Be(VerticalPanelLEDChannel.Left);

    }

    [Fact]
    public void RightStripLighting_HutchPanel() {

        var part = CreateHutchPanelPart("R");

        var panel = ClosetProPartMapper.CreateHutchVerticalPanel(part, false, RoomNamingStrategy.None);

        panel.LEDChannel.Should().Be(VerticalPanelLEDChannel.Right);

    }

    [Fact]
    public void BothSidesStripLighting_HutchPanel() {

        var part = CreateHutchPanelPart("B");

        var panel = ClosetProPartMapper.CreateHutchVerticalPanel(part, false, RoomNamingStrategy.None);

        panel.LEDChannel.Should().Be(VerticalPanelLEDChannel.Both);

    }

    [Fact]
    public void UnexpectedValue_HutchPanel() {

        var part = CreateHutchPanelPart("Any Other Value");

        var action = () => ClosetProPartMapper.CreateHutchVerticalPanel(part, false, RoomNamingStrategy.None);

        action.Should().Throw<InvalidOperationException>();

    }

    [Fact]
    public void NoStripLighting_IslandPanel() {

        var part = CreateIslandPanelPart("");

        var panel = ClosetProPartMapper.CreateIslandVerticalPanel(part, RoomNamingStrategy.None);

        panel.LEDChannel.Should().Be(VerticalPanelLEDChannel.None);

    }

    [Fact]
    public void LeftStripLighting_IslandPanel() {

        var part = CreateIslandPanelPart("L");

        var panel = ClosetProPartMapper.CreateIslandVerticalPanel(part, RoomNamingStrategy.None);

        panel.LEDChannel.Should().Be(VerticalPanelLEDChannel.Left);

    }

    [Fact]
    public void RightStripLighting_IslandPanel() {

        var part = CreateIslandPanelPart("R");

        var panel = ClosetProPartMapper.CreateIslandVerticalPanel(part, RoomNamingStrategy.None);

        panel.LEDChannel.Should().Be(VerticalPanelLEDChannel.Right);

    }

    [Fact]
    public void BothSidesStripLighting_IslandPanel() {

        var part = CreateIslandPanelPart("B");

        var panel = ClosetProPartMapper.CreateIslandVerticalPanel(part, RoomNamingStrategy.None);

        panel.LEDChannel.Should().Be(VerticalPanelLEDChannel.Both);

    }

    [Fact]
    public void UnexpectedValue_IslandPanel() {

        var part = CreateIslandPanelPart("Any Other Value");

        var action = () => ClosetProPartMapper.CreateIslandVerticalPanel(part, RoomNamingStrategy.None);

        action.Should().Throw<InvalidOperationException>();

    }

    public static Part CreateVerticalPanelPart(string cornerShelfSizes) {
        return new Part {
            WallNum = 1,
            SectionNum = 1,
            PartType = "Material",
            PartName = "Vertical Panel - Floor",
            ExportName = "CPS FM Vert",
            Color = "White",
            Height = 83.898,
            Width = 0.75,
            Depth = 14,
            Quantity = 1,
            VertHand = "L",
            VertDrillL = 0,
            VertDrillR = 14,
            BBHeight = 0,
            BBDepth = 0,
            ShoeHeight = 0,
            ShoeDepth = 0,
            DrillLeft1 = "",
            DrillLeft2 = "",
            DrillRight1 = "0.0000|83.8980",
            DrillRight2 = "",
            RailNotch = "N",
            RailNotchElevation = 0,
            CornerShelfSizes = cornerShelfSizes,
            PartCost = "$24.41",
            UnitL = "",
            UnitR = "",
            PartNum = 1,
            InfoRecords = []
        };
    }

    public static Part CreateTransitionPanelPart(string cornerShelfSizes) {
        return new Part {
            WallNum = 1,
            SectionNum = 1,
            PartType = "Material",
            PartName = "Vertical Panel - Floor",
            ExportName = "CPS FM Vert",
            Color = "White",
            Height = 83.898,
            Width = 0.75,
            Depth = 14,
            Quantity = 1,
            VertHand = "L",
            VertDrillL = 12,
            VertDrillR = 14,
            BBHeight = 0,
            BBDepth = 0,
            ShoeHeight = 0,
            ShoeDepth = 0,
            DrillLeft1 = "",
            DrillLeft2 = "",
            DrillRight1 = "0.0000|83.8980",
            DrillRight2 = "",
            RailNotch = "N",
            RailNotchElevation = 0,
            CornerShelfSizes = cornerShelfSizes,
            PartCost = "$24.41",
            UnitL = "",
            UnitR = "",
            PartNum = 1,
            InfoRecords = []
        };
    }

    public static Part CreateHutchPanelPart(string cornerShelfSizes) {

        Dimension panelHeight = Dimension.FromMillimeters(2259);
        Dimension panelDepth = Dimension.FromInches(14);
        Dimension leftDrilling = Dimension.FromInches(14);
        Dimension rightDrilling = Dimension.FromInches(0);
        Dimension expectedBaseNotchDepth = Dimension.FromInches(1.5);
        Dimension expectedBaseNotchHeight = Dimension.FromInches(10);
        Dimension expectedHutchTopDepth = Dimension.FromInches(12);
        Dimension expectedHutchDwrPanelHeight = Dimension.FromInches(45);

        return new Part() {
            Depth = panelDepth.AsInches(),
            Height = panelHeight.AsInches(),
            Color = "White",
            PartCost = "123.45",
            Quantity = 123,
            VertDrillL = leftDrilling.AsInches(),
            VertDrillR = rightDrilling.AsInches(),
            PartName = "Vertical Panel - Hutch",
            ExportName = "VP-Hutch",
            VertHand = "Right",
            BBDepth = expectedBaseNotchDepth.AsInches(),
            BBHeight = expectedBaseNotchHeight.AsInches(),
            DrillLeft1 = $"0|{expectedHutchTopDepth.AsInches()}",
            DrillLeft2 = $"0|{panelHeight.AsInches()}",
            UnitL = "",
            UnitR = $"{panelDepth.AsInches()}|{expectedHutchDwrPanelHeight.AsInches()}|{expectedHutchTopDepth.AsInches()}|{(panelHeight - expectedHutchDwrPanelHeight).AsInches()}",
            CornerShelfSizes = cornerShelfSizes,
            InfoRecords = new() {
                new() {
                    PartName = "Edge Banding",
                    Color = "RED"
                }
            }
        };

    }

    public static Part CreateIslandPanelPart(string cornerShelfSizes) {
        return new Part {
            WallNum = 1,
            SectionNum = 1,
            PartType = "Material",
            PartName = "Vertical Panel - Island",
            ExportName = "CPS FM Vert",
            Color = "White",
            Height = 83.898,
            Width = 0.75,
            Depth = 14,
            Quantity = 1,
            VertHand = "L",
            VertDrillL = 12,
            VertDrillR = 14,
            BBHeight = 0,
            BBDepth = 0,
            ShoeHeight = 0,
            ShoeDepth = 0,
            DrillLeft1 = "",
            DrillLeft2 = "",
            DrillRight1 = "0.0000|83.8980",
            DrillRight2 = "",
            RailNotch = "N",
            RailNotchElevation = 0,
            CornerShelfSizes = cornerShelfSizes,
            PartCost = "$24.41",
            UnitL = "",
            UnitR = "",
            PartNum = 1,
            InfoRecords = []
        };
    }

}
