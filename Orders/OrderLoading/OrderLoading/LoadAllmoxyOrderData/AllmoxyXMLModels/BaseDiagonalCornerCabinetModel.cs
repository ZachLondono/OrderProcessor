using Domain.Orders.Builders;
using Domain.Orders.Entities;
using Domain.Orders.Entities.Products.Cabinets;
using Domain.Orders.Entities.Products;
using Domain.ValueObjects;
using OneOf;
using System.Xml.Serialization;

namespace OrderLoading.LoadAllmoxyOrderData.AllmoxyXMLModels;

public class BaseDiagonalCornerCabinetModel : CabinetModelBase {

	[XmlElement("rightWidth")]
	public double RightWidth { get; set; }

	[XmlElement("rightDepth")]
	public double RightDepth { get; set; }

	[XmlElement("toeType")]
	public string ToeType { get; set; } = string.Empty;

	[XmlElement("hingeSide")]
	public string HingeSide { get; set; } = string.Empty;

	[XmlElement("doorQty")]
	public int DoorQty { get; set; }

	[XmlElement("adjShelfQty")]
	public int AdjShelfQty { get; set; }

	[XmlAttribute("isGarage")]
	public bool IsGarage { get; set; } = false;

	public override OneOf<IProduct, AdditionalItem> CreateProductOrItem(ProductBuilderFactory builderFactory) {

		var builder = builderFactory.CreateBaseDiagonalCornerCabinetBuilder();

		return InitializeBuilder<BaseDiagonalCornerCabinetBuilder, BaseDiagonalCornerCabinet>(builder)
					.WithRightWidth(Dimension.FromMillimeters(RightWidth))
					.WithRightDepth(Dimension.FromMillimeters(RightDepth))
					.WithToeType(AllmoxyXMLOrderProviderHelpers.GetToeType(ToeType))
					.WithAdjustableShelves(AdjShelfQty)
					.WithHingeSide(AllmoxyXMLOrderProviderHelpers.GetHingeSide(HingeSide))
					.WithDoorQty(DoorQty)
					.WithIsGarage(IsGarage)
					.Build();

	}
}
