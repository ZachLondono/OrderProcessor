using Domain.Orders.Builders;
using Domain.Orders.Entities;
using Domain.Orders.Products.Cabinets;
using Domain.Orders.Entities.Products;
using Domain.ValueObjects;
using OneOf;
using System.Xml.Serialization;

namespace ApplicationCore.Features.Orders.OrderLoading.LoadAllmoxyOrderData.AllmoxyXMLModels;

public class WallDiagonalCornerCabinetModel : CabinetModelBase {

    [XmlElement("rightWidth")]
    public double RightWidth { get; set; }

    [XmlElement("rightDepth")]
    public double RightDepth { get; set; }

    [XmlElement("hingeSide")]
    public string HingeSide { get; set; } = string.Empty;

    [XmlElement("doorQty")]
    public int DoorQty { get; set; }

    [XmlElement("adjShelfQty")]
    public int AdjShelfQty { get; set; }

    [XmlElement("extendDoorDown")]
    public double ExtendDoorDown { get; set; }

    [XmlAttribute("isGarage")]
    public bool IsGarage { get; set; } = false;

    public override OneOf<IProduct, AdditionalItem> CreateProductOrItem(ProductBuilderFactory builderFactory) {

        var builder = builderFactory.CreateWallDiagonalCornerCabinetBuilder();

        return InitializeBuilder<WallDiagonalCornerCabinetBuilder, WallDiagonalCornerCabinet>(builder)
                    .WithRightWidth(Dimension.FromMillimeters(RightWidth))
                    .WithRightDepth(Dimension.FromMillimeters(RightDepth))
                    .WithAdjustableShelves(AdjShelfQty)
                    .WithHingeSide(AllmoxyXMLOrderProviderHelpers.GetHingeSide(HingeSide))
                    .WithDoorQty(DoorQty)
                    .WithExtendedDoor(Dimension.FromMillimeters(ExtendDoorDown))
                    .WithIsGarage(IsGarage)
                    .Build();
    }

}