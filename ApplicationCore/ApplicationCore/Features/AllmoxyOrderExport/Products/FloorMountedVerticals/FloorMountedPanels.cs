using CsvHelper.Configuration.Attributes;

namespace ApplicationCore.Features.AllmoxyOrderExport.Products.FloorMountedVerticals;

public class FloorMountedPanels : IAllmoxyProduct {

    [Index(0)] public required string Folder { get; set; }
    [Index(1)] public string Name => "Floor Mounted Panels";
    [Index(2)] public required string ClosetMaterial { get; set; }
    [Index(3)] public required string BandingColor { get; set; }
    [Index(4)] public required double BaseNotchHeight { get; set; }
    [Index(5)] public required double BaseNotchWidth { get; set; }
    [Index(6)] public required bool EdgeBandTop { get; set; }
    [Index(7)] public required bool ExtendForBackPanel { get; set; }
    [Index(8)] public required string PanelFinish { get; set; }
    [Index(9)] public required int Qty { get; set; }
    [Index(10)] public required string PanelHeight { get; set; }
    [Index(11)] public required double Depth { get; set; }
    [Index(12)] public required string FinishedSide { get; set; }
    [Index(13)] public required string PartComment { get; set; }
    [Index(14)] public required bool AddLEDChannel { get; set; }
    [Index(15)] public required double LEDOffFront { get; set; }
    [Index(16)] public required double LEDWidth { get; set; }
    [Index(17)] public required double LEDDepth { get; set; }

}