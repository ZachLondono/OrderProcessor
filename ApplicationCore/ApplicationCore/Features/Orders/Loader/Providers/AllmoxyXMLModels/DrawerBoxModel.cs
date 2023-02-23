using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Shared.Domain;
using System.Xml.Serialization;

namespace ApplicationCore.Features.Orders.Loader.Providers.AllmoxyXMLModels;

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

    public int GetProductNumber() => int.Parse($"{GroupNumber}{LineNumber:00}");

    public override IProduct CreateProduct(ProductBuilderFactory builderFactory) {

        decimal unitPrice = AllmoxyXMLOrderProviderHelpers.StringToMoney(UnitPrice);

        var height = Dimension.FromMillimeters(Height);
        var width = Dimension.FromMillimeters(Width);
        var depth = Dimension.FromMillimeters(Depth);

        var labelFields = new Dictionary<string, string>();

        var options = new DrawerBoxOptions(Material, Material, Material, Bottom, Clips, Notch, DrawerSlideType.Unknown, Insert, LogoPosition.None);

        return DovetailDrawerBoxProduct.Create(unitPrice, Qty, GetProductNumber(), height, width, depth, Note, labelFields, options);

    }

}