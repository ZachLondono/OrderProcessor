using System.Xml.Serialization;

namespace OrderLoading.LoadAllmoxyOrderData.AllmoxyXMLModels;

public class InvoiceModel {

	[XmlElement("preadjustment")]
	public string PreAdjustment { get; set; } = string.Empty;

	[XmlElement("adjustment")]
	public string PriceAdjustment { get; set; } = string.Empty;

	[XmlElement("subtotal")]
	public string Subtotal { get; set; } = string.Empty;

	[XmlElement("tax")]
	public string Tax { get; set; } = string.Empty;

	[XmlElement("shipping")]
	public string Shipping { get; set; } = string.Empty;

	[XmlElement("total")]
	public string Total { get; set; } = string.Empty;

	[XmlElement("address")]
	public AddressModel Address { get; set; } = new();

}