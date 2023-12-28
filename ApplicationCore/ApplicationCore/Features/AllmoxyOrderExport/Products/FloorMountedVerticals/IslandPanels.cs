using CsvHelper.Configuration.Attributes;

namespace ApplicationCore.Features.AllmoxyOrderExport.Products.FloorMountedVerticals;

public class IslandPanels : IAllmoxyProduct {

    [Index(0)] public required string Folder { get; set; }
    [Index(1)] public string Name => "Island Panels";
    [Index(2)] public required string ClosetMaterial { get; set; }
    [Index(3)] public required string BandingColor { get; set; }
    [Index(7)] public required string PanelFinish { get; set; }
    [Index(8)] public required int Qty { get; set; }
    [Index(9)] public required string PanelHeight { get; set; }
    [Index(10)] public required double Depth { get; set; }
    [Index(11)] public required string Side1Depth { get; set; }
    [Index(11)] public required string FinishedSide { get; set; }
    [Index(12)] public required string PartComment { get; set; }

}
