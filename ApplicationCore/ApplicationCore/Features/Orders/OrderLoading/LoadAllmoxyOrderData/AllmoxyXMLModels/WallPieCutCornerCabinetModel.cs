using ApplicationCore.Features.Orders.OrderLoading.LoadAllmoxyOrderData;
using Domain.Orders.Builders;
using Domain.Orders.Entities;
using Domain.Orders.Products.Cabinets;
using Domain.Orders.ValueObjects;
using Domain.Orders.Entities.Products;
using Domain.ValueObjects;
using OneOf;
using System.Xml.Serialization;

namespace ApplicationCore.Features.Orders.OrderLoading.LoadAllmoxyOrderData.AllmoxyXMLModels;

public class WallPieCutCornerCabinetModel : CabinetModelBase {

    [XmlElement("rightWidth")]
    public double RightWidth { get; set; }

    [XmlElement("rightDepth")]
    public double RightDepth { get; set; }

    [XmlElement("hingeSide")]
    public string HingeSide { get; set; } = string.Empty;

    [XmlElement("adjShelfQty")]
    public int AdjShelfQty { get; set; }

    [XmlElement("extendDoorDown")]
    public double ExtendDoorDown { get; set; }

    public override OneOf<IProduct, AdditionalItem> CreateProductOrItem(ProductBuilderFactory builderFactory) {

        var builder = builderFactory.CreateWallPieCutCornerCabinetBuilder();

        return InitializeBuilder<WallPieCutCornerCabinetBuilder, WallPieCutCornerCabinet>(builder)
                    .WithRightWidth(Dimension.FromMillimeters(RightWidth))
                    .WithRightDepth(Dimension.FromMillimeters(RightDepth))
                    .WithAdjustableShelves(AdjShelfQty)
                    .WithHingeSide(AllmoxyXMLOrderProviderHelpers.GetHingeSide(HingeSide))
                    .WithExtendedDoor(Dimension.FromMillimeters(ExtendDoorDown))
                    .Build();

    }

}
