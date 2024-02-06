using Domain.Orders.Excel;
using Microsoft.Office.Interop.Excel;

namespace ApplicationCore.Features.Orders.OrderLoading.LoadHafeleDBSpreadsheetOrderData;

public class GlobalDrawerSpecs {

    public required string BoxMaterial { get; set; }
    public required string BottomMaterial { get; set; }
    public required string Assembly { get; set; }
    public required string Notch { get; set; }
    public required string MountingHoles { get; set; }
    public required string Clips { get; set; }
    public required string Units { get; set; }

    public static GlobalDrawerSpecs ReadFromSheet(Worksheet sheet) {

        return new() {
            BoxMaterial = sheet.GetRangeValueOrDefault("PeanutBoxMaterial", "UNKNOWN"),
            BottomMaterial = sheet.GetRangeValueOrDefault("PeanutBottomMaterial", "UNKNOWN"),
            Assembly = sheet.GetRangeValueOrDefault("PeanutAssembled", "UNKNOWN"),
            Notch = sheet.GetRangeValueOrDefault("PeanutConfiguration", "UNKNOWN"),
            MountingHoles = sheet.GetRangeValueOrDefault("PeanutMountingHoles", "UNKNOWN"),
            Clips = sheet.GetRangeOffsetValueOrDefault("PeanutClips", "UNKNOWN"),
            Units = sheet.GetRangeOffsetValueOrDefault("PeanutUnits", "UNKNOWN")
        };

    }

}
