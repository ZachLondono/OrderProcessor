using System.Xml.Serialization;

namespace ApplicationCore.Features.Orders.Loader.Providers.AllmoxyXMLModels;

public class ProductsModel {

	[XmlElement("baseCabinet")]
	public BaseCabinetModel[] BaseCabinets { get; set; } = Array.Empty<BaseCabinetModel>();

    [XmlElement("drawerBaseCabinet")]
    public DrawerBaseCabinetModel[] DrawerBaseCabinets { get; set; } = Array.Empty<DrawerBaseCabinetModel>();

    [XmlElement("wallCabinet")]
    public WallCabinetModel[] WallCabinets { get; set; } = Array.Empty<WallCabinetModel>();

    [XmlElement("tallCabinet")]
    public TallCabinetModel[] TallCabinets { get; set; } = Array.Empty<TallCabinetModel>();

    [XmlElement("pieCutCabinet")]
    public PieCutCornerCabinetModel[] PieCutCornerCabinets { get; set; } = Array.Empty<PieCutCornerCabinetModel>();

    [XmlElement("diagonalCabinet")]
    public DiagonalCornerCabinetModel[] DiagonalCornerCabinets { get; set; } = Array.Empty<DiagonalCornerCabinetModel>();

    [XmlElement("sinkCabinet")]
    public SinkCabinetModel[] SinkCabinets { get; set; } = Array.Empty<SinkCabinetModel>();

    /*[XmlArrayItem(typeof(DrawerBoxModel), ElementName="drawerBox")]
	public List<DrawerBoxModel> DrawerBoxes { get; set; } = new();*/

}
