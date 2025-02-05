using Domain.Orders.Entities.Hardware;
using Domain.Orders.Enums;
using Domain.Orders.ValueObjects;
using Domain.ProductPlanner;

namespace Domain.Orders.Entities.Products.Cabinets;

public class CabinetPart : IProduct, IPPProductContainer {

    public Guid Id { get; }
    public int Qty { get; }
    public decimal UnitPrice { get; }
    public int ProductNumber { get; }
    public string SKU { get; }
    public string Room { get; set; }
    public CabinetMaterial Material { get; }
    public string EdgeBandingColor { get; }
    public string Comment { get; }
    public IDictionary<string, string> Parameters { get; }
    public List<string> ProductionNotes { get; }

    public CabinetPart(Guid id, int qty, decimal unitPrice, int productNumber, string sku, string room, CabinetMaterial material, string edgeBandingColor, string comment, IDictionary<string, string> parameters, List<string> productionNotes) {
        Id = id;
        Qty = qty;
        UnitPrice = unitPrice;
        ProductNumber = productNumber;
        SKU = sku;
        Room = room;
        Material = material;
        EdgeBandingColor = edgeBandingColor;
        Comment = comment;
        Parameters = parameters;
        ProductionNotes = productionNotes;
    }

    public string GetDescription() => $"Cabinet Extra - {SKU}";

    public bool ContainsPPProducts() => true;

    public IEnumerable<PPProduct> GetPPProducts() {
        return new List<PPProduct>() { new PPProduct(Id, Qty, Room, SKU, ProductNumber, "Royal2", GetMaterialType(), "Buyout", "Standard", Comment, GetFinishMaterials(), GetEBMaterials(), Parameters, new Dictionary<string, string>(), new Dictionary<string, string>(), new Dictionary<string, double>()) };
    }

    private string GetMaterialType() => Material.Core switch {
        CabinetMaterialCore.Plywood => "Sterling 18_5",
        CabinetMaterialCore.ParticleBoard => "Crown Paint",
        _ => "Crown Paint"
    };

    private Dictionary<string, PPMaterial> GetFinishMaterials() {
        string material = Cabinet.GetFinishMaterialType(Material.Core);
        var materials = new Dictionary<string, PPMaterial> {
            ["F_Exp_SemiExp"] = new PPMaterial(material, Material.Finish),
            ["F_Exp_Unseen"] = new PPMaterial(material, Material.Finish),
            ["F_Exposed"] = new PPMaterial(material, Material.Finish),
            ["F_OvenSupport"] = new PPMaterial("Veneer", "PRE"),
            ["F_SemiExp_Unseen"] = new PPMaterial(material, Material.Finish),
            ["F_SemiExposed"] = new PPMaterial(material, Material.Finish),
            ["F_Door"] = new PPMaterial(material, Material.Finish),
            ["F_DoorBack"] = new PPMaterial(material, Material.Finish),
        };
        return materials;
    }

    private Dictionary<string, PPMaterial> GetEBMaterials() {
        string ebMaterial = Cabinet.GetEBMaterialType(Material.Core);
        return new Dictionary<string, PPMaterial>() {
            ["EB_Case"] = new PPMaterial(ebMaterial, EdgeBandingColor),
            ["EB_Inside"] = new PPMaterial(ebMaterial, EdgeBandingColor),
            ["EB_ShellExposed"] = new PPMaterial(ebMaterial, EdgeBandingColor),
            ["EB_WallBottom"] = new PPMaterial(ebMaterial, EdgeBandingColor),
        };
    }

}
