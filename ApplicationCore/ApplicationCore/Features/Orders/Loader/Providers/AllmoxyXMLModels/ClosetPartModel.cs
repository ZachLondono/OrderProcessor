using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Shared.Domain;
using System.Xml.Serialization;

namespace ApplicationCore.Features.Orders.Loader.Providers.AllmoxyXMLModels;

public class ClosetPartModel : ProductModel {

    [XmlAttribute("groupNumber")]
    public int GroupNumber { get; set; }

    [XmlAttribute("lineNumber")]
    public int LineNumber { get; set; }

    [XmlElement("room")]
    public string Room { get; set; } = string.Empty;

    [XmlElement("sku")]
    public string SKU { get; set; } = string.Empty;

    [XmlElement("width")]
    public double Width { get; set; }
    
    [XmlElement("length")]
    public double Length { get; set; }
    
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
    
    [XmlElement("comment")]
    public string Comment { get; set; } = string.Empty;

    [XmlArray("parameters")]
    public List<Parameter> Parameters { get; set; } = new();

    public int GetProductNumber() => int.Parse($"{GroupNumber}{LineNumber:00}");

    public override IProduct CreateProduct(ProductBuilderFactory builderFactory) {

        ClosetMaterialCore core = MaterialCore switch {
            "flake" => ClosetMaterialCore.Flake,
            "ply" => ClosetMaterialCore.Plywood,
            _ => ClosetMaterialCore.Flake,
        };

        string? paintColor = string.IsNullOrWhiteSpace(PaintColor) ? null : PaintColor;

        ClosetMaterial material = new(MaterialFinish, core, paintColor);

        Dimension width = Dimension.FromMillimeters(Width);
        Dimension length = Dimension.FromMillimeters(Length);

        decimal unitPrice = AllmoxyXMLOrderProviderHelpers.StringToMoney(UnitPrice);

        IReadOnlyDictionary<string, string> parameters = Parameters.ToDictionary(p => p.Name, p => p.Value).AsReadOnly();

        return new ClosetPart(Guid.NewGuid(), Qty, unitPrice, GetProductNumber(), Room, SKU, width, length, material, EdgeBandColor, Comment, parameters);

    }

    public class Parameter {

        [XmlAttribute("name")]
        public string Name { get; set; } = string.Empty;

        [XmlAttribute("value")]
        public string Value { get; set; } = string.Empty;

    }

}