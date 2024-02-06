using Domain.Orders.Builders;
using Domain.Orders.Entities;
using Domain.Orders.Enums;
using Domain.Orders.Products.Closets;
using Domain.Orders.ValueObjects;
using Domain.Orders.Entities.Products;
using Domain.ValueObjects;
using OneOf;
using System.Xml.Serialization;
using ClosetPaintedSide = Domain.Orders.ValueObjects.PaintedSide;

namespace ApplicationCore.Features.Orders.OrderLoading.LoadAllmoxyOrderData.AllmoxyXMLModels;

public class ZargenDrawerModel : ProductOrItemModel {

    [XmlAttribute("groupNumber")]
    public int GroupNumber { get; set; }

    [XmlAttribute("lineNumber")]
    public int LineNumber { get; set; }

    [XmlElement("room")]
    public string Room { get; set; } = string.Empty;

    [XmlElement("sku")]
    public string SKU { get; set; } = string.Empty;

    [XmlElement("openingWidth")]
    public double OpeningWidth { get; set; }

    [XmlElement("Height")]
    public double Height { get; set; }

    [XmlElement("Depth")]
    public double Depth { get; set; }

    [XmlElement("qty")]
    public int Qty { get; set; }

    [XmlElement("unitPrice")]
    public string UnitPrice { get; set; } = string.Empty;

    [XmlElement("edgeBandColor")]
    public string EdgeBandColor { get; set; } = string.Empty;

    [XmlElement("materialFinish")]
    public string MaterialFinish { get; set; } = string.Empty;

    [XmlElement("materialCore")]
    public string MaterialCore { get; set; } = string.Empty;

    [XmlElement("paintColor")]
    public string PaintColor { get; set; } = string.Empty;

    [XmlElement("paintedSide")]
    public string PaintedSide { get; set; } = string.Empty;

    [XmlElement("comment")]
    public string Comment { get; set; } = string.Empty;

    [XmlArray("parameters")]
    [XmlArrayItem(ElementName = "entry", Type = typeof(PSIParameter))]
    public List<PSIParameter> Parameters { get; set; } = new();

    [XmlArray("productionNotes")]
    [XmlArrayItem(ElementName = "note", Type = typeof(string))]
    public List<string> ProductionNotes { get; set; } = new();

    public int GetProductNumber() => int.Parse($"{GroupNumber}{LineNumber:00}");

    public override OneOf<IProduct, AdditionalItem> CreateProductOrItem(ProductBuilderFactory builderFactory) {

        ClosetMaterialCore core = MaterialCore switch {
            AllmoxyXMLOrderProviderHelpers.PARTICLE_BOARD_CORE_CODE => ClosetMaterialCore.ParticleBoard,
            AllmoxyXMLOrderProviderHelpers.PLYWOOD_CORE_CODE => ClosetMaterialCore.Plywood,
            _ => throw new InvalidOperationException($"Unexpected material core type '{MaterialCore}'"),
        };

        ClosetMaterial material = new(MaterialFinish, core);

        string? paintColor = string.IsNullOrWhiteSpace(PaintColor) ? null : PaintColor;
        ClosetPaint? paint = null;
        if (paintColor is not null) {
            if (Enum.TryParse(PaintedSide, out ClosetPaintedSide paintedSide)) {
                paint = new(paintColor, paintedSide);
            } else {
                paint = new(paintColor, ClosetPaintedSide.Custom);
            }

        }

        Dimension openingWidth = Dimension.FromMillimeters(OpeningWidth);
        Dimension height = Dimension.FromMillimeters(Height);
        Dimension depth = Dimension.FromMillimeters(Depth);

        decimal unitPrice = AllmoxyXMLOrderProviderHelpers.StringToMoney(UnitPrice);

        IReadOnlyDictionary<string, string> parameters = Parameters.ToDictionary(p => p.Name, p => p.Value).AsReadOnly();

        string edgeBandColor = EdgeBandColor == "Match" ? MaterialFinish : EdgeBandColor;

        return new ZargenDrawer(Guid.NewGuid(), Qty, unitPrice, GetProductNumber(), Room, SKU, openingWidth, height, depth, material, paint, edgeBandColor, Comment, parameters) {
            ProductionNotes = ProductionNotes.Where(n => !string.IsNullOrWhiteSpace(n)).ToList()
        };

    }

}