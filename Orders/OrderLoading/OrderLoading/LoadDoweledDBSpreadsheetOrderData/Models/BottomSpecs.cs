using Microsoft.Office.Interop.Excel;

namespace OrderLoading.LoadDoweledDBSpreadsheetOrderData.Models;

public record BottomSpecs {

    public double BottomDadoOversize { get; set; }
    public double BottomDadoDepth { get; set; }
    public double BottomHeight { get; set; }
    public double BottomDadoOvercut { get; set; }
    public double BottomDadoToolDiameter { get; set; }
    public double BottomSizeAdjustment { get; set; }
    public bool PreDrillBottoms { get; set; }
    public double MinPreDrillingSpace { get; set; }

    public static BottomSpecs ReadFromSheet(Worksheet worksheet) {

        return new() {
            BottomDadoOversize = worksheet.GetRangeValue("BottomDadoOversize", 0.0),
            BottomDadoDepth = worksheet.GetRangeValue("BottomDadoDepth", 0.0),
            BottomHeight = worksheet.GetRangeValue("BottomHeight", 0.0),
            BottomDadoOvercut = worksheet.GetRangeValue("BottomDadoOvercut", 0.0),
            BottomDadoToolDiameter = worksheet.GetRangeValue("BottomDadoToolDiameter", 0.0),
            BottomSizeAdjustment = worksheet.GetRangeValue("BottomSizeAdjustment", 0.0),
            PreDrillBottoms = worksheet.GetRangeStringValue("PreDrillBottom") == "Yes",
            MinPreDrillingSpace = worksheet.GetRangeValue("MinPreDrillSpacing", 0.0),
        };

    }

}


