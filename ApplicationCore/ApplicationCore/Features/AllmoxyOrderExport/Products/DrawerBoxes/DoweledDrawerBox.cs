using CsvHelper.Configuration.Attributes;

namespace ApplicationCore.Features.AllmoxyOrderExport.Products.DrawerBoxes;

public class DoweledDrawerBox : IAllmoxyProduct {

    [Index(0)] public required string Folder { get; set; }
    [Index(1)] public string Name => "Doweled Drawer Box";
    [Index(2)] public required string BoxConstruction { get; set; }
    [Index(3)] public required string UndermountNotch { get; set; }
    [Index(4)] public required int Qty { get; set; }
    [Index(5)] public required double Height { get; set; }
    [Index(6)] public required double Width { get; set; }
    [Index(7)] public required double Depth { get; set; }

}
