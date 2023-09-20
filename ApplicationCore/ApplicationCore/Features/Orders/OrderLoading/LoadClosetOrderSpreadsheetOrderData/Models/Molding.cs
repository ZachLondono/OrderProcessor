using ApplicationCore.Features.Orders.Shared.Domain.Excel;
using Microsoft.Office.Interop.Excel;

namespace ApplicationCore.Features.Orders.OrderLoading.LoadClosetOrderSpreadsheetOrderData.Models;

internal record Molding(string Name, double LinearFt, string Color, decimal Price) {

    public static Molding ReadFromWorksheet(Worksheet worksheet, int row) {
        var name = worksheet.GetRangeValueOrDefault($"A{row}", "");
        var linFt = worksheet.GetRangeValueOrDefault($"B{row}", 0.0);
        var color = worksheet.GetRangeValueOrDefault($"C{row}", "");
        var price = worksheet.GetRangeValueOrDefault($"D{row}", 0m);
        return new(name, linFt, color, price);
    }

}

