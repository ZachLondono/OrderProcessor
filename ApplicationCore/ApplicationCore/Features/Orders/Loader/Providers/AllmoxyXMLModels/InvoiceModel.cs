using System.Xml.Serialization;

namespace ApplicationCore.Features.Orders.Providers.AllmoxyXMLModels;

public class InvoiceModel {

    [XmlElement("subtotal")]
    public decimal Subtotal { get; set; }

    [XmlElement("tax")]
    public decimal Tax { get; set; }

    [XmlElement("shipping")]
    public decimal Shipping { get; set; }

    [XmlElement("total")]
    public decimal Total { get; set; }

}