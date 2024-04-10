using Domain.Orders.Builders;
using Domain.Orders.Entities;
using Domain.Orders.Entities.Products.Cabinets;
using Domain.Orders.ValueObjects;
using Domain.Orders.Entities.Products;
using Domain.ValueObjects;
using OneOf;
using System.Xml.Serialization;

namespace OrderLoading.LoadAllmoxyOrderData.AllmoxyXMLModels;

public class DrawerBaseCabinetModel : CabinetModelBase {

	[XmlElement("toeType")]
	public string ToeType { get; set; } = string.Empty;

	[XmlElement("drawerMaterial")]
	public string DrawerMaterial { get; set; } = string.Empty;

	[XmlElement("drawerSlide")]
	public string DrawerSlide { get; set; } = string.Empty;

	[XmlElement("drawerQty")]
	public int DrawerQty { get; set; }

	[XmlElement("drawerFace1")]
	public double DrawerFace1 { get; set; }

	[XmlElement("drawerFace2")]
	public double DrawerFace2 { get; set; }

	[XmlElement("drawerFace3")]
	public double DrawerFace3 { get; set; }

	[XmlElement("drawerFace4")]
	public double DrawerFace4 { get; set; }

	[XmlElement("drawerFace5")]
	public double DrawerFace5 { get; set; }

	[XmlAttribute("isGarage")]
	public bool IsGarage { get; set; } = false;

	public override OneOf<IProduct, AdditionalItem> CreateProductOrItem(ProductBuilderFactory builderFactory) {

		var drawerFaces = new Dimension[DrawerQty];
		if (DrawerQty >= 1) drawerFaces[0] = Dimension.FromMillimeters(DrawerFace1); // For 1 drawer box cabinets, the drawer box size is calculated
		if (DrawerQty >= 2) drawerFaces[1] = Dimension.FromMillimeters(DrawerFace2);
		if (DrawerQty >= 3) drawerFaces[2] = Dimension.FromMillimeters(DrawerFace3);
		if (DrawerQty >= 4) drawerFaces[3] = Dimension.FromMillimeters(DrawerFace4);
		if (DrawerQty >= 5) drawerFaces[4] = Dimension.FromMillimeters(DrawerFace5);

		VerticalDrawerBank verticalDrawerBank = new() {
			FaceHeights = drawerFaces
		};

		var boxOptions = new CabinetDrawerBoxOptions(AllmoxyXMLOrderProviderHelpers.GetDrawerMaterial(DrawerMaterial), AllmoxyXMLOrderProviderHelpers.GetDrawerSlideType(DrawerSlide));

		var builder = builderFactory.CreateDrawerBaseCabinetBuilder();

		return InitializeBuilder<DrawerBaseCabinetBuilder, DrawerBaseCabinet>(builder)
					.WithToeType(AllmoxyXMLOrderProviderHelpers.GetToeType(ToeType))
					.WithDrawers(verticalDrawerBank)
					.WithBoxOptions(boxOptions)
					.WithIsGarage(IsGarage)
					.Build();
	}

}