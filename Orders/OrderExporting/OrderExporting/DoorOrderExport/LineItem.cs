using Domain.Orders.Components;
using Domain.ValueObjects;

namespace OrderExporting.DoorOrderExport;

public record LineItem {

    public required Optional<int> PartNumber { get; set; }
    public required string Description { get; set; }
    public required int Qty { get; set; }
    public required double Width { get; set; }
    public required double Height { get; set; }
    public required Optional<string> SpecialFeatures { get; set; }

    public required Optional<string> DoorType { get; set; }

    public required Optional<double> StileLeft { get; set; }
    public required Optional<double> StileRight { get; set; }
    public required Optional<double> RailTop { get; set; }
    public required Optional<double> RailBottom { get; set; }

    public required Optional<double> HingeTop { get; set; }
    public required Optional<double> HingeBottom { get; set; }
    public required Optional<double> Hinge3 { get; set; }
    public required Optional<double> Hinge4 { get; set; }
    public required Optional<double> Hinge5 { get; set; }
    public required Optional<double> Tab { get; set; }
    public required Optional<double> CupDiameter { get; set; }
    public required Optional<string> HingePattern { get; set; }
    public required Optional<double> CupDepth { get; set; }
    public required Optional<string> SwingDirection { get; set; }

    public required Optional<string> HardwareReference { get; set; }
    public required Optional<string> Hardware { get; set; }
    public required Optional<double> HardwareTBOffset { get; set; }
    public required Optional<double> HardwareSideOffset { get; set; }
    public required Optional<double> HardwareDepth { get; set; }
    public required Optional<string> DoubleHardware { get; set; }

    public required Optional<double> Panel1 { get; set; }
    public required Optional<double> RailStile3 { get; set; }
    public required Optional<double> Panel2 { get; set; }
    public required Optional<double> RailStile4 { get; set; }
    public required Optional<double> Panel3 { get; set; }
    public required Optional<double> RailStile5 { get; set; }

    public required Optional<double> RabbetDepth { get; set; }
    public required Optional<double> RabbetWidth { get; set; }
    public required Optional<string> SquareRabbet { get; set; }
    public required Optional<double> PanelClearance { get; set; }
    public required Optional<double> PanelRadius { get; set; }
    public required Optional<double> PanelThickness { get; set; }
    public required Optional<double> PanelStyle { get; set; }

    public required Optional<string> MullionOpening { get; set; }
    public required Optional<string> MullionShape { get; set; }
    public required Optional<double> MullionWidth { get; set; }
    public required Optional<int> HorizontalMullions { get; set; }
    public required Optional<int> VerticalMullions { get; set; }
    public required Optional<double> Row1 { get; set; }
    public required Optional<double> Col1 { get; set; }
    public required Optional<double> Row2 { get; set; }
    public required Optional<double> Col2 { get; set; }

    public required Optional<string> Ease { get; set; }
    public required Optional<string> MachinedEdges { get; set; }

    public required Optional<double> Thickness { get; set; }
    public required Optional<string> Material { get; set; }
    public required Optional<string> BackCut { get; set; }
    public required Optional<string> RailSeams { get; set; }
    public required Optional<string> Grain { get; set; }
    public required Optional<string> PanelOrientation { get; set; }

    public static LineItem FromDoor(MDFDoor door) => new LineItem() {
        PartNumber = door.ProductNumber,
        Description = door.Type switch {
            Domain.Orders.Enums.DoorType.Door or Domain.Orders.Enums.DoorType.HamperDoor => "Door",
            Domain.Orders.Enums.DoorType.DrawerFront => "Drawer Front",
            _ => throw new InvalidOperationException($"Unexpected door type {door.Type}")
        },
        Qty = door.Qty,
        Width = door.Width.AsMillimeters(),
        Height = door.Height.AsMillimeters(),
        SpecialFeatures = Optional<string>.None,
        DoorType = Optional<string>.None,
        StileLeft = door.FrameSize.LeftStile.AsMillimeters(),
        StileRight = door.FrameSize.RightStile.AsMillimeters(),
        RailTop = door.FrameSize.TopRail.AsMillimeters(),
        RailBottom = door.FrameSize.BottomRail.AsMillimeters(),
        HingeTop = Optional<double>.None,
        HingeBottom = Optional<double>.None,
        Hinge3 = Optional<double>.None,
        Hinge4 = Optional<double>.None,
        Hinge5 = Optional<double>.None,
        Tab = Optional<double>.None,
        CupDiameter = Optional<double>.None,
        HingePattern = Optional<string>.None,
        CupDepth = Optional<double>.None,
        SwingDirection = Optional<string>.None,
        HardwareReference = Optional<string>.None,
        Hardware = Optional<string>.None,
        HardwareTBOffset = Optional<double>.None,
        HardwareSideOffset = Optional<double>.None,
        HardwareDepth = Optional<double>.None,
        DoubleHardware = Optional<string>.None,
        Panel1 = Optional<double>.None,
        RailStile3 = Optional<double>.None,
        Panel2 = Optional<double>.None,
        RailStile4 = Optional<double>.None,
        Panel3 = Optional<double>.None,
        RailStile5 = Optional<double>.None,
        RabbetDepth = Optional<double>.None,
        RabbetWidth = Optional<double>.None,
        SquareRabbet = Optional<string>.None,
        PanelClearance = Optional<double>.None,
        PanelRadius = Optional<double>.None,
        PanelThickness = Optional<double>.None,
        PanelStyle = Optional<double>.None,
        MullionOpening = Optional<string>.None,
        MullionShape = Optional<string>.None,
        MullionWidth = Optional<double>.None,
        HorizontalMullions = Optional<int>.None,
        VerticalMullions = Optional<int>.None,
        Row1 = Optional<double>.None,
        Col1 = Optional<double>.None,
        Row2 = Optional<double>.None,
        Col2 = Optional<double>.None,
        Ease = Optional<string>.None,
        MachinedEdges = Optional<string>.None,
        Thickness = Optional<double>.None,
        Material = Optional<string>.None,
        BackCut = Optional<string>.None,
        RailSeams = Optional<string>.None,
        Grain = Optional<string>.None,
        PanelOrientation = Optional<string>.None,
    };

}