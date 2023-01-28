using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Shared.Domain;
using System.Xml.Serialization;

namespace ApplicationCore.Features.Orders.Loader.Providers.AllmoxyXMLModels;

public class SinkCabinetModel : CabinetModelBase {

    [XmlElement("toeType")]
    public string ToeType { get; set; } = string.Empty;

    [XmlElement("hingeSide")]
    public string HingeSide { get; set; } = string.Empty;

    [XmlElement("doorQty")]
    public int DoorQty { get; set; }

    [XmlElement("drawerQty")]
    public int DrawerQty { get; set; }

    [XmlElement("drawerFaceHeight")]
    public double DrawerFaceHeight { get; set; }

    [XmlElement("drawerMaterial")]
    public string DrawerMaterial { get; set; } = string.Empty;

    [XmlElement("drawerSlide")]
    public string DrawerSlide { get; set; } = string.Empty;

    [XmlElement("adjShelfQty")]
    public int AdjShelfQty { get; set; }

    [XmlElement("shelfDepth")]
    public string ShelfDepth { get; set; } = string.Empty;

    [XmlElement("rollOuts")]
    public RollOuts RollOuts { get; set; } = new();

    public override IProduct CreateProduct(ProductBuilderFactory builderFactory) {

        MDFDoorOptions? mdfOptions = null;
        if (Cabinet.Fronts.Type != "Slab") mdfOptions = new(Cabinet.Fronts.Style, Cabinet.Fronts.Color);

        Dimension[] rollOutBoxPositions = AllmoxyXMLOrderProviderHelpers.GetRollOutPositions(RollOuts.Pos1, RollOuts.Pos2, RollOuts.Pos3, RollOuts.Pos4, RollOuts.Pos5);
        bool scoopFront = true;
        RollOutBlockPosition rollOutBlocks = AllmoxyXMLOrderProviderHelpers.GetRollOutBlockPositions(RollOuts.Blocks);
        var slideType = AllmoxyXMLOrderProviderHelpers.GetDrawerSlideType(DrawerSlide);
        var material = AllmoxyXMLOrderProviderHelpers.GetDrawerMaterial(DrawerMaterial);

        var rollOutOptions = new RollOutOptions(rollOutBoxPositions, scoopFront, rollOutBlocks, slideType, material);

        var shelfDepth = AllmoxyXMLOrderProviderHelpers.GetShelfDepth(ShelfDepth);

        var builder = builderFactory.CreateSinkCabinetBuilder();

        return InitilizeBuilder<SinkCabinetBuilder, SinkCabinet>(builder)
                    .WithRollOutBoxes(rollOutOptions)
                    .WithToeType(AllmoxyXMLOrderProviderHelpers.GetToeType(ToeType))
                    .WithHingeSide((HingeSide == "Left") ? Shared.Domain.Enums.HingeSide.Left : Shared.Domain.Enums.HingeSide.Right)
                    .WithDoorQty(DoorQty)
                    .WithFalseDrawerQty(DrawerQty)
                    .WithDrawerFaceHeight(Dimension.FromMillimeters(DrawerFaceHeight))
                    .WithAdjustableShelves(AdjShelfQty)
                    .WithMDFOptions(Cabinet.Fronts.Type == "Slab" ? null : mdfOptions)
                    .WithShelfDepth(shelfDepth)
                    .Build();
    }

}