using System.Xml.Serialization;

namespace ApplicationCore.Features.Orders.OrderLoading.LoadAllmoxyOrderData.AllmoxyXMLModels;

public class RollOuts {

    [XmlElement("rollOutPos1")]
    public string Pos1 { get; set; } = string.Empty;

    [XmlElement("rollOutPos2")]
    public string Pos2 { get; set; } = string.Empty;

    [XmlElement("rollOutPos3")]
    public string Pos3 { get; set; } = string.Empty;

    [XmlElement("rollOutPos4")]
    public string Pos4 { get; set; } = string.Empty;

    [XmlElement("rollOutPos5")]
    public string Pos5 { get; set; } = string.Empty;

    [XmlElement("rollOutBlocks")]
    public string Blocks { get; set; } = string.Empty;

}