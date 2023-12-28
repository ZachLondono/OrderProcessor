using CsvHelper.Configuration.Attributes;

namespace ApplicationCore.Features.AllmoxyOrderExport.Products.Shelves;

public class AdjustableSafetyShelf : IAllmoxyProduct {

    [Index(0)] public required string Folder { get; set; }
    [Index(1)] public string Name => "Adjustable Safety Shelves";
    [Index(2)] public required string ClosetMaterial { get; set; }
    [Index(3)] public required string BandingColor { get; set; }
    [Index(4)] public required string PanelFinish { get; set; }
    [Index(5)] public required bool EdgeBandFrontAndBack { get; set; }
    [Index(6)] public required int Qty { get; set; }
    [Index(7)] public required double Width { get; set; }
    [Index(8)] public required double Depth { get; set; }
    [Index(9)] public required string Recess { get; set; }
    [Index(10)] public required bool DrawerLock { get; set; }
    [Index(11)] public required string PartComment { get; set; }
    [Index(12)] public required bool AddLEDChannel { get; set; }
    [Index(13)] public required double LEDOffFront { get; set; }
    [Index(14)] public required double LEDWidth { get; set; }
    [Index(15)] public required double LEDDepth { get; set; }

}
