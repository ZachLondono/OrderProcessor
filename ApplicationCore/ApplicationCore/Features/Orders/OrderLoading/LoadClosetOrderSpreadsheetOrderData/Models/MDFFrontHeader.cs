using Domain.Orders.Excel;
using Microsoft.Office.Interop.Excel;

namespace ApplicationCore.Features.Orders.OrderLoading.LoadClosetOrderSpreadsheetOrderData.Models;

internal record MDFFrontHeader(string Style, double FrameSize, double AStyleFrameSize, string Finish, string PaintColor) {

    public static MDFFrontHeader ReadFromWorksheet(Worksheet worksheet) {
        var style = worksheet.GetRangeValueOrDefault($"E1", "");
        var frameSize = worksheet.GetRangeValueOrDefault($"E2", 0.0);
        var aStyleFrameSize = worksheet.GetRangeValueOrDefault($"E3", 0.0);
        var finish = worksheet.GetRangeValueOrDefault($"H1", "");
        var paintColor = worksheet.GetRangeValueOrDefault($"H2", "");
        return new(style, frameSize, aStyleFrameSize, finish, paintColor);
    }

}

