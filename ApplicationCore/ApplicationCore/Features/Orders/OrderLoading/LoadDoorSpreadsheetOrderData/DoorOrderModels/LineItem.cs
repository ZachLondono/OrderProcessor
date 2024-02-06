using Domain.Excel;
using Microsoft.Office.Interop.Excel;

namespace ApplicationCore.Features.Orders.OrderLoading.LoadDoorSpreadsheetOrderData.DoorOrderModels;

internal class LineItem {

    public required int PartNumber { get; set; }
    public required string Description { get; set; }
    public required double Line { get; set; }
    public required int Qty { get; set; }
    public required double Width { get; set; }
    public required double Height { get; set; }
    public required string Note { get; set; }
    public required decimal UnitPrice { get; set; }
    public required double LeftStile { get; set; }
    public required double RightStile { get; set; }
    public required double TopRail { get; set; }
    public required double BottomRail { get; set; }
    public required double Opening1 { get; set; }
    public required double Rail3 { get; set; }
    public required double Opening2 { get; set; }
    public required double Rail4 { get; set; }
    public required string Material { get; set; }
    public required double Thickness { get; set; }
    public required string Orientation { get; set; }

    public static LineItem ReadFromWorksheet(Worksheet sheet, int offset) {
        // TODO: save references to ranges somewhere, don't try to find the range again every time
        return new() {
            PartNumber = sheet.GetRangeOffsetValueOrDefault("PartNumStart", 0, offset),
            Description = sheet.GetRangeOffsetValueOrDefault("DescriptionStart", string.Empty, offset),
            Line = sheet.GetRangeOffsetValueOrDefault("LineNumStart", 0, offset),
            Qty = sheet.GetRangeOffsetValueOrDefault("QtyStart", 0, offset),
            Width = sheet.GetRangeOffsetValueOrDefault("WidthStart", 0d, offset),
            Height = sheet.GetRangeOffsetValueOrDefault("HeightStart", 0d, offset),
            Note = sheet.GetRangeOffsetValueOrDefault("NoteStart", string.Empty, offset),
            UnitPrice = sheet.GetRangeOffsetValueOrDefault("UnitPriceStart", 0m, offset),
            LeftStile = sheet.GetRangeOffsetValueOrDefault("LeftStileStart", 0d, offset),
            RightStile = sheet.GetRangeOffsetValueOrDefault("RightStileStart", 0d, offset),
            TopRail = sheet.GetRangeOffsetValueOrDefault("TopRailStart", 0d, offset),
            BottomRail = sheet.GetRangeOffsetValueOrDefault("BottomRailStart", 0d, offset),
            Opening1 = sheet.GetRangeOffsetValueOrDefault("Opening1Start", 0d, offset),
            Rail3 = sheet.GetRangeOffsetValueOrDefault("Rail3Start", 0d, offset),
            Opening2 = sheet.GetRangeOffsetValueOrDefault("Opening1Start", 0d, offset),
            Rail4 = sheet.GetRangeOffsetValueOrDefault("Rail4Start", 0d, offset),
            Material = sheet.GetRangeOffsetValueOrDefault("MaterialStart", string.Empty, offset),
            Thickness = sheet.GetRangeOffsetValueOrDefault("ThicknessStart", 0d, offset),
            Orientation = sheet.GetRangeOffsetValueOrDefault("OrientationStart", "Vertical", offset)
        };

    }

}