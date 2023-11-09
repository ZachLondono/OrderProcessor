using ApplicationCore.Features.Orders.Shared.Domain.Excel;
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
    public required string Notch { get; set; }
    public required string Logo { get; set; }
    public required string Clips { get; set; }
    public required string Accessory { get; set; }
    public required string JobName { get; set; }
    public required decimal UnitPrice { get; set; }
    public required string LabelNote { get; set; }
    public required double UBoxA { get; set; }
    public required double UBoxB { get; set; }
    public required double UBoxC { get; set; }
    public required int DividerOpeningsLeftToRight { get; set; }
    public required int DividerOpeningsFrontToBack { get; set; }
    
    public static LineItem ReadFromSheet(Worksheet sheet, int rowOffset) {

        return new() {
            Line = sheet.GetRangeOffsetValueOrDefault("A16", 1, rowOffset),
            Qty = (int) sheet.GetRangeOffsetValueOrDefault("DovetailQtyCol", 0.0, rowOffset),
            Height = sheet.GetRangeOffsetValueOrDefault("DovetailHeightCol", 0.0, rowOffset),
            Width = sheet.GetRangeOffsetValueOrDefault("DovetailWidthCol", 0.0, rowOffset),
            Depth = sheet.GetRangeOffsetValueOrDefault("DovetailDepthCol", 0.0, rowOffset),
            PullOut = sheet.GetRangeOffsetValueOrDefault("I16", "UNKNOWN", rowOffset),
            BottomMaterial = sheet.GetRangeOffsetValueOrDefault("J16", "UNKNOWN", rowOffset),
            Notch = sheet.GetRangeOffsetValueOrDefault("K16", "UNKNOWN", rowOffset),
            Logo = sheet.GetRangeOffsetValueOrDefault("L16", "UNKNOWN", rowOffset),
            Clips = sheet.GetRangeOffsetValueOrDefault("M16", "UNKNOWN", rowOffset),
            Accessory = sheet.GetRangeOffsetValueOrDefault("DovetailAccessoryCol", "UNKNOWN", rowOffset),
            JobName = sheet.GetRangeOffsetValueOrDefault("O16", "UNKNOWN", rowOffset),
            UnitPrice = (decimal) sheet.GetRangeOffsetValueOrDefault("DovetailUnitPriceCol", 0.0, rowOffset),
            LabelNote = sheet.GetRangeOffsetValueOrDefault("S16", "", rowOffset),
            UBoxA = sheet.GetRangeOffsetValueOrDefault("U16", 0.0, rowOffset),
            UBoxB = sheet.GetRangeOffsetValueOrDefault("V16", 0.0, rowOffset),
            UBoxC = sheet.GetRangeOffsetValueOrDefault("W16", 0.0, rowOffset),
            DividerOpeningsLeftToRight = (int) sheet.GetRangeOffsetValueOrDefault("DovetailLeftRightOpeningCol", 0.0, rowOffset),
            DividerOpeningsFrontToBack = (int) sheet.GetRangeOffsetValueOrDefault("DovetailFrontBackOpeningCol", 0.0, rowOffset)
        };

    }

}
