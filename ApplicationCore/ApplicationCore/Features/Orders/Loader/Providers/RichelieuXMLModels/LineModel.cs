using System.Xml.Serialization;

namespace ApplicationCore.Features.Orders.Loader.Providers.RichelieuXMLModels;

public class LineModel {

    [XmlAttribute("sku")]
    public required string Sku { get; set; }

    [XmlAttribute("descriptionFr")]
    public required string DescriptionFr { get; set; }

    [XmlAttribute("descriptionEn")]
    public required string DescriptionEn { get; set; }

    [XmlAttribute("note")]
    public required string Note { get; set; }

    [XmlElement("dimension")]
    public required DimensionModel[] Dimensions { get; set; }

}
