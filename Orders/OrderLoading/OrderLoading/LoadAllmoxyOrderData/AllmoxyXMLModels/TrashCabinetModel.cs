using OrderLoading.LoadAllmoxyOrderData;
using Domain.Orders.Builders;
using Domain.Orders.Entities;
using Domain.Orders.Enums;
using Domain.Orders.Entities.Products.Cabinets;
using Domain.Orders.ValueObjects;
using Domain.Orders.Entities.Products;
using Domain.ValueObjects;
using OneOf;
using System.Xml.Serialization;

namespace OrderLoading.LoadAllmoxyOrderData.AllmoxyXMLModels;

public class TrashCabinetModel : CabinetModelBase {

	[XmlElement("toeType")]
	public string ToeType { get; set; } = string.Empty;

	[XmlElement("drawerMaterial")]
	public string DrawerMaterial { get; set; } = string.Empty;

	[XmlElement("drawerSlide")]
	public string DrawerSlide { get; set; } = string.Empty;

	[XmlElement("drawerFaceHeight")]
	public double DrawerFaceHeight { get; set; }

	[XmlElement("trashPulloutQty")]
	public int TrashPulloutQty { get; set; }

	public override OneOf<IProduct, AdditionalItem> CreateProductOrItem(ProductBuilderFactory builderFactory) {

		var toeType = AllmoxyXMLOrderProviderHelpers.GetToeType(ToeType);
		var faceHeight = Dimension.FromMillimeters(DrawerFaceHeight);
		var trashConfig = TrashPulloutQty switch {
			1 => TrashPulloutConfiguration.OneCan,
			2 => TrashPulloutConfiguration.TwoCans,
			_ => throw new InvalidOperationException("Unrecognized trash pullout qty")
		};

		var boxOptions = new CabinetDrawerBoxOptions(AllmoxyXMLOrderProviderHelpers.GetDrawerMaterial(DrawerMaterial), AllmoxyXMLOrderProviderHelpers.GetDrawerSlideType(DrawerSlide));

		var builder = builderFactory.CreateTrashCabinetBuilder();

		return InitializeBuilder<TrashCabinetBuilder, TrashCabinet>(builder)
				.WithDrawerFaceHeight(faceHeight)
				.WithBoxOptions(boxOptions)
				.WithToeType(toeType)
				.WithTrashPulloutConfiguration(trashConfig)
				.Build();

	}

}