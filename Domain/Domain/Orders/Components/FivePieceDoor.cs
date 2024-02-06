using Domain.Orders.ValueObjects;
using Domain.ValueObjects;

namespace Domain.Orders.Components;

public class FivePieceDoor : FivePieceDoorConfig {

    public static Dimension DadoDepth { get; set; } = Dimension.FromMillimeters(8);
    public static Dimension TotalCenterPanelUndersize { get; set; } = Dimension.FromMillimeters(0.5);

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
        var frame = GetFrameParts(qty);
        var center = GetCenterPanelPart(qty);
        List<FivePieceDoorPart> parts = [];
        parts.AddRange(frame);
        parts.Add(center);
        return parts;
    }

    public IEnumerable<FivePieceDoorPart> GetFrameParts(int qty) {
        (var top, var bottom) = GetRailParts(qty);
        (var left, var right) = GetStileParts(qty);
        List<FivePieceDoorPart> parts = new() {
            top,
            bottom,
            left,
            right,
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
        Dimension length = Height;

        var left = new FivePieceDoorPart("Left Stile", qty, leftWidth, length, Material);
        var right = new FivePieceDoorPart("Right Stile", qty, rightWidth, length, Material);

        return (left, right);

    }

    public FivePieceDoorPart GetCenterPanelPart(int qty) {

        Dimension width = Width - FrameSize.LeftStile - FrameSize.RightStile + 2 * DadoDepth - TotalCenterPanelUndersize;
        Dimension length = Height - FrameSize.TopRail - FrameSize.BottomRail + 2 * DadoDepth - TotalCenterPanelUndersize;

        return new("Center Panel", qty, width, length, Material);

    }

}
