using Microsoft.Office.Interop.Excel;

namespace ApplicationCore.Features.Orders.Loader.Providers.DoorOrderModels;

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
    public required string Material { get; set; }
    public required double Thickness { get; set; }

    public static LineItem ReadFromWorksheet(Worksheet sheet, int offset) {

        return new() {
            PartNumber = (int) sheet.Range["PartNumStart"].Offset[offset].Value2,
            Description = sheet.Range["DescriptionStart"].Offset[offset].Value2,
            Line = (int) sheet.Range["LineNumStart"].Offset[offset].Value2,
            Qty = (int) sheet.Range["QtyStart"].Offset[offset].Value2,
            Width = sheet.Range["WidthStart"].Offset[offset].Value2,
            Height = sheet.Range["HeightStart"].Offset[offset].Value2,
            Note = sheet.Range["NoteStart"].Offset[offset].Value2?.ToString() ?? "",
            UnitPrice = (decimal) sheet.Range["UnitPriceStart"].Offset[offset].Value2,
            LeftStile = sheet.Range["LeftStileStart"].Offset[offset].Value2,
            RightStile = sheet.Range["RightStileStart"].Offset[offset].Value2,
            TopRail = sheet.Range["TopRailStart"].Offset[offset].Value2,
            BottomRail = sheet.Range["BottomRailStart"].Offset[offset].Value2,
            Material  = sheet.Range["MaterialStart"].Offset[offset].Value2?.ToString() ?? "",
            Thickness = sheet.Range["ThicknessStart"].Offset[offset].Value2
        };

    }

}