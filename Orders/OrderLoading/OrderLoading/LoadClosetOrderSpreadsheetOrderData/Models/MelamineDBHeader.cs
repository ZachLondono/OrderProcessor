using Domain.Excel;
using Microsoft.Office.Interop.Excel;

namespace OrderLoading.LoadClosetOrderSpreadsheetOrderData.Models;

public record MelamineDBHeader(string BoxMaterial, string BottomMaterial, string Slides) {

    public static MelamineDBHeader ReadFromWorksheet(Worksheet worksheet) {
        var boxMat = worksheet.GetRangeValueOrDefault($"G1", "");
        var bottomMat = worksheet.GetRangeValueOrDefault($"G2", "");
        var slides = worksheet.GetRangeValueOrDefault($"G3", "");
        return new(boxMat, bottomMat, slides);
    }

}

