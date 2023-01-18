using System.Xml.Serialization;

namespace ApplicationCore.Features.Orders.Loader.Providers.AllmoxyXMLModels;

public class ProductsModel {

    [XmlElement("baseCabinet")]
    public List<BaseCabinetModel> BaseCabinets { get; set; } = new();

    [XmlElement("drawerBaseCabinet")]
    public List<DrawerBaseCabinetModel> DrawerBaseCabinets { get; set; } = new();

    [XmlElement("wallCabinet")]
    public List<WallCabinetModel> WallCabinets { get; set; } = new();

    [XmlElement("tallCabinet")]
    public List<TallCabinetModel> TallCabinets { get; set; } = new();

    [XmlElement("pieCutCabinet")]
    public List<PieCutCornerCabinetModel> PieCutCornerCabinets { get; set; } = new();

    [XmlElement("baseDiagonalCabinet")]
    public List<BaseDiagonalCornerCabinetModel> BaseDiagonalCornerCabinets { get; set; } = new();

    [XmlElement("sinkCabinet")]
    public List<SinkCabinetModel> SinkCabinets { get; set; } = new();

    /*[XmlArrayItem(typeof(DrawerBoxModel), ElementName="drawerBox")]
	public List<DrawerBoxModel> DrawerBoxes { get; set; } = new();*/

}
