using CsvHelper.Configuration.Attributes;

namespace ApplicationCore.Features.AllmoxyOrderExport.Products.Fronts;

public class MDFDoorFront : IAllmoxyProduct {

    [Index(0)] public required string Folder { get; set; }
    [Index(1)] public string Name => "Door - Custom Height";
    [Index(2)] public required string FramingBead { get; set; }
    [Index(3)] public required string PanelDetail { get; set; }
    [Index(4)] public required string EdgeProfile { get; set; }
    [Index(5)] public required string Finish { get; set; }
    [Index(6)] public required string Material { get; set; }
    [Index(7)] public required double Rails { get; set; }
    [Index(8)] public required double Stiles { get; set; }
    [Index(9)] public required double ReducedRails { get; set; }
    [Index(10)] public required double CustomPanelDrop { get; set; }
    [Index(11)] public required bool HingeDrilling { get; set; }
    [Index(12)] public required string OrderComments { get; set; }
    [Index(13)] public required int Qty { get; set; }
    [Index(14)] public required double Width { get; set; }
    [Index(15)] public required double Height { get; set; }
    [Index(16)] public required bool GlassFrame { get; set; }
    [Index(17)] public required bool ReduceRails { get; set; }
    [Index(18)] public required string Comments { get; set; }

}
