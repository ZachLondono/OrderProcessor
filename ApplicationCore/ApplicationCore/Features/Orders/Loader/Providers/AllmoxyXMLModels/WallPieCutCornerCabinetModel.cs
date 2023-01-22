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
        
        MDFDoorOptions? mdfOptions = null;
        if (Cabinet.Fronts.Type != "Slab") mdfOptions = new(Cabinet.Fronts.Style, Cabinet.Fronts.Color);

        var builder = builderFactory.CreateWallPieCutCornerCabinetBuilder();

        return AllmoxyXMLOrderProviderHelpers.InitilizeBuilder<WallPieCutCornerCabinetBuilder, WallPieCutCornerCabinet>(builder, this)
                    .WithRightWidth(Dimension.FromMillimeters(RightWidth))
                    .WithRightDepth(Dimension.FromMillimeters(RightDepth))
                    .WithAdjustableShelves(AdjShelfQty)
                    .WithHingeSide((HingeSide == "Left") ? Shared.Domain.Enums.HingeSide.Left : Shared.Domain.Enums.HingeSide.Right)
                    .WithMDFOptions(Cabinet.Fronts.Type == "Slab" ? null : mdfOptions)
                    .Build();

    }

}
