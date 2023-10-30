using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.Components;

public class FivePieceDoor : FivePieceDoorConfig {

    public Dimension Width { get; init; }
    public Dimension Height { get; init; }
    public DoorFrame FrameSize { get; init; }

    public FivePieceDoor(Dimension width, Dimension height, DoorFrame frameSize, Dimension frameThickness, Dimension panelThickness, string material)
            : base(frameThickness, panelThickness, material) {
        Width = width;
        Height = height;
        FrameSize = frameSize;
    }

    public IEnumerable<FivePieceDoorPart> GetParts(int qty) {
        (var top, var bottom) = GetRailParts(qty);
        (var left, var right) = GetStileParts(qty);
        var center = GetCenterPanelPart(qty);
        List<FivePieceDoorPart> parts = new() {
            top,
            bottom,
            left,
            right,
            center
        };
        return parts;
    }

    public (FivePieceDoorPart top, FivePieceDoorPart bottom) GetRailParts(int qty) {

        Dimension topWidth = FrameSize.TopRail;
        Dimension bottomWidth = FrameSize.BottomRail;
        Dimension length = Width - FrameSize.LeftStile - FrameSize.RightStile;

        var top = new FivePieceDoorPart("Top Rail", qty, topWidth, length, Material);
        var bottom = new FivePieceDoorPart("Bottom Rail", qty, bottomWidth, length, Material);

        return (top, bottom);

    }

    public (FivePieceDoorPart left, FivePieceDoorPart right) GetStileParts(int qty) {

        Dimension leftWidth = FrameSize.LeftStile;
        Dimension rightWidth = FrameSize.RightStile;
        Dimension length = Height - FrameSize.TopRail - FrameSize.BottomRail;

        var left = new FivePieceDoorPart("Left Stile", qty, leftWidth, length, Material);
        var right = new FivePieceDoorPart("Right Stile", qty, rightWidth, length, Material);

        return (left, right);

    }

    public FivePieceDoorPart GetCenterPanelPart(int qty) {

        Dimension width = Width - FrameSize.LeftStile - FrameSize.RightStile;
        Dimension length = Height - FrameSize.TopRail - FrameSize.BottomRail;

        return new("Center Panel", qty, width, length, Material);

    }

}
