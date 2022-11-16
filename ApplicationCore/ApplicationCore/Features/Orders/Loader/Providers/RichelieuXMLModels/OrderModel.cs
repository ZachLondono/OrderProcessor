using System.Xml.Serialization;

namespace ApplicationCore.Features.Orders.Loader.Providers.RichelieuXMLModels;

public class OrderModel {

    [XmlElement("shipTo")]
    public required ShipToModel ShipTo { get; set; }

    [XmlElement("header")]
    public required HeaderModel Header { get; set; }

    [XmlElement("line")]
    public required LineModel[] Lines { get; set; }

}
