using CsvHelper.Configuration.Attributes;

namespace ApplicationCore.Features.AllmoxyOrderExport.Products.DrawerBoxes;

public class DovetailDrawerBox : IAllmoxyProduct {

    [Index(0)] public required string Folder { get; set; }
    [Index(1)] public string Name => "Dovetail Drawer Box - Standard Height";
    [Index(2)] public required string SideMaterial { get; set; }
    [Index(3)] public required string BottomThickness { get; set; }
    [Index(4)] public required string UndermountNotching { get; set; }
    [Index(5)] public required string Clips { get; set; }
    [Index(6)] public required string IncludeSlides { get; set; }
    [Index(7)] public required string Comments { get; set; }
    [Index(8)] public required int Qty { get; set; }
    [Index(9)] public required string Height { get; set; }
    [Index(10)] public required double Width { get; set; }
    [Index(11)] public required double Depth { get; set; }
    [Index(12)] public required bool Scoop { get; set; }
    [Index(13)] public required bool Logo { get; set; }
    [Index(14)] public required string LabelNote { get; set; }
    [Index(15)] public required string Insert { get; set; }

}
