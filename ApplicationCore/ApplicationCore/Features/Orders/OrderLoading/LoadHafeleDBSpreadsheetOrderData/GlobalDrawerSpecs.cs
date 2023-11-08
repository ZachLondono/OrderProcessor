using ApplicationCore.Features.Orders.Shared.Domain.Excel;
using Microsoft.Office.Interop.Excel;

namespace ApplicationCore.Features.Orders.OrderLoading.LoadHafeleDBSpreadsheetOrderData;

public class GlobalDrawerSpecs {

    public required string Material { get; set; }
    public required string Bottoms { get; set; }
    public required string Assembly { get; set; }
    public required string NotchForSlides { get; set; }
    public required string LogosOnAll { get; set; }
    public required string Clips { get; set; }
    public required string PostFinish { get; set; }
    public required string MountingHoles { get; set; }

    public static GlobalDrawerSpecs ReadFromSheet(Worksheet sheet) {

        return new() {
            Material = sheet.GetRangeValueOrDefault("Material", "UNKNOWN"),
            Bottoms = sheet.GetRangeValueOrDefault("BotThickness", "UNKNOWN"),
            Assembly = sheet.GetRangeValueOrDefault("Assembled", "UNKNOWN"),
            NotchForSlides = sheet.GetRangeValueOrDefault("Notch", "UNKNOWN"),
            LogosOnAll = sheet.GetRangeValueOrDefault("LogoOption", "UNKNOWN"),
            Clips = sheet.GetRangeOffsetValueOrDefault("Clips", "UNKNOWN"),
            PostFinish = sheet.GetRangeOffsetValueOrDefault("PostFinish", "UNKNOWN"),
            MountingHoles = sheet.GetRangeOffsetValueOrDefault("MountingHoles", "UNKNOWN"),
        };

    }

}
