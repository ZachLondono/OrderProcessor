using Microsoft.Office.Interop.Excel;

namespace OrderLoading.LoadDoweledDBSpreadsheetOrderData.Models;

public record FrontDrillingSpecs {

    public bool PeanutSlotFronts { get; set; }
    public double PeanutSlotFrontOffSides { get; set; }
    public bool DrillFronts { get; set; }
    public double FrontDrillingOffSides { get; set; }

    public static FrontDrillingSpecs ReadFromSheet(Worksheet worksheet) {

        return new() {
            PeanutSlotFronts = worksheet.GetRangeStringValue("SlotFront") == "Yes",
            PeanutSlotFrontOffSides = worksheet.GetRangeValue<double>("FrontSlotsOffSides", 0),
            DrillFronts = worksheet.GetRangeStringValue("DrillFront") == "Yes",
            FrontDrillingOffSides = worksheet.GetRangeValue<double>("FrontDrillingOffSides", 0)
        };

    }

}


