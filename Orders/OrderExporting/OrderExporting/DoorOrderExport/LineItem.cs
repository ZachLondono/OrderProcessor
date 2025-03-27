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

}