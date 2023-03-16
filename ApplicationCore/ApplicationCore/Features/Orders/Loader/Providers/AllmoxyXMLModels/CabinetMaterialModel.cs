using System.Xml.Serialization;

namespace ApplicationCore.Features.Orders.Loader.Providers.AllmoxyXMLModels;

public class CabinetMaterialModel {

    [XmlElement("core")]
    public string Core { get; set; } = string.Empty;

    [XmlElement("type")]
    public string Type { get; set; } = string.Empty;

    [XmlElement("finish")]
    public string Finish { get; set; } = string.Empty;
}
