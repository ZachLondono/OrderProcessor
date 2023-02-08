using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Shared.Domain;
using System.Xml.Serialization;

namespace ApplicationCore.Features.Orders.Loader.Providers.AllmoxyXMLModels;

public class BaseCabinetModel : CabinetModelBase {

    [XmlElement("toeType")]
    public string ToeType { get; set; } = string.Empty;

    [XmlElement("hingeSide")]
    public string HingeSide { get; set; } = string.Empty;

    [XmlElement("doorQty")]
    public int DoorQty { get; set; }

    [XmlElement("drawerQty")]
    public int DrawerQty { get; set; }

    [XmlElement("drawerMaterial")]
    public string DrawerMaterial { get; set; } = string.Empty;

    [XmlElement("drawerSlide")]
    public string DrawerSlide { get; set; } = string.Empty;

    [XmlElement("drawerFaceHeight")]
    public double DrawerFaceHeight { get; set; }

    [XmlElement("adjShelfQty")]
    public int AdjShelfQty { get; set; }

    [XmlElement("vertDivQty")]
    public int VerticalDividerQty { get; set; }

    [XmlElement("shelfDepth")]
    public string ShelfDepth { get; set; } = string.Empty;

    [XmlElement("rollOuts")]
    public RollOuts RollOuts { get; set; } = new();

    public override IProduct CreateProduct(ProductBuilderFactory builderFactory) {

        bool hingeLeft = (HingeSide == "Left");
        BaseCabinetDoors doors = DoorQty switch {
            0 => BaseCabinetDoors.NoDoors(),
            1 => new(hingeLeft ? Shared.Domain.Enums.HingeSide.Left : Shared.Domain.Enums.HingeSide.Right),
            2 => new(),
            _ => new(hingeLeft ? Shared.Domain.Enums.HingeSide.Left : Shared.Domain.Enums.HingeSide.Right)
        };

        HorizontalDrawerBank drawers = new() {
            BoxMaterial = AllmoxyXMLOrderProviderHelpers.GetDrawerMaterial(DrawerMaterial),
            FaceHeight = DrawerQty == 0 ? Dimension.Zero : Dimension.FromMillimeters(DrawerFaceHeight),
            Quantity = DrawerQty,
            SlideType = AllmoxyXMLOrderProviderHelpers.GetDrawerSlideType(DrawerSlide)
        };

        BaseCabinetInside inside;
        Dimension[] rollOutBoxPositions = AllmoxyXMLOrderProviderHelpers.GetRollOutPositions(RollOuts.Pos1, RollOuts.Pos2, RollOuts.Pos3, RollOuts.Pos4, RollOuts.Pos5);
        RollOutBlockPosition rollOutBlocks = AllmoxyXMLOrderProviderHelpers.GetRollOutBlockPositions(RollOuts.Blocks);
        if (rollOutBoxPositions.Length != 0) {
            var rollOutOptions = new RollOutOptions(rollOutBoxPositions, true, rollOutBlocks, drawers.SlideType, drawers.BoxMaterial);
            inside = new(AdjShelfQty, rollOutOptions, AllmoxyXMLOrderProviderHelpers.GetShelfDepth(ShelfDepth));
        } else inside = new(AdjShelfQty, VerticalDividerQty, AllmoxyXMLOrderProviderHelpers.GetShelfDepth(ShelfDepth));

        var toeType = AllmoxyXMLOrderProviderHelpers.GetToeType(ToeType);

        var builder = builderFactory.CreateBaseCabinetBuilder();

        return InitilizeBuilder<BaseCabinetBuilder, BaseCabinet>(builder)
                    .WithInside(inside)
                    .WithToeType(toeType)
                    .WithDoors(doors)
                    .WithDrawers(drawers)
                    .Build();

    }
}