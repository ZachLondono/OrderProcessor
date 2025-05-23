using Domain.Orders.Components;
using Domain.Orders.ValueObjects;
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

    public static LineItem FromDoor(MDFDoor door) {

        string description = "";
        Optional<string> doorType = Optional<string>.None;
        Optional<double> panel1 = Optional<double>.None;
        Optional<double> panel2 = Optional<double>.None;
        Optional<double> panel3 = Optional<double>.None;
        Optional<double> rail3 = Optional<double>.None;
        Optional<double> rail4 = Optional<double>.None;
        Optional<double> rail5 = Optional<double>.None;

        if (door.Type == Domain.Orders.Enums.DoorType.DrawerFront) {

            description = "Drawer Front";

            if (door.AdditionalOpenings.Length != 0) {
                throw new InvalidOperationException($"MDF Door spreadsheet does not support drawer fronts with more than 1 opening.");
            }

        } else {

            description = "Door";

            if (door.AdditionalOpenings.Length >= 1) {
                panel1 = door.AdditionalOpenings[0].OpeningHeight.AsMillimeters();
                rail3 = door.AdditionalOpenings[0].RailWidth.AsMillimeters();
            }

            if (door.AdditionalOpenings.Length >= 2) {
                panel2 = door.AdditionalOpenings[1].OpeningHeight.AsMillimeters();
                rail4 = door.AdditionalOpenings[1].RailWidth.AsMillimeters();
            }

            if (door.AdditionalOpenings.Length >= 3) {
                panel3 = door.AdditionalOpenings[2].OpeningHeight.AsMillimeters();
                rail5 = door.AdditionalOpenings[2].RailWidth.AsMillimeters();
            }

            switch (door.AdditionalOpenings.Length) {

                case 0:

                    door.AdditionalOpenings[0].Panel.Switch(
                        (SolidPanel _) => { },
                        (OpenPanel o) => {

                            if (o.RouteForGasket) {
                                description = "Frame, w/ Gasket Route"; 
                            } else if (o.RabbetBack) {
                                description = "Frame Only, w/ Rabbet";
                            } else {
                                description = "Frame Only, No Rabbet";
                            }

                            if (o.RabbetBack) {
                                doorType = "Door FO";
                            } else {
                                doorType = "Door FO, NR";
                            }

                        });

                    break;

                case 1:

                    description = "Double Door";
                    doorType = "Double Panel, ";

                    if (door.AdditionalOpenings[0].Panel.IsOpen) {
                        doorType += "O";
                    } else {
                        doorType += "S";
                    }

                    if (door.Panel.IsOpen) {
                        doorType += "O";
                    } else {
                        doorType += "S";
                    }

                    break;

                case 2:

                    if (door.Panel.IsOpen && door.AdditionalOpenings.All(o => o.Panel.IsOpen)) {
                        doorType = "Triple Frame";
                    } else if (door.Panel.IsSolid && door.AdditionalOpenings.All(o => o.Panel.IsSolid)) {
                        doorType = "Triple Panel";
                    } else {
                        throw new InvalidOperationException("Triple panel doors must be either all open or all solid panels");
                    }

                    break;

                case 3:
                    doorType = "Quadruple Panel";
                    if (door.Panel.IsOpen || door.AdditionalOpenings.Any(o => o.Panel.IsOpen)) {
                        throw new InvalidOperationException("Quadruple panel doors can not have any open panels");
                    }

                    break;

                default:
                    throw new InvalidOperationException($"MDF Door spreadsheet does not support doors with {door.AdditionalOpenings.Length + 1} openings.");

            }

        }

        return new LineItem() {
            PartNumber = door.ProductNumber,
            Description = description,
            Qty = door.Qty,
            Width = door.Width.AsMillimeters(),
            Height = door.Height.AsMillimeters(),
            SpecialFeatures = door.Note,
            DoorType = doorType,
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
            Panel1 = panel1,
            RailStile3 = rail3,
            Panel2 = panel2,
            RailStile4 = rail4,
            Panel3 = panel3,
            RailStile5 = rail5,
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

}