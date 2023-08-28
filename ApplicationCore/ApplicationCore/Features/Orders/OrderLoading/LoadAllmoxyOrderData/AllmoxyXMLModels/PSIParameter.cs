using System.Xml.Serialization;

namespace ApplicationCore.Features.Orders.OrderLoading.LoadAllmoxyOrderData.AllmoxyXMLModels;

public class PSIParameter {

    [XmlElement("name")]
    public string Name { get; set; } = string.Empty;

    [XmlElement("value")]
    public string Value { get; set; } = string.Empty;

}
