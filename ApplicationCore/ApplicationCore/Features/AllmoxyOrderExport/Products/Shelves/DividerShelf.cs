using CsvHelper.Configuration.Attributes;

namespace ApplicationCore.Features.AllmoxyOrderExport.Products.Shelves;

public class DividerShelf : IAllmoxyProduct {

    [Index(0)] public required string Folder { get; set; }
    [Index(1)] public string Name => "Divider Shelves";
    [Index(2)] public required string ClosetMaterial { get; set; }
    [Index(3)] public required string BandingColor { get; set; }
    [Index(4)] public required string PanelFinish { get; set; }
    [Index(5)] public required string EdgeDrilling { get; set; }
    [Index(6)] public required string TopOrBottom { get; set; }
    [Index(7)] public required int Qty { get; set; }
    [Index(8)] public required double Width { get; set; }
    [Index(9)] public required double Depth { get; set; }
    [Index(10)] public required string Divisions { get; set; }
    [Index(11)] public required string PartComment { get; set; }
    [Index(12)] public required double Opening1 { get; set; }
    [Index(13)] public required double Opening2 { get; set; }
    [Index(14)] public required double Opening3 { get; set; }
    [Index(15)] public required double Opening4 { get; set; }

}
