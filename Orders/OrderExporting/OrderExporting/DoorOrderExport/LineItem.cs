using Domain.Orders.Components;
using Domain.Orders.ValueObjects;
using Domain.ValueObjects;
using OneOf.Types;
using DoorTypeEnum = Domain.Orders.Enums.DoorType;

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

    public static LineItem FromDoor(MDFDoor door, Optional<double> defaultStilesRails) {

        var lineItem = new LineItem() {
            PartNumber = door.ProductNumber,
            Description = "",
            Qty = door.Qty,
            Width = door.Width.AsMillimeters(),
            Height = door.Height.AsMillimeters(),
            SpecialFeatures = door.Note,
            DoorType = Optional<string>.None,
            StileLeft = GetFrameDim(door.FrameSize.LeftStile, defaultStilesRails),
            StileRight = GetFrameDim(door.FrameSize.RightStile, defaultStilesRails),
            RailTop = GetFrameDim(door.FrameSize.TopRail, defaultStilesRails),
            RailBottom = GetFrameDim(door.FrameSize.BottomRail, defaultStilesRails),
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
            PanelOrientation = door.Orientation switch {
                DoorOrientation.Horizontal => "Horizontal",
                DoorOrientation.Vertical => "Vertical",
                _ => throw new ArgumentOutOfRangeException(nameof(door.Orientation), "Invalid door orientation."),
            }
        };

        switch (door.Type) {
            case DoorTypeEnum.DrawerFront:
                lineItem.Description = "Drawer Front";

                if (door.AdditionalOpenings.Length != 0) {
                    throw new InvalidOperationException($"MDF Door spreadsheet does not support drawer fronts with more than 1 opening.");
                }

                if (door.Panel.IsOpen) {
                    throw new InvalidOperationException("Drawer fronts cannot have open panels.");
                }

                break;

            case DoorTypeEnum.AppliedPanel:
                lineItem.Description = "Applied Panel";
                lineItem.DoorType = "Applied Panel";

                lineItem.SetAdditionalOpeningProperties(door, defaultStilesRails);

                if (door.Panel.IsOpen || door.AdditionalOpenings.Any(a => a.Panel.IsOpen)) {
                    throw new InvalidOperationException("Applied panels cannot have open panels.");
                }

                break;

            case DoorTypeEnum.HamperDoor:
                throw new NotImplementedException("Exporting hamper doors is not implemented.");

            case DoorTypeEnum.Door:

                lineItem.Description = "Door";

                lineItem.SetAdditionalOpeningProperties(door, defaultStilesRails);
                lineItem.SetMultiOpeningDoorTypeAndDescription(door);

                break;

        }

        Optional<double> GetFrameDim(Dimension dim, Optional<double> defaultVal) =>
            defaultVal.Match(
                (double some) => {

                    var mm = dim.AsMillimeters();
                    if (mm == some) {
                        return Optional<double>.None;
                    }

                    return mm;

                },
                (None _) => dim.AsMillimeters());

        return lineItem;

    }

    private void SetMultiOpeningDoorTypeAndDescription(MDFDoor door) {

        switch (door.AdditionalOpenings.Length) {

            case 0:

                door.Panel.Switch(
                    (SolidPanel _) => { },
                    (OpenPanel o) => {

                        if (o.RouteForGasket) {
                            Description = "Frame, w/ Gasket Route";
                        } else if (o.RabbetBack) {
                            Description = "Frame Only, w/ Rabbet";
                        } else {
                            Description = "Frame Only, No Rabbet";
                        }

                        if (o.RabbetBack) {
                            DoorType = "Door FO";
                        } else {
                            DoorType = "Door FO, NR";
                        }

                    });

                break;

            case 1:

                Description = "Double Door";
                string temp = "Double Panel, ";

                if (door.AdditionalOpenings[0].Panel.IsOpen) {
                    temp += "O";
                } else {
                    temp += "S";
                }

                if (door.Panel.IsOpen) {
                    temp += "O";
                } else {
                    temp += "S";
                }

                DoorType = temp;

                break;

            case 2:

                if (door.Panel.IsOpen && door.AdditionalOpenings.All(o => o.Panel.IsOpen)) {
                    DoorType = "Triple Frame";
                } else if (door.Panel.IsSolid && door.AdditionalOpenings.All(o => o.Panel.IsSolid)) {
                    DoorType = "Triple Panel";
                } else {
                    throw new InvalidOperationException("Triple panel doors must be either all open or all solid panels");
                }

                break;

            case 3:
                DoorType = "Quadruple Panel";
                if (door.Panel.IsOpen || door.AdditionalOpenings.Any(o => o.Panel.IsOpen)) {
                    throw new InvalidOperationException("Quadruple panel doors can not have any open panels");
                }

                break;

            default:
                throw new InvalidOperationException($"MDF Door spreadsheet does not support doors with {door.AdditionalOpenings.Length + 1} openings.");

        }

    }

    private void SetAdditionalOpeningProperties(MDFDoor door, Optional<double> defaultVal) {

        if (door.AdditionalOpenings.Length >= 1) {
            Panel1 = door.AdditionalOpenings[0].OpeningHeight.AsMillimeters();
            RailStile3 = GetFrameDimOpt(door.AdditionalOpenings[0].RailWidth.AsMillimeters(), defaultVal);
        }

        if (door.AdditionalOpenings.Length >= 2) {
            Panel2 = door.AdditionalOpenings[1].OpeningHeight.AsMillimeters();
            RailStile4 = GetFrameDimOpt(door.AdditionalOpenings[1].RailWidth.AsMillimeters(), defaultVal);
        }

        if (door.AdditionalOpenings.Length >= 3) {
            Panel3 = door.AdditionalOpenings[2].OpeningHeight.AsMillimeters();
            RailStile5 = GetFrameDimOpt(door.AdditionalOpenings[2].RailWidth.AsMillimeters(), defaultVal);
        }

        /// <summary>
        /// If there is no value set for the optional dimension, return None. If there is a value set for the optional
        /// and it matches the default value, return None. Otherwise, return the value.
        /// </summary>
        Optional<double> GetFrameDimOpt(Optional<double> dim, Optional<double> defaultVal) =>
            dim.Match(
                (double some) => defaultVal.Match(
                    (double someDefault) => {

                        if (some == someDefault) {
                            return Optional<double>.None;
                        }

                        return some;

                    },
                    (None _) => dim),
                (None _) => Optional<double>.None);

    }

}