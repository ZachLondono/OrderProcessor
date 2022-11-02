using System.Xml.Serialization;

namespace ApplicationCore.Features.Orders.Providers.AllmoxyXMLModels;

public class AllmoxyOrder {

    [XmlElement("order")]
    public OrderModel Order { get; set; } = new();

}
