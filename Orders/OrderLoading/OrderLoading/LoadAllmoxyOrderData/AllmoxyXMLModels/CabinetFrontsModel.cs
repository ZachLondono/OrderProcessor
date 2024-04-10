using System.Xml.Serialization;

namespace OrderLoading.LoadAllmoxyOrderData.AllmoxyXMLModels;

public class CabinetFrontsModel {

	[XmlElement("type")]
	public string Type { get; set; } = string.Empty;

	[XmlElement("style")]
	public string Style { get; set; } = string.Empty;

	[XmlElement("color")]
	public string Color { get; set; } = string.Empty;

	[XmlElement("finish_type")]
	public string FinishType { get; set; } = string.Empty;

}