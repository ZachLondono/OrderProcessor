using Domain.Excel;
using Microsoft.Office.Interop.Excel;

namespace OrderLoading.LoadClosetOrderSpreadsheetOrderData.Models;

public record Zargen : IWorksheetReadable<Zargen> {

    public static int FirstRow => 2;

    public static int RowStep => 3;

    public int Qty { get; set; }
    public string Item { get; set; } = string.Empty;
    public double HoleSize { get; set; }
    public double SlideDepth { get; set; }
    public double PullCtrDim { get; set; }
    public decimal ExtPrice { get; set; }

    public static Zargen ReadFromWorksheet(Worksheet worksheet, int row) {
        int qty = (int)worksheet.GetRangeValueOrDefault($"A{row}", 0.0);
        var item = worksheet.GetRangeValueOrDefault($"B{row}", "");
        var holeSize = worksheet.GetRangeValueOrDefault($"C{row}", 0.0);
        var slideDepth = worksheet.GetRangeValueOrDefault($"D{row}", 0.0);
        var pullCtrDim = worksheet.GetRangeValueOrDefault($"E{row}", 0.0);
        var extPrice = (decimal)worksheet.GetRangeValueOrDefault($"N{row}", 0.0);
        return new() {
            Qty = qty,
            Item = item,
            HoleSize = holeSize,
            SlideDepth = slideDepth,
            PullCtrDim = pullCtrDim,
            ExtPrice = extPrice
        };
    }

}

