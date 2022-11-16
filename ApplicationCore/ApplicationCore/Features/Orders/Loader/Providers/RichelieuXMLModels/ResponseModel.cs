using System.Xml.Serialization;

namespace ApplicationCore.Features.Orders.Loader.Providers.RichelieuXMLModels;

[XmlRoot("response")]
public class ResponseModel {

    [XmlElement("order")]
    public required OrderModel Order { get; set; }

}
