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

    [XmlElement("basePieCutCabinet")]
    public List<BasePieCutCornerCabinetModel> BasePieCutCornerCabinets { get; set; } = new();

    [XmlElement("wallPieCutCabinet")]
    public List<WallPieCutCornerCabinetModel> WallPieCutCornerCabinets { get; set; } = new();

    [XmlElement("baseDiagonalCabinet")]
    public List<BaseDiagonalCornerCabinetModel> BaseDiagonalCornerCabinets { get; set; } = new();

    [XmlElement("wallDiagonalCabinet")]
    public List<WallDiagonalCornerCabinetModel> WallDiagonalCornerCabinets { get; set; } = new();

    [XmlElement("sinkCabinet")]
    public List<SinkCabinetModel> SinkCabinets { get; set; } = new();

    [XmlElement("blindBaseCabinet")]
    public List<BlindBaseCabinetModel> BlindBaseCabinets { get; set; } = new();

    [XmlElement("blindWallCabinet")]
    public List<BlindWallCabinetModel> BlindWallCabinets { get; set; } = new();

    /*[XmlArrayItem(typeof(DrawerBoxModel), ElementName="drawerBox")]
	public List<DrawerBoxModel> DrawerBoxes { get; set; } = new();*/

}
