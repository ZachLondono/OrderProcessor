using System.Xml.Serialization;

namespace ApplicationCore.Features.Orders.Loader.Providers.AllmoxyXMLModels;

public class DrawerBoxModel {

	[XmlElement("qty")]
	public int Qty { get; set; }

	[XmlElement("unitPrice")]
	public decimal UnitPrice { get; set; }

}