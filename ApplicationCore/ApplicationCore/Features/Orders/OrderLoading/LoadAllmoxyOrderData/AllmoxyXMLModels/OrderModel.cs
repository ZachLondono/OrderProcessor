using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using System.Xml.Serialization;

namespace ApplicationCore.Features.Orders.OrderLoading.Providers.AllmoxyXMLModels;

[XmlRoot("order")]
public class OrderModel {

    [XmlElement("number")]
    public int Number { get; set; }

    [XmlElement("name")]
    public string Name { get; set; } = string.Empty;

    [XmlElement("date")]
    public string OrderDate { get; set; } = string.Empty;

    [XmlElement("note")]
    public string Note { get; set; } = string.Empty;

    [XmlElement("description")]
    public string Description { get; set; } = string.Empty;

    [XmlElement("customer")]
    public CustomerModel Customer { get; set; } = new();

    [XmlElement("invoice")]
    public InvoiceModel Invoice { get; set; } = new();

    [XmlElement("shipping")]
    public ShippingModel Shipping { get; set; } = new();

    [XmlArray("products")]
    [XmlArrayItem("baseCabinet", typeof(BaseCabinetModel))]
    [XmlArrayItem("drawerBaseCabinet", typeof(DrawerBaseCabinetModel))]
    [XmlArrayItem("wallCabinet", typeof(WallCabinetModel))]
    [XmlArrayItem("tallCabinet", typeof(TallCabinetModel))]
    [XmlArrayItem("basePieCutCabinet", typeof(BasePieCutCornerCabinetModel))]
    [XmlArrayItem("wallPieCutCabinet", typeof(WallPieCutCornerCabinetModel))]
    [XmlArrayItem("baseDiagonalCabinet", typeof(BaseDiagonalCornerCabinetModel))]
    [XmlArrayItem("wallDiagonalCabinet", typeof(WallDiagonalCornerCabinetModel))]
    [XmlArrayItem("sinkCabinet", typeof(SinkCabinetModel))]
    [XmlArrayItem("blindBaseCabinet", typeof(BlindBaseCabinetModel))]
    [XmlArrayItem("blindWallCabinet", typeof(BlindWallCabinetModel))]
    [XmlArrayItem("trashCabinet", typeof(TrashCabinetModel))]
    [XmlArrayItem("closetPart", typeof(ClosetPartModel))]
    [XmlArrayItem("mdfDoor", typeof(MDFDoorModel))]
    [XmlArrayItem("dovetailDrawerBox", typeof(DovetailDrawerBoxModel))]
    public List<ProductModel> Products { get; set; } = new();

}
