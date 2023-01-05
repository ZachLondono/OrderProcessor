using System.Xml.Serialization;

namespace ApplicationCore.Features.Orders.Loader.Providers.AllmoxyXMLModels;

public class CabinetModel {

	[XmlElement("boxMaterial")]
	public CabinetMaterialModel BoxMaterial { get; set; } = new();

	[XmlElement("finishMaterial")]
	public CabinetMaterialModel FinishMaterial { get; set; } = new();

    [XmlElement("fronts")]
    public CabinetFrontsModel Fronts { get; set; } = new();

	[XmlElement("edgeBandColor")]
	public string EdgeBandColor { get; set; } = string.Empty;

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
    public double Depth { get; set; }

    [XmlElement("leftSide")]
    public string LeftSide { get; set; } = string.Empty;

    [XmlElement("rightSide")]
    public string RightSide { get; set; } = string.Empty;

    [XmlElement("assembled")]
    public string Assembled { get; set; } = string.Empty;

}
