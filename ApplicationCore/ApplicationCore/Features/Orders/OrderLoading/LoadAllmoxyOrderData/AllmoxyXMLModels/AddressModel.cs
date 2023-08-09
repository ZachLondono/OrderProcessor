using System.Xml.Serialization;

namespace ApplicationCore.Features.Orders.OrderLoading.LoadAllmoxyOrderData.AllmoxyXMLModels;

public class AddressModel {

    [XmlElement("line1")]
    public string Line1 { get; set; } = string.Empty;

    [XmlElement("line2")]
    public string Line2 { get; set; } = string.Empty;

    [XmlElement("line3")]
    public string Line3 { get; set; } = string.Empty;

    [XmlElement("city")]
    public string City { get; set; } = string.Empty;

    [XmlElement("state")]
    public string State { get; set; } = string.Empty;

    [XmlElement("zip")]
    public string Zip { get; set; } = string.Empty;

    [XmlElement("country")]
    public string Country { get; set; } = string.Empty;

}
