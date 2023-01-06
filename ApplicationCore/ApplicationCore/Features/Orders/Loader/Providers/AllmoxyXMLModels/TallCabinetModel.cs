using System.Xml.Serialization;

namespace ApplicationCore.Features.Orders.Loader.Providers.AllmoxyXMLModels;

public class TallCabinetModel {

    [XmlAttribute("groupNumber")]
    public int GroupNumber { get; set; }

    [XmlAttribute("lineNumber")]
    public int LineNumber { get; set; }

    [XmlElement("cabinet")]
    public CabinetModel Cabinet { get; set; } = new();

    [XmlElement("toeType")]
    public string ToeType { get; set; } = string.Empty;

    [XmlElement("hingeSide")]
    public string HingeSide { get; set; } = string.Empty;

    [XmlElement("upperDoorQty")]
    public int UpperDoorQty { get; set; }

    [XmlElement("lowerDoorQty")]
    public int LowerDoorQty { get; set; }

    [XmlElement("lowerDoorHeight")]
    public double LowerDoorHeight { get; set; }

    [XmlElement("drawerMaterial")]
    public string DrawerMaterial { get; set; } = string.Empty;

    [XmlElement("drawerSlide")]
    public string DrawerSlide { get; set; } = string.Empty;

    [XmlElement("upperAdjShelfQty")]
    public int UpperAdjShelfQty { get; set; }

    [XmlElement("adjShelfQty")]
    public int LowerAdjShelfQty { get; set; }

    [XmlElement("upperVertDivQty")]
    public int UpperVerticalDividerQty { get; set; }

    [XmlElement("vertDivQty")]
    public int LowerVerticalDividerQty { get; set; }

    [XmlElement("rollOuts")]
    public RollOuts RollOuts { get; set; } = new();

}
