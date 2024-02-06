using Domain.Orders.Excel;
using Microsoft.Office.Interop.Excel;

namespace ApplicationCore.Features.Orders.OrderLoading.LoadHafeleDBSpreadsheetOrderData;

public class LineItem {

    public required int Line { get; set; }
    public required int Qty { get; set; }
    public required double Height { get; set; }
    public required double Width { get; set; }
    public required double Depth { get; set; }
    public required string PullOut { get; set; }
    public required string BottomMaterial { get; set; }
    public required string Clips { get; set; }
    public required string JobName { get; set; }
    public required decimal UnitPrice { get; set; }

    public static LineItem ReadFromSheet(Worksheet sheet, int rowOffset) {

        return new() {
            Line = (int)sheet.GetRangeOffsetValueOrDefault("DowelLineCol", 1.0, rowOffset),
            Qty = (int)sheet.GetRangeOffsetValueOrDefault("DowelQtyCol", 0.0, rowOffset),
            Height = sheet.GetRangeOffsetValueOrDefault("DowelHeightCol", 0.0, rowOffset),
            Width = sheet.GetRangeOffsetValueOrDefault("DowelWidthCol", 0.0, rowOffset),
            Depth = sheet.GetRangeOffsetValueOrDefault("DowelDepthCol", 0.0, rowOffset),
            PullOut = sheet.GetRangeOffsetValueOrDefault("DowelPullOutCol", "UNKNOWN", rowOffset),
            BottomMaterial = sheet.GetRangeOffsetValueOrDefault("DowelBottomCol", "UNKNOWN", rowOffset),
            Clips = sheet.GetRangeOffsetValueOrDefault("DowelClipsCol", "UNKNOWN", rowOffset),
            JobName = sheet.GetRangeOffsetValueOrDefault("DowelJobNameCol", "UNKNOWN", rowOffset),
            UnitPrice = (decimal)sheet.GetRangeOffsetValueOrDefault("DowelUnitPriceCol", 0.0, rowOffset)
        };

    }

}
