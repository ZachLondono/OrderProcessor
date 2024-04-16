using Domain.Orders.Builders;
using Domain.Orders.Entities;
using Domain.Orders.Entities.Products.Cabinets;
using Domain.Orders.ValueObjects;
using Domain.Orders.Entities.Products;
using Domain.ValueObjects;
using OneOf;
using System.Xml.Serialization;

namespace OrderLoading.LoadAllmoxyOrderData.AllmoxyXMLModels.Cabinets;

public class BasePieCutCornerCabinetModel : CabinetModelBase {

    [XmlElement("rightWidth")]
    public double RightWidth { get; set; }

    [XmlElement("rightDepth")]
    public double RightDepth { get; set; }

    [XmlElement("toeType")]
    public string ToeType { get; set; } = string.Empty;

    [XmlElement("hingeSide")]
    public string HingeSide { get; set; } = string.Empty;

    [XmlElement("adjShelfQty")]
    public int AdjShelfQty { get; set; }

    public override OneOf<IProduct, AdditionalItem> CreateProductOrItem(ProductBuilderFactory builderFactory) {

        var builder = builderFactory.CreateBasePieCutCornerCabinetBuilder();

        return InitializeBuilder<BasePieCutCornerCabinetBuilder, BasePieCutCornerCabinet>(builder)
                    .WithRightWidth(Dimension.FromMillimeters(RightWidth))
                    .WithRightDepth(Dimension.FromMillimeters(RightDepth))
                    .WithToeType(AllmoxyXMLOrderProviderHelpers.GetToeType(ToeType))
                    .WithAdjustableShelves(AdjShelfQty)
                    .WithHingeSide(AllmoxyXMLOrderProviderHelpers.GetHingeSide(HingeSide))
                    .Build();

    }

}