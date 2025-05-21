using Domain.Orders.Components;
using Domain.Orders.ValueObjects;
using Domain.ValueObjects;
using Microsoft.Office.Interop.Excel;
using OneOf.Types;
using Range = Microsoft.Office.Interop.Excel.Range;

namespace OrderExporting.DoorOrderExport;

public record GeneralSpecs {

    public required string Material { get; set; }
    public required string Style { get; set; }
    public required string EdgeProfile { get; set; }
    public required string PanelDetail { get; set; }
    public required Optional<double> StilesRails { get; set; }
    public required Optional<double> AStyleRails { get; set; }
    public required Optional<double> AStyleDwrFrontMax { get; set; }

    public required Optional<string> DoorType { get; set; }
    public required Optional<double> PanelDrop { get; set; }

    public required string Finish { get; set; }
    public required Optional<string> Color { get; set; }
    
    public required Optional<string> HingePattern { get; set; }
    public required Optional<double> Tab { get; set;  }
    public required Optional<double> HingeFromTopBottom { get; set;  }

    public required Optional<string> Hardware { get; set; }
    public required Optional<double> HardwareFromTopBottom { get; set;  }

    public void WriteToWorksheet(Worksheet worksheet) {

        WriteToRange(worksheet.Range["Material"], Material);
        WriteToRange(worksheet.Range["FramingBead"], Style);
        WriteToRange(worksheet.Range["EdgeDetail"], EdgeProfile);
        WriteToRange(worksheet.Range["PanelDetail"], PanelDetail);

        WriteToRange(worksheet.Range["StilesRails"], StilesRails);
        WriteToRange(worksheet.Range["ARail"], AStyleRails);
        WriteToRange(worksheet.Range["AMax"], AStyleDwrFrontMax);

        WriteToRange(worksheet.Range["OpeningType"], DoorType);
        WriteToRange(worksheet.Range["PanelDrop"], PanelDrop);

        WriteToRange(worksheet.Range["FinishOption"], Finish);
        WriteToRange(worksheet.Range["FinishColor"], Color);

        WriteToRange(worksheet.Range["HingeStyle"], HingePattern);
        WriteToRange(worksheet.Range["HingeTab"], Tab);
        WriteToRange(worksheet.Range["HingeTopBottom"], HingeFromTopBottom);

        WriteToRange(worksheet.Range["HardwareOption"], Hardware);
        WriteToRange(worksheet.Range["HardwarePosition"], HardwareFromTopBottom);

    }

    private static void WriteToRange<T>(Range? rng, Optional<T> data) =>
        data.Switch(
            d => WriteToRange(rng, d),
            none => { });

    private static void WriteToRange(Range? rng, object? data) {
        if (rng is null || data is null) return;
        rng.Value2 = data;
    }

    public static IGrouping<GeneralSpecs, MDFDoor>[] SeparatedDoorsBySpecs(IEnumerable<MDFDoor> doors) => doors.GroupBy(d => {

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

}
