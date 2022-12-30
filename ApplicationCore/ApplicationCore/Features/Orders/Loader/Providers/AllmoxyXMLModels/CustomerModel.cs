using System.Xml.Serialization;

namespace ApplicationCore.Features.Orders.Loader.Providers.AllmoxyXMLModels;

public class CustomerModel {

	[XmlAttribute("id")]
	public int CompanyId { get; set; }

	[XmlText]
	public string Company { get; set; } = string.Empty;

}
