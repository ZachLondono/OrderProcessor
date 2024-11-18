using CsvHelper.Configuration.Attributes;

namespace ApplicationCore.Features.AllmoxyOrderExport.Products.Miscellaneous;

public class ToeKick : IAllmoxyProduct {

    [Index(0)] public required string Folder { get; set; }
    [Index(1)] public string Name => "Toe Kick";
    [Index(2)] public required string ClosetMaterial { get; set; }
    [Index(3)] public required string BandingColor { get; set; }
    [Index(4)] public required string Cams { get; set; }
    [Index(5)] public required string PanelFinish { get; set; }
    [Index(6)] public required bool InstallCams { get; set; }
    [Index(7)] public required int Qty { get; set; }
    [Index(8)] public required double Width { get; set; }
    [Index(9)] public required double Length { get; set; }
    [Index(10)] public required string PartComment { get; set; }

}
