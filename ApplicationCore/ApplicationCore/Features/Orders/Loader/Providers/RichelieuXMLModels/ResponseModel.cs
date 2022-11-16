using System.Xml.Serialization;

namespace ApplicationCore.Features.Orders.Loader.Providers.RichelieuXMLModels;

internal class ResponseModel {

    [XmlElement("order")]
    public required OrderModel Order { get; set; }

}
