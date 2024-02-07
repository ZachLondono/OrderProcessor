using Domain.Orders.Builders;
using Domain.Orders.Entities;
using Domain.Orders.Enums;
using Domain.Orders.Entities.Products.Cabinets;
using Domain.Orders.ValueObjects;
using Domain.Orders.Entities.Products;
using OneOf;
using System.Xml.Serialization;

namespace OrderLoading.LoadAllmoxyOrderData.AllmoxyXMLModels;

public class CabinetPartModel : ProductOrItemModel {

    [XmlAttribute("groupNumber")]
    public int GroupNumber { get; set; }

    [XmlAttribute("lineNumber")]
    public int LineNumber { get; set; }

    [XmlElement("room")]
    public string Room { get; set; } = string.Empty;

    [XmlElement("sku")]
    public string SKU { get; set; } = string.Empty;

    [XmlElement("qty")]
    public int Qty { get; set; }

    [XmlElement("unitPrice")]
    public string UnitPrice { get; set; } = string.Empty;

    [XmlElement("edgeBandColor")]
    public string EdgeBandColor { get; set; } = string.Empty;

    [XmlElement("materialFinish")]
    public string MaterialFinish { get; set; } = string.Empty;

    [XmlElement("materialFinishType")]
    public string MaterialFinishType { get; set; } = string.Empty;

    [XmlElement("materialCore")]
    public string MaterialCore { get; set; } = string.Empty;

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

        CabinetMaterialCore core = MaterialCore switch {
            AllmoxyXMLOrderProviderHelpers.PARTICLE_BOARD_CORE_CODE => CabinetMaterialCore.ParticleBoard,
            AllmoxyXMLOrderProviderHelpers.PLYWOOD_CORE_CODE => CabinetMaterialCore.Plywood,
            _ => throw new InvalidOperationException($"Unexpected material core type '{MaterialCore}'"),
        };

        CabinetMaterialFinishType finishType = MaterialFinishType switch {
            AllmoxyXMLOrderProviderHelpers.MELAMINE_FINISH_CODE => CabinetMaterialFinishType.Melamine,
            AllmoxyXMLOrderProviderHelpers.VENEER_FINISH_CODE => CabinetMaterialFinishType.Veneer,
            AllmoxyXMLOrderProviderHelpers.PAINT_FINISH_CODE => CabinetMaterialFinishType.Paint,
            AllmoxyXMLOrderProviderHelpers.NO_FINISH_CODE => CabinetMaterialFinishType.None,
            _ => throw new InvalidOperationException($"Unrecognized material finish type '{MaterialFinishType}'")
        };

        CabinetMaterial material = new(MaterialFinish, finishType, core);

        decimal unitPrice = AllmoxyXMLOrderProviderHelpers.StringToMoney(UnitPrice);

        IDictionary<string, string> parameters = Parameters.ToDictionary(p => p.Name, p => p.Value);

        string edgeBandColor = EdgeBandColor == "Match" ? MaterialFinish : EdgeBandColor;

        return new CabinetPart(Guid.NewGuid(), Qty, unitPrice, GetProductNumber(), SKU, Room, material, edgeBandColor, Comment, parameters, ProductionNotes.Where(n => !string.IsNullOrWhiteSpace(n)).ToList());
    }

}
