using CsvHelper.Configuration.Attributes;

namespace ApplicationCore.Features.AllmoxyOrderExport.Products.Miscellaneous;

public class Nailer : IAllmoxyProduct {

    [Index(0)] public required string Folder { get; set; }
    [Index(1)] public string Name => "Nailer";
    [Index(2)] public required string ClosetMaterial { get; set; }
    [Index(3)] public required string BandingColor { get; set; }
    [Index(4)] public required string PanelFinish { get; set; }
    [Index(5)] public required int Qty { get; set; }
    [Index(6)] public required string Banding { get; set; }
    [Index(7)] public required double Width { get; set; }
    [Index(8)] public required double Length { get; set; }
    [Index(9)] public required string PartComment { get; set; }

}
