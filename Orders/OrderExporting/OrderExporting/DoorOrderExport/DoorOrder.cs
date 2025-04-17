using Domain.Orders.Components;
using Domain.Orders.Entities;
using Domain.Orders.Enums;
using Domain.Orders.ValueObjects;
using Domain.ValueObjects;
using Microsoft.Office.Interop.Excel;
using OneOf.Types;

namespace OrderExporting.DoorOrderExport;

public record DoorOrder {

    public required Guid ProcessorOrderId { get; set; }
    public required Optional<string> VendorName { get; set; }

    public required DateTime OrderDate { get; set; }
    public required Optional<DateTime> DueDate { get; set; }
    public required string Company { get; set; } = string.Empty;
    public required string TrackingNumber { get; set; } = string.Empty;
    public required string JobName { get; set; } = string.Empty;

    public required string Units { get; set; }

    public required GeneralSpecs Specs { get; set; }
    public required IEnumerable<LineItem> LineItems { get; set; }

    public static string METRIC_UNITS = "Metric (mm)";

    public void WriteToWorksheet(Worksheet worksheet) {

        VendorName.Switch(
            name => {
                worksheet.Range["Vendor"].Value2 = name;
            },
            none => { });
        worksheet.Range["Processor_Order_Id"].Value2 = ProcessorOrderId.ToString();
        worksheet.Range["OrderDate"].Value2 = OrderDate.ToString();
        DueDate.Switch(
            date => {
                worksheet.Range["DueDate"].Value2 = date.ToString();
            },
            none => { });
        worksheet.Range["Company"].Value2 = Company;
        worksheet.Range["JobNumber"].Value2 = TrackingNumber;
        worksheet.Range["JobName"].Value2 = JobName; 
        worksheet.Range["units"].Value2 = Units;

        Specs.WriteToWorksheet(worksheet);

        var writer = new LineItemWriter(worksheet);
        foreach (var lineItem in LineItems) {
            writer.WriteLine(lineItem);
        }

    }

    public static IEnumerable<DoorOrder> FromOrder(Order order, IEnumerable<MDFDoor> doors, string customerName, string vendorName) {

        var groups = doors.GroupBy(d => {

            string finish = "None";
            Optional<string> color = Optional<string>.None;
            d.Finish.Switch(
                (Paint paint) => {
                    finish = "Std. Color";
                    color = paint.Color;
                },
                (Primer primer) => {
                    finish = "Prime Only";
                    color = primer.Color;
                },
                (None _) => {
                    finish = "None";
                    color = Optional<string>.None;
                });

            return new GeneralSpecs() {

                Material = d.Material,
                Style = d.FramingBead,
                EdgeProfile = d.EdgeDetail,
                PanelDetail = d.PanelDetail,
                Finish = finish,
                Color = color,
                PanelDrop = d.PanelDrop == Dimension.Zero ? Optional<double>.None : d.PanelDrop.AsMillimeters(),

                HingePattern = Optional<string>.None,
                Tab = Optional<double>.None,
                StilesRails = Optional<double>.None,
                AStyleRails = Optional<double>.None,
                AStyleDwrFrontMax = Optional<double>.None,
                DoorType = Optional<string>.None,
                HingeFromTopBottom = Optional<double>.None,
                Hardware = Optional<string>.None,
                HardwareFromTopBottom = Optional<double>.None

            };

        }).ToArray();

        for (int i = 0; i < groups.Length; i++) {

            // TODO: Optimization - find the most common frame width and set that to the default for the group. Then do not overwrite those values in the line items.

            var group = groups[i];

            yield return new() {
                OrderDate = order.OrderDate,
                DueDate = order.DueDate ?? Optional<DateTime>.None,
                Company = customerName,
                TrackingNumber = $"{order.Number}{(groups.Length == 0 ? string.Empty : $"-{i + 1}")}",
                JobName = order.Name,
                ProcessorOrderId = order.Id,
                Units = METRIC_UNITS,
                VendorName = vendorName,

                Specs = group.Key,

                LineItems = group.Select(d => new LineItem() {
                    PartNumber = d.ProductNumber,
                    Description = d.Type switch {
                        DoorType.Door or DoorType.HamperDoor => "Door",
                        DoorType.DrawerFront => "Drawer Front",
                        _ => throw new InvalidOperationException($"Unexpected door type {d.Type}")
                    },
                    Qty = d.Qty,
                    Width = d.Width.AsMillimeters(),
                    Height = d.Height.AsMillimeters(),
                    SpecialFeatures = Optional<string>.None,
                    DoorType = Optional<string>.None,
                    StileLeft = d.FrameSize.LeftStile.AsMillimeters(),
                    StileRight = d.FrameSize.RightStile.AsMillimeters(),
                    RailTop = d.FrameSize.TopRail.AsMillimeters(),
                    RailBottom = d.FrameSize.BottomRail.AsMillimeters(),
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
                })

            };

        }

    }

}
