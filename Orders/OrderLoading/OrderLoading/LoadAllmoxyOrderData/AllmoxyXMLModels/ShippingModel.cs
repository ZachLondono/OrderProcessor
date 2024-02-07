using OrderLoading.LoadAllmoxyOrderData.AllmoxyXMLModels;
using System.Xml.Serialization;

namespace OrderLoading.LoadAllmoxyOrderData.AllmoxyXMLModels;

public class ShippingModel {

    [XmlElement("method")]
    public string Method { get; set; } = string.Empty;

    [XmlElement("date")]
    public string Date { get; set; } = string.Empty;

    [XmlElement("tax")]
    public decimal Tax { get; set; }

    [XmlElement("instructions")]
    public string Instructions { get; set; } = string.Empty;

    [XmlElement("attention")]
    public string Attn { get; set; } = string.Empty;

    [XmlElement("address")]
    public AddressModel Address { get; set; } = new();

}
