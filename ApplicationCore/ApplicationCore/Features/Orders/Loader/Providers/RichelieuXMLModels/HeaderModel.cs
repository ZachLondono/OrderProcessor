using System.Xml.Serialization;

namespace ApplicationCore.Features.Orders.Loader.Providers.RichelieuXMLModels;

public class HeaderModel {

    [XmlAttribute("orderDate")]
    public required string OrderDate { get; set; }

    [XmlAttribute("estimatedShippingLeadTime")]
    public required string EstimatedShippingLeadTime { get; set; }

    [XmlAttribute("webOrder")]
    public required string WebOrder { get; set; }

    [XmlAttribute("richelieuOrder")]
    public required string RichelieuOrder { get; set; }

    [XmlAttribute("richelieuPO")]
    public required string RichelieuPO { get; set; }

    [XmlAttribute("clientPO")]
    public required string ClientPO { get; set; }

    [XmlAttribute("currency")]
    public required string Currency { get; set; }

    [XmlAttribute("items")]
    public required string Items { get; set; }

    [XmlAttribute("totalCost")]
    public required string TotalCost { get; set; }

    [XmlAttribute("lang")]
    public required string Lang { get; set; }

    [XmlAttribute("statusFr")]
    public required string StatusFr { get; set; }

    [XmlAttribute("statusEn")]
    public required string StatusEn { get; set; }

    [XmlAttribute("typeCommandeFr")]
    public required string TypeCommandeFr { get; set; }

    [XmlAttribute("typeCommandeEn")]
    public required string TypeCommandeEn { get; set; }

    [XmlAttribute("typeCommandeNote")]
    public required string TypeCommandeNote { get; set; }

    [XmlAttribute("billingRegionFr")]
    public required string BillingRegionFr { get; set; }

    [XmlAttribute("billingRegionEn")]
    public required string BillingRegionEn { get; set; }

    [XmlAttribute("billingRegionCode")]
    public required string BillingRegionCode { get; set; }

    [XmlAttribute("note")]
    public required string Note { get; set; }

    [XmlAttribute("tsConfirmeXML")]
    public required string TsConfirmeXML {  get; set; }


}
