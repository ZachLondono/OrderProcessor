using System.Xml.Serialization;

namespace ApplicationCore.Features.Orders.Loader.Providers.AllmoxyXMLModels;

[XmlRoot("order")]
public class OrderModel {

    [XmlAttribute("id")]
    public int Id { get; set; }

    [XmlElement("customer")]
    public string Customer { get; set; } = string.Empty;

    [XmlElement("customerId")]
    public int CustomerId { get; set; }

    [XmlElement("name")]
    public string Name { get; set; } = string.Empty;

    [XmlElement("note")]
    public string Note { get; set; } = string.Empty;

    [XmlElement("date")]
    public string OrderDate { get; set; } = string.Empty;

    [XmlElement("description")]
    public string Description { get; set; } = string.Empty;

    [XmlElement("status")]
    public string Status { get; set; } = string.Empty;

    [XmlElement("total")]
    public decimal Total { get; set; }

    [XmlElement("invoice")]
    public InvoiceModel Invoice { get; set; } = new();

    [XmlElement("shipping")]
    public ShippingModel Shipping { get; set; } = new();

    [XmlElement("DrawerBox")]
    public DrawerBoxModel[] DrawerBoxes { get; set; } = Array.Empty<DrawerBoxModel>();

}
