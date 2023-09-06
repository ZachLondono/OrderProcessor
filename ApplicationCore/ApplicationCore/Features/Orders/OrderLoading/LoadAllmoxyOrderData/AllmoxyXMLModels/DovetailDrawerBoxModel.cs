using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.Products.DrawerBoxes;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Shared.Domain;
using System.Xml.Serialization;

namespace ApplicationCore.Features.Orders.OrderLoading.LoadAllmoxyOrderData.AllmoxyXMLModels;

public class DovetailDrawerBoxModel : ProductModel {

    [XmlAttribute("groupNumber")]
    public int GroupNumber { get; set; }

    [XmlAttribute("lineNumber")]
    public int LineNumber { get; set; }

    [XmlElement("comments")]
    public string Comments { get; set; } = string.Empty;

    [XmlElement("material")]
    public string Material { get; set; } = string.Empty;

    [XmlElement("bottom")]
    public string Bottom { get; set; } = string.Empty;

    [XmlElement("clips")]
    public string Clips { get; set; } = string.Empty;

    [XmlElement("notch")]
    public string Notch { get; set; } = string.Empty;

    [XmlElement("insert")]
    public string Insert { get; set; } = string.Empty;

    [XmlElement("unitPrice")]
    public string UnitPrice { get; set; } = string.Empty;

    [XmlElement("qty")]
    public int Qty { get; set; }

    [XmlElement("logo")]
    public string Logo { get; set; } = string.Empty;

    [XmlElement("scoop")]
    public string Scoop { get; set; } = string.Empty;

    [XmlElement("note")]
    public string Note { get; set; } = string.Empty;

    [XmlElement("height")]
    public double Height { get; set; }

    [XmlElement("width")]
    public double Width { get; set; }

    [XmlElement("depth")]
    public double Depth { get; set; }

    [XmlElement("room")]
    public string Room { get; set; } = string.Empty;

    [XmlArray("productionNotes")]
    [XmlArrayItem(ElementName = "note", Type = typeof(string))]
    public List<string> ProductionNotes { get; set; } = new();

    public int GetProductNumber() => int.Parse($"{GroupNumber}{LineNumber:00}");

    public override IProduct CreateProduct(ProductBuilderFactory builderFactory) {

        decimal unitPrice = AllmoxyXMLOrderProviderHelpers.StringToMoney(UnitPrice);

        var height = Dimension.FromMillimeters(Height);
        var width = Dimension.FromMillimeters(Width);
        var depth = Dimension.FromMillimeters(Depth);

        var labelFields = new Dictionary<string, string>();

        if (Material == "Pre-Finished Birch")
            Material = DovetailDrawerBoxConfig.SOLID_BIRCH;
        else if (Material == "Economy Birch")
            Material = DovetailDrawerBoxConfig.FINGER_JOINT_BIRCH;

        var options = new DovetailDrawerBoxConfig(Material, Material, Material, Bottom, Clips, Notch, Insert, LogoPosition.None);

        var product = DovetailDrawerBoxProduct.Create(unitPrice, Qty, Room, GetProductNumber(), height, width, depth, Note, labelFields, options);
        product.ProductionNotes = ProductionNotes.Where(n => !string.IsNullOrWhiteSpace(n)).ToList();
        return product;

    }

}
