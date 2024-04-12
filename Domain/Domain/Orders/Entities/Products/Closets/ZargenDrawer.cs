using Domain.Orders.Entities.Hardware;
using Domain.Orders.Enums;
using Domain.Orders.ValueObjects;
using Domain.ProductPlanner;
using Domain.ValueObjects;

namespace Domain.Orders.Entities.Products.Closets;

public class ZargenDrawer : IProduct, IPPProductContainer {

    public Guid Id { get; }
    public int Qty { get; }
    public decimal UnitPrice { get; }
    public int ProductNumber { get; }
    public string Room { get; set; }
    public string SKU { get; }
    public Dimension OpeningWidth { get; set; }
    public Dimension Height { get; set; }
    public Dimension Depth { get; set; }
    public ClosetMaterial Material { get; }
    public ClosetPaint? Paint { get; }
    public string EdgeBandingColor { get; }
    public string Comment { get; }
    public IReadOnlyDictionary<string, string> Parameters { get; }
    public List<string> ProductionNotes { get; set; } = new();

    public static Dimension[] StandardHeights => new Dimension[] {
        Dimension.FromMillimeters(93),
        Dimension.FromMillimeters(125),
        Dimension.FromMillimeters(157),
        Dimension.FromMillimeters(189),
        Dimension.FromMillimeters(221),
        Dimension.FromMillimeters(253),
        Dimension.FromMillimeters(285),
        Dimension.FromMillimeters(317),
        Dimension.FromMillimeters(349),
        Dimension.FromMillimeters(477),
    };

    public ZargenDrawer(Guid id, int qty, decimal unitPrice, int productNumber, string room, string sku, Dimension openingWidth, Dimension height, Dimension depth, ClosetMaterial material, ClosetPaint? paint, string edgeBandingColor, string comment, IReadOnlyDictionary<string, string> parameters) {
        Id = id;
        Qty = qty;
        UnitPrice = unitPrice;
        ProductNumber = productNumber;
        Room = room;
        SKU = sku;
        OpeningWidth = openingWidth;
        Height = height;
        Depth = depth;
        Material = material;
        Paint = paint;
        EdgeBandingColor = edgeBandingColor;
        Comment = comment;
        Parameters = parameters;
    }

    public bool ContainsPPProducts() => true;

    public string GetDescription() => "Zargen Drawer Parts";

    public IEnumerable<PPProduct> GetPPProducts() {

        (string materialType, string finishMaterial, string ebMaterial) = Material.Core switch {
            ClosetMaterialCore.ParticleBoard => ("Melamine", "Mela", "PVC"),
            ClosetMaterialCore.Plywood => ("PLY", "Veneer", "Veneer"),
            _ => ("Melamine", "Mela", "PVC"),
        };

        var finishMaterials = new Dictionary<string, PPMaterial>() {
            ["F_DoorDrawer"] = new(finishMaterial, Material.Finish),
            ["F_DrawerBox"] = new(finishMaterial, Material.Finish),
            ["F_Panel"] = new(finishMaterial, Material.Finish)
        };

        var ebMaterials = new Dictionary<string, PPMaterial>() {
            ["EB_Bottom"] = new(ebMaterial, EdgeBandingColor),
            ["EB_DoorDrawer"] = new(ebMaterial, EdgeBandingColor),
            ["EB_DrawerBox"] = new(ebMaterial, EdgeBandingColor),
            ["EB_HandEdgeBand"] = new(ebMaterial, EdgeBandingColor),
            ["EB_Panel"] = new(ebMaterial, EdgeBandingColor),
            ["EB_Top"] = new(ebMaterial, EdgeBandingColor)
        };

        var parameters = new Dictionary<string, string>(Parameters) {
            ["OpeningWidth"] = OpeningWidth.AsMillimeters().ToString(),
            ["ProductHeight"] = Height.AsMillimeters().ToString(),
            ["ProductDepth"] = Depth.AsMillimeters().ToString()
        };

        return new List<PPProduct>() { new PPProduct(Id, Qty, Room, SKU, ProductNumber, "Royal_c", materialType, "slab", "standard", Comment, finishMaterials, ebMaterials, parameters, new Dictionary<string, string>(), new Dictionary<string, string>()) };

    }

    public IEnumerable<Supply> GetSupplies() => Enumerable.Empty<Supply>();

}
