using System.Xml.Serialization;

namespace ApplicationCore.Features.Orders.Loader.Providers.AllmoxyXMLModels;

public class DrawerBaseCabinetModel {

    [XmlAttribute("groupNumber")]
    public int GroupNumber { get; set; }

    [XmlAttribute("lineNumber")]
    public int LineNumber { get; set; }

    [XmlElement("cabinet")]
    public CabinetModel Cabinet { get; set; } = new();

    [XmlElement("toeType")]
    public string ToeType { get; set; } = string.Empty;

    [XmlElement("drawerMaterial")]
    public string DrawerMaterial { get; set; } = string.Empty;

    [XmlElement("drawerSlide")]
    public string DrawerSlide { get; set; } = string.Empty;

    [XmlElement("drawerQty")]
    public int DrawerQty { get; set; }

    [XmlElement("drawerFace1")]
    public double DrawerFace1 { get; set; }

    [XmlElement("drawerFace2")]
    public double DrawerFace2 { get; set; }

    [XmlElement("drawerFace3")]
    public double DrawerFace3 { get; set; }

    [XmlElement("drawerFace4")]
    public double DrawerFace4 { get; set; }

    [XmlElement("drawerFace5")]
    public double DrawerFace5 { get; set; }

}