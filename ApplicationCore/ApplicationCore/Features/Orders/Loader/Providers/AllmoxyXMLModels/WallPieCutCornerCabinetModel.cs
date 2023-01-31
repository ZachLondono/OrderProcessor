using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Shared.Domain;
using System.Xml.Serialization;

namespace ApplicationCore.Features.Orders.Loader.Providers.AllmoxyXMLModels;

public class WallPieCutCornerCabinetModel : CabinetModelBase {

    [XmlElement("rightWidth")]
    public double RightWidth { get; set; }

    [XmlElement("rightDepth")]
    public double RightDepth { get; set; }

    [XmlElement("hingeSide")]
    public string HingeSide { get; set; } = string.Empty;

    [XmlElement("adjShelfQty")]
    public int AdjShelfQty { get; set; }

    public override IProduct CreateProduct(ProductBuilderFactory builderFactory) {

        var builder = builderFactory.CreateWallPieCutCornerCabinetBuilder();

        return InitilizeBuilder<WallPieCutCornerCabinetBuilder, WallPieCutCornerCabinet>(builder)
                    .WithRightWidth(Dimension.FromMillimeters(RightWidth))
                    .WithRightDepth(Dimension.FromMillimeters(RightDepth))
                    .WithAdjustableShelves(AdjShelfQty)
                    .WithHingeSide((HingeSide == "Left") ? Shared.Domain.Enums.HingeSide.Left : Shared.Domain.Enums.HingeSide.Right)
                    .Build();

    }

}
