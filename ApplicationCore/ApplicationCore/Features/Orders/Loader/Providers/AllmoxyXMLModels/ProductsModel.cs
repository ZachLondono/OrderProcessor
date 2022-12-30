using System.Xml.Serialization;

namespace ApplicationCore.Features.Orders.Loader.Providers.AllmoxyXMLModels;

public class ProductsModel {

	[XmlElement("baseCabinet")]
	public BaseCabinetModel[] BaseCabinets { get; set; } = Array.Empty<BaseCabinetModel>();

	/*[XmlArrayItem(typeof(DrawerBoxModel), ElementName="drawerBox")]
	public List<DrawerBoxModel> DrawerBoxes { get; set; } = new();*/

}
