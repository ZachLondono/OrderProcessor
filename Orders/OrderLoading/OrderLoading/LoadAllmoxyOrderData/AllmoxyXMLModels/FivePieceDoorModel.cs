using Domain.Orders.Builders;
using Domain.Orders.Entities;
using Domain.Orders.ValueObjects;
using Domain.Orders.Entities.Products;
using Domain.ValueObjects;
using OneOf;
using System.Xml.Serialization;
using Domain.Orders.Entities.Products.Doors;
using Domain.Orders.Enums;

namespace OrderLoading.LoadAllmoxyOrderData.AllmoxyXMLModels;

public class FivePieceDoorModel : ProductOrItemModel {

    [XmlAttribute("groupNumber")]
    public int GroupNumber { get; set; }

    [XmlAttribute("lineNumber")]
    public int LineNumber { get; set; }

    [XmlAttribute("isDrawerFront")]
    public bool IsDrawerFront { get; set; } = false;

    [XmlElement("qty")]
    public int Qty { get; set; }

    [XmlElement("unitPrice")]
    public string UnitPrice { get; set; } = string.Empty;

    [XmlElement("room")]
    public string Room { get; set; } = string.Empty;

    [XmlElement("comment")]
    public string Comment { get; set; } = string.Empty;

    [XmlElement("materialFinish")]
    public string MaterialFinish { get; set; } = string.Empty;

    [XmlElement("materialCore")]
    public string MaterialCore { get; set; } = string.Empty;

    [XmlElement("rails")]
    public double Rails { get; set; }

    [XmlElement("height")]
    public double Height { get; set; }

    [XmlElement("width")]
    public double Width { get; set; }

    [XmlArray("productionNotes")]
    [XmlArrayItem(ElementName = "note", Type = typeof(string))]
    public List<string> ProductionNotes { get; set; } = new();

    public int GetProductNumber() => int.Parse($"{GroupNumber}{LineNumber:00}");

    public override OneOf<IProduct, AdditionalItem> CreateProductOrItem(ProductBuilderFactory builderFactory) {

        Dimension width = Dimension.FromMillimeters(Width);
        Dimension height = Dimension.FromMillimeters(Height);
        var frameSize = new DoorFrame(Dimension.FromMillimeters(Rails));
        var frameThickness = Dimension.FromMillimeters(19.05);
        var panelThickness = Dimension.FromMillimeters(6.35);

        decimal unitPrice = AllmoxyXMLOrderProviderHelpers.StringToMoney(UnitPrice);

        DoorType doorType = IsDrawerFront switch {
            true => DoorType.DrawerFront,
            false => DoorType.Door
        };

        return new FivePieceDoorProduct(Guid.NewGuid(), Qty, unitPrice, GetProductNumber(), Room, width, height, frameSize, frameThickness, panelThickness, MaterialFinish, doorType) {
            ProductionNotes = ProductionNotes.Where(n => !string.IsNullOrWhiteSpace(n)).ToList()
        };

    }

}
