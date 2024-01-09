using ApplicationCore.Features.ClosetProCSVCutList;
using ApplicationCore.Features.ClosetProCSVCutList.CSVModels;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Shared.Domain;
using FluentAssertions;

namespace ApplicationCore.Tests.Unit.Orders.ClosetProOrderLoading;

public class MelamineSlabDoorMapping {

    [Fact]
    public void Door() {

        // Arrange
        Dimension hardwareSpread = Dimension.FromMillimeters(128);
        Part part = new() {
            WallNum = 1,
            SectionNum = 2,
            PartType = "Door",
            PartName = "Cab Door Insert",
            ExportName = "Slab",
            Color = "White",
            Height = 38.976,
            Width = 12.25,
            Depth = 0,
            Quantity = 2,
            VertHand = "D",
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
            PartCost = "$21.63",
            UnitL = "",
            UnitR = "",
            PartNum = 18,
            InfoRecords = [] 
        };

        // Act
        var front = ClosetProPartMapper.CreateSlabFront(part, hardwareSpread, RoomNamingStrategy.ByWallAndSection);

        // Assert
        front.Height.Should().Be(Dimension.FromInches(part.Height));
        front.Width.Should().Be(Dimension.FromInches(part.Width));
        front.Type.Should().Be(DoorType.Door);
        front.HardwareSpread.Should().Be(hardwareSpread);

    }

    [Fact]
    public void DrawerFront() {

        // Arrange
        Dimension hardwareSpread = Dimension.FromMillimeters(128);
        Part part = new() {
            WallNum = 1,
            SectionNum = 2,
            PartType = "Drawer",
            PartName = "Drawer X Small Insert",
            ExportName = "Slab",
            Color = "White",
            Height = 5.04,
            Width = 24.625,
            Depth = 0,
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
            PartCost = "$8.36",
            UnitL = "",
            UnitR = "",
            PartNum = 12,
            InfoRecords = [] 
        };

        // Act
        var front = ClosetProPartMapper.CreateSlabFront(part, hardwareSpread, RoomNamingStrategy.ByWallAndSection);

        // Assert
        front.Height.Should().Be(Dimension.FromInches(part.Height));
        front.Width.Should().Be(Dimension.FromInches(part.Width));
        front.Type.Should().Be(DoorType.DrawerFront);
        front.HardwareSpread.Should().Be(hardwareSpread);

    }

}
