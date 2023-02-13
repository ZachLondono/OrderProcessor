using ApplicationCore.Features.Orders.Details.OrderExport.Handlers.ExtExport.Contracts;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.Products;

public class ClosetPart : IProduct, IPPProductContainer {

    public Guid Id { get; }
    public int Qty { get; }
    public decimal UnitPrice { get; }
    public int ProductNumber { get; }
    public string Room { get; }
    public string SKU { get; }
    public Dimension Width { get; }
    public Dimension Length { get; }
    public ClosetMaterial Material { get; }
    public string EdgeBandingColor { get; }
    public string Comment { get; }
    public IReadOnlyDictionary<string, string> Parameters { get; }

    public string Description => $"Closet Part - {SKU}";

    public ClosetPart(Guid id, int qty, decimal unitPrice, int productNumber, string room, string sku, Dimension width, Dimension length, ClosetMaterial material, string edgeBandingColor, string comment, IReadOnlyDictionary<string, string> parameters) {
        Id = id;
        Qty = qty;
        UnitPrice = unitPrice;
        ProductNumber = productNumber;
        Room = room;
        SKU = sku;
        Width = width;
        Length = length;
        Material = material;
        EdgeBandingColor = edgeBandingColor;
        Comment = comment;
        Parameters = parameters;
    }

    public IEnumerable<PPProduct> GetPPProducts() {

        (string materialType, string finishMaterial, string ebMaterial) = Material.Core switch {
            ClosetMaterialCore.Flake => ("Melamine", "Mela", "PVC"),
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

        return new List<PPProduct>() { new PPProduct(Id, Room, SKU, ProductNumber, "Royal_c", materialType, "slab", "standard", Comment, finishMaterials, ebMaterials, new Dictionary<string, string>(Parameters), new Dictionary<string, string>(), new Dictionary<string, string>()) };
    }

    public IEnumerable<Supply> GetSupplies() => Enumerable.Empty<Supply>();

}
