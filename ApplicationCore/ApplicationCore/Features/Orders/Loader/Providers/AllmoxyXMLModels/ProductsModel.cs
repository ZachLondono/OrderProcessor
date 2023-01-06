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

    /*[XmlArrayItem(typeof(DrawerBoxModel), ElementName="drawerBox")]
	public List<DrawerBoxModel> DrawerBoxes { get; set; } = new();*/

}
