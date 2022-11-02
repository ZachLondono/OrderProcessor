using System.Xml.Serialization;

namespace ApplicationCore.Features.Orders.Providers.AllmoxyXMLModels;

public class DrawerBoxModel {

    [XmlElement("comment")]
    public string Comment { get; set; } = string.Empty;

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

    [XmlElement("price")]
    public decimal Price { get; set; }

    [XmlElement("qty")]
    public int Qty { get; set; }

    [XmlElement("logo")]
    public string Logo { get; set; } = string.Empty;

    [XmlElement("scoop")]
    public string Scoop { get; set; } = string.Empty;

    [XmlElement("note")]
    public string Note { get; set; } = string.Empty;

    [XmlElement("dimensions")]
    public DimensionsModel Dimensions { get; set; } = new();

}
