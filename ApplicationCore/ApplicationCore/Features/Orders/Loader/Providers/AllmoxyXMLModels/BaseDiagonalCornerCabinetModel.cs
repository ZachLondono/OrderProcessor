using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Shared.Domain;
using System.Xml.Serialization;

namespace ApplicationCore.Features.Orders.Loader.Providers.AllmoxyXMLModels;

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

    public override IProduct CreateProduct(ProductBuilderFactory builderFactory) {

        var builder = builderFactory.CreateBaseDiagonalCornerCabinetBuilder();

        return InitializeBuilder<BaseDiagonalCornerCabinetBuilder, BaseDiagonalCornerCabinet>(builder)
                    .WithRightWidth(Dimension.FromMillimeters(RightWidth))
                    .WithRightDepth(Dimension.FromMillimeters(RightDepth))
                    .WithToeType(AllmoxyXMLOrderProviderHelpers.GetToeType(ToeType))
                    .WithAdjustableShelves(AdjShelfQty)
                    .WithHingeSide(AllmoxyXMLOrderProviderHelpers.GetHingeSide(HingeSide))
                    .WithDoorQty(DoorQty)
                    .Build();

    }
}
