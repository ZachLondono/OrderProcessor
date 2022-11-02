using System.Xml.Serialization;

namespace ApplicationCore.Features.Orders.Providers.AllmoxyXMLModels;

public class ShippingModel {

    [XmlElement("instructions")]
    public string Instructions { get; set; } = string.Empty;

    [XmlElement("method")]
    public string Method { get; set; } = string.Empty;

    [XmlElement("attn")]
    public string Attn { get; set; } = string.Empty;

    [XmlElement("address")]
    public AddressModel Address { get; set; } = new();

}
