using System.Xml.Serialization;

namespace ApplicationCore.Features.Orders.Loader.Providers.AllmoxyXMLModels;

public class BaseCabinetModel {

	[XmlElement("cabinet")]
	public CabinetModel Cabinet { get; set; } = new();

    [XmlElement("toeType")]
	public string ToeType { get; set; } = string.Empty;

	[XmlElement("hingeSide")]
	public string HingeSide { get; set; } = string.Empty;

	[XmlElement("doorQty")]
	public int DoorQty { get; set; }

	[XmlElement("drawerQty")]
	public int DrawerQty { get; set; }

	[XmlElement("drawerSlide")]
	public string DrawerSlide { get; set; } = string.Empty;

	[XmlElement("drawerFaceHeight")]
	public double DrawerFaceHeight { get; set; }

	[XmlElement("adjShelfQty")]
	public int AdjShelfQty { get; set; }

	[XmlElement("vertDivQty")]
	public int VerticalDividerQty { get; set; }

	[XmlElement("rollOuts")]
	public RollOuts RollOuts { get; set; } = new();

}