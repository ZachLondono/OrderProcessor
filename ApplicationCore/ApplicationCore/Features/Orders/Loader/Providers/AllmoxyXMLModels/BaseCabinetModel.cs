using System.Xml.Serialization;

namespace ApplicationCore.Features.Orders.Loader.Providers.AllmoxyXMLModels;

public class BaseCabinetModel {

	[XmlElement("materials")]
	public CabinetMaterialsModel Materials { get; set; } = new();

	[XmlElement("doorType")]
	public string DoorType { get; set; } = string.Empty;

    [XmlElement("doorStyle")]
    public string DoorStyle { get; set; } = string.Empty;

    [XmlElement("toeType")]
	public string ToeType { get; set; } = string.Empty;

	[XmlElement("assembled")]
	public string Assembled { get; set; } = string.Empty;

	[XmlElement("qty")]
	public int Qty { get; set; }

	[XmlElement("unitPrice")]
	public decimal UnitPrice { get; set; }

	[XmlElement("room")]
	public string Room { get; set; } = string.Empty;

	[XmlElement("width")]
	public double Width { get; set; }

	[XmlElement("height")]
	public double Height { get; set; }

	[XmlElement("depth")]
	public double Depth { get; set;  }

	[XmlElement("hingeSide")]
	public string HingeSide { get; set; } = string.Empty;

	[XmlElement("doorQty")]
	public int DoorQty { get; set; }

	[XmlElement("drawerQty")]
	public int DrawerQty { get; set; }

	[XmlElement("drawerFaceHeight")]
	public double DrawerFaceHeight { get; set; }

	[XmlElement("leftSide")]
	public string LeftSide { get; set; } = string.Empty;

	[XmlElement("rightSide")]
	public string RightSide { get; set; } = string.Empty;

	[XmlElement("adjShelfQty")]
	public int AdjShelfQty { get; set; }

	[XmlElement("vertDivQty")]
	public int VerticalDividerQty { get; set; }

	[XmlElement("rollOutPos1")]
	public string RollOutPos1 { get; set; } = string.Empty;

	[XmlElement("rollOutPos2")]
	public string RollOutPos2 { get; set; } = string.Empty;

	[XmlElement("rollOutPos3")]
	public string RollOutPos3 { get; set; } = string.Empty;

	[XmlElement("rollOutBlocks")]
	public string RollOutBlocks { get; set; } = string.Empty;

}
