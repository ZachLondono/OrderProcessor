using Domain.Orders.Builders;
using Domain.Orders.Entities;
using Domain.Orders.Enums;
using Domain.Orders.Entities.Products.Cabinets;
using Domain.Orders.ValueObjects;
using Domain.Orders.Entities.Products;
using Domain.ValueObjects;
using OneOf;
using System.Xml.Serialization;

namespace OrderLoading.LoadAllmoxyOrderData.AllmoxyXMLModels.Cabinets;

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

    [XmlElement("bottomNotchHeight")]
    public double BottomNotchHeight { get; set; }

    [XmlElement("bottomNotchDepth")]
    public double BottomNotchDepth { get; set; }

    [XmlAttribute("isGarage")]
    public bool IsGarage { get; set; } = false;

    public override OneOf<IProduct, AdditionalItem> CreateProductOrItem(ProductBuilderFactory builderFactory) {

        BaseCabinetDoors doors = DoorQty switch {
            0 => BaseCabinetDoors.NoDoors(),
            1 => new(AllmoxyXMLOrderProviderHelpers.GetHingeSide(HingeSide)),
            2 => new(),
            _ => new(AllmoxyXMLOrderProviderHelpers.GetHingeSide(HingeSide))
        };

        HorizontalDrawerBank drawers = new() {
            FaceHeight = DrawerQty == 0 ? Dimension.Zero : Dimension.FromMillimeters(DrawerFaceHeight),
            Quantity = DrawerQty
        };

        var boxOptions = new CabinetDrawerBoxOptions(AllmoxyXMLOrderProviderHelpers.GetDrawerMaterial(DrawerMaterial), AllmoxyXMLOrderProviderHelpers.GetDrawerSlideType(DrawerSlide));

        BaseCabinetInside inside;
        Dimension[] rollOutBoxPositions = AllmoxyXMLOrderProviderHelpers.GetRollOutPositions(RollOuts.Pos1, RollOuts.Pos2, RollOuts.Pos3, RollOuts.Pos4, RollOuts.Pos5);
        RollOutBlockPosition rollOutBlocks = AllmoxyXMLOrderProviderHelpers.GetRollOutBlockPositions(RollOuts.Blocks);
        if (rollOutBoxPositions.Length != 0) {
            var rollOutOptions = new RollOutOptions(rollOutBoxPositions, true, rollOutBlocks);
            inside = new(AdjShelfQty, rollOutOptions, AllmoxyXMLOrderProviderHelpers.GetShelfDepth(ShelfDepth));
        } else inside = new(AdjShelfQty, VerticalDividerQty, AllmoxyXMLOrderProviderHelpers.GetShelfDepth(ShelfDepth));

        var toeType = AllmoxyXMLOrderProviderHelpers.GetToeType(ToeType);

        CabinetBaseNotch? notch = null;
        if (BottomNotchHeight != 0 && BottomNotchDepth != 0) {
            var height = Dimension.FromMillimeters(BottomNotchHeight);
            var depth = Dimension.FromMillimeters(BottomNotchDepth);
            notch = new(height, depth);
        }

        var builder = builderFactory.CreateBaseCabinetBuilder();

        return InitializeBuilder<BaseCabinetBuilder, BaseCabinet>(builder)
                    .WithInside(inside)
                    .WithToeType(toeType)
                    .WithDoors(doors)
                    .WithDrawers(drawers)
                    .WithBoxOptions(boxOptions)
                    .WithIsGarage(IsGarage)
                    .WithBaseNotch(notch)
                    .Build();

    }
}