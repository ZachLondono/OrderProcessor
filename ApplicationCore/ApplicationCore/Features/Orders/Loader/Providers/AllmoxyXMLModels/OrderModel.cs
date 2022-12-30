using System.Xml.Serialization;

namespace ApplicationCore.Features.Orders.Loader.Providers.AllmoxyXMLModels;

[XmlRoot("order")]
public class OrderModel {

    [XmlElement("number")]
    public int Number { get; set; }

	[XmlElement("name")]
	public string Name { get; set; } = string.Empty;

	[XmlElement("date")]
	public string OrderDate { get; set; } = string.Empty;

	[XmlElement("note")]
	public string Note { get; set; } = string.Empty;

	[XmlElement("description")]
	public string Description { get; set; } = string.Empty;

	[XmlElement("customer")]
	public CustomerModel Customer { get; set; } = new();

    [XmlElement("invoice")]
    public InvoiceModel Invoice { get; set; } = new();

    [XmlElement("shipping")]
    public ShippingModel Shipping { get; set; } = new();

	[XmlElement("products")]
	public ProductsModel Products { get; set; } = new();

}
