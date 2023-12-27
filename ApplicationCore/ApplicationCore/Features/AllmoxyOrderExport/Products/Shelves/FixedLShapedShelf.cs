using CsvHelper.Configuration.Attributes;

namespace ApplicationCore.Features.AllmoxyOrderExport.Products.Shelves;

public class FixedLShapedShelf : IAllmoxyProduct {

    [Index(0)] public required string Folder { get; set; }
    [Index(1)] public string Name => "Fixed L-Shaped";
    [Index(2)] public required string ClosetMaterial { get; set; }
    [Index(3)] public required string BandingColor { get; set; }
    [Index(4)] public required string PanelFinish { get; set; }
    [Index(5)] public required string Notch { get; set; }
    [Index(6)] public required double Radius { get; set; }
    [Index(7)] public required int Qty { get; set; }
    [Index(8)] public required double A { get; set; }
    [Index(9)] public required double B { get; set; }
    [Index(10)] public required double C { get; set; }
    [Index(11)] public required double D { get; set; }
    [Index(12)] public required string NotchSide { get; set; }
    [Index(13)] public required string PartComment { get; set; }

}
