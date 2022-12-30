using System.Xml.Serialization;

namespace ApplicationCore.Features.Orders.Loader.Providers.AllmoxyXMLModels;

public class CabinetMaterialsModel {

	[XmlElement("boxMaterial")]
	public CabinetMaterialModel BoxMaterial { get; set; } = new();

	[XmlElement("finishMaterial")]
	public CabinetMaterialModel FinishMaterial { get; set; } = new();

	[XmlElement("edgeBandColor")]
	public string EdgeBandColor { get; set; } = string.Empty;

	[XmlElement("doorColor")]
	public string DoorColor { get; set; } = string.Empty;

}
