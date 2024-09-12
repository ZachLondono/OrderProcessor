using Domain.Orders.ValueObjects;
using Domain.ValueObjects;
using Domain.ProductPlanner;
using Domain.Orders.Enums;

namespace Domain.Orders.Entities.Products.Closets;

public partial class ClosetPart : IPPProductContainer, IClosetPartProduct {

    public Guid Id { get; }
    public int Qty { get; set; }
    public decimal UnitPrice { get; }
    public int ProductNumber { get; }
    public string Room { get; set; }
    public string SKU { get; set; }
    public Dimension Width { get; }
    public Dimension Length { get; }
    public ClosetMaterial Material { get; }
    public ClosetPaint? Paint { get; }
    public string EdgeBandingColor { get; }
    public string Comment { get; set; }
    public bool InstallCams { get; set; }
    public IDictionary<string, string> Parameters { get; }
    public List<ProductionNote> ProductionNotes { get; set; } = [];

    public string GetDescription() => $"Closet Part - {SKU}";

    public ClosetPart(Guid id, int qty, decimal unitPrice, int productNumber, string room, string sku, Dimension width, Dimension length, ClosetMaterial material, ClosetPaint? paint, string edgeBandingColor, string comment, bool installCams, IDictionary<string, string> parameters) {
        Id = id;
        Qty = qty;
        UnitPrice = unitPrice;
        ProductNumber = productNumber;
        Room = room;
        SKU = sku;
        Width = width;
        Length = length;
        Material = material;
        Paint = paint;
        EdgeBandingColor = edgeBandingColor;
        Comment = comment;
        InstallCams = installCams;
        Parameters = parameters;
    }

    public bool ContainsPPProducts() => true;

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
            ["ProductWidth"] = Width.AsMillimeters().ToString(),
            ["ProductLength"] = Length.AsMillimeters().ToString()
        };

        return new List<PPProduct>() { new PPProduct(Id, Qty, Room, SKU, ProductNumber, "Royal_c", materialType, "slab", "standard", Comment, finishMaterials, ebMaterials, parameters, new Dictionary<string, string>(), new Dictionary<string, string>()) };

    }

}
