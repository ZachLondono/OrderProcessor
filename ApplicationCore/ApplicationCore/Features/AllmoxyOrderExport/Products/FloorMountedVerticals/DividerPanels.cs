using CsvHelper.Configuration.Attributes;

namespace ApplicationCore.Features.AllmoxyOrderExport.Products.FloorMountedVerticals;

public class DividerPanels : IAllmoxyProduct {

    [Index(0)] public required string Folder { get; set; }
    [Index(1)] public string Name => "Divider Panels";
    [Index(2)] public required string ClosetMaterial { get; set; }
    [Index(3)] public required string BandingColor { get; set; }
    [Index(4)] public required bool EdgeBandTop { get; set; }
    [Index(5)] public required bool ExtendForBackPanel { get; set; }
    [Index(6)] public required string Cams { get; set; }
    [Index(7)] public required string PanelFinish { get; set; }
    [Index(8)] public required int Qty { get; set; }
    [Index(9)] public required string Height { get; set; }
    [Index(10)] public required double Depth { get; set; }
    [Index(11)] public required string FinishedSide { get; set; }
    [Index(12)] public required string PartComment { get; set; }

}
