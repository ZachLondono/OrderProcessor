using System.Xml.Serialization;

namespace ApplicationCore.Features.Orders.Loader.Providers.RichelieuXMLModels;

public class DimensionModel {

    [XmlAttribute("unite")]
    public required string Unite { get; set; }

    [XmlAttribute("quantite")]
    public required string Quantite { get; set; }

    [XmlAttribute("HEIGHT")]
    public required string Height { get; set; }

    [XmlAttribute("WIDTH")]
    public required string Width { get; set; }

    [XmlAttribute("DEPTH")]
    public required string Depth { get; set; }

    [XmlAttribute("note")]
    public required string Note { get; set; }

    [XmlAttribute("qty")]
    public required string Qty { get; set; }

    [XmlAttribute("price")]
    public required string Price { get; set; }

}