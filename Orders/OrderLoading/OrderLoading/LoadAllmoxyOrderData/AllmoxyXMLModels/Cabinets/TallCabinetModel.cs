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

public class TallCabinetModel : CabinetModelBase {

    [XmlElement("toeType")]
    public string ToeType { get; set; } = string.Empty;

    [XmlElement("hingeSide")]
    public string HingeSide { get; set; } = string.Empty;

    [XmlElement("upperDoorQty")]
    public int UpperDoorQty { get; set; }

    [XmlElement("lowerDoorQty")]
    public int LowerDoorQty { get; set; }

    [XmlElement("lowerDoorHeight")]
    public double LowerDoorHeight { get; set; }

    [XmlElement("drawerMaterial")]
    public string DrawerMaterial { get; set; } = string.Empty;

    [XmlElement("drawerSlide")]
    public string DrawerSlide { get; set; } = string.Empty;

    [XmlElement("upperAdjShelfQty")]
    public int UpperAdjShelfQty { get; set; }

    [XmlElement("adjShelfQty")]
    public int LowerAdjShelfQty { get; set; }

    [XmlElement("upperVertDivQty")]
    public int UpperVerticalDividerQty { get; set; }

    [XmlElement("vertDivQty")]
    public int LowerVerticalDividerQty { get; set; }

    [XmlElement("rollOuts")]
    public RollOuts RollOuts { get; set; } = new();

    [XmlElement("bottomNotchHeight")]
    public double BottomNotchHeight { get; set; }

    [XmlElement("bottomNotchDepth")]
    public double BottomNotchDepth { get; set; }

    [XmlAttribute("isGarage")]
    public bool IsGarage { get; set; } = false;

    public override OneOf<IProduct, AdditionalItem> CreateProductOrItem(ProductBuilderFactory builderFactory) {

        TallCabinetInside inside;
        Dimension[] rollOutBoxPositions = AllmoxyXMLOrderProviderHelpers.GetRollOutPositions(RollOuts.Pos1, RollOuts.Pos2, RollOuts.Pos3, RollOuts.Pos4, RollOuts.Pos5);
        RollOutBlockPosition rollOutBlocks = AllmoxyXMLOrderProviderHelpers.GetRollOutBlockPositions(RollOuts.Blocks);
        if (rollOutBoxPositions.Length != 0) {
            var rollOutOptions = new RollOutOptions(rollOutBoxPositions, true, rollOutBlocks);
            inside = new(UpperAdjShelfQty, LowerAdjShelfQty, UpperVerticalDividerQty, rollOutOptions);
        } else inside = new(UpperAdjShelfQty, LowerAdjShelfQty, UpperVerticalDividerQty, LowerVerticalDividerQty);

        TallCabinetDoors doors;
        HingeSide hingeSide = LowerDoorQty == 1 ? AllmoxyXMLOrderProviderHelpers.GetHingeSide(HingeSide) : Domain.Orders.Enums.HingeSide.NotApplicable;
        if (LowerDoorQty == 0) {
            doors = TallCabinetDoors.NoDoors();
        } else if (UpperDoorQty != 0) {
            doors = new(Dimension.FromMillimeters(LowerDoorHeight), hingeSide);
        } else {
            doors = new(hingeSide);
        }

        CabinetDrawerBoxOptions? boxOptions = null;
        if (DrawerMaterial != AllmoxyXMLOrderProviderHelpers.DRAWER_BOXES_NOT_INCLUDED) {
            boxOptions = new CabinetDrawerBoxOptions(AllmoxyXMLOrderProviderHelpers.GetDrawerMaterial(DrawerMaterial), AllmoxyXMLOrderProviderHelpers.GetDrawerSlideType(DrawerSlide));
        }

        CabinetBaseNotch? notch = null;
        if (BottomNotchHeight != 0 && BottomNotchDepth != 0) {
            var height = Dimension.FromMillimeters(BottomNotchHeight);
            var depth = Dimension.FromMillimeters(BottomNotchDepth);
            notch = new(height, depth);
        }

        var builder = builderFactory.CreateTallCabinetBuilder();

        return InitializeBuilder<TallCabinetBuilder, TallCabinet>(builder)
                    .WithDoors(doors)
                    .WithToeType(AllmoxyXMLOrderProviderHelpers.GetToeType(ToeType))
                    .WithInside(inside)
                    .WithBoxOptions(boxOptions)
                    .WithIsGarage(IsGarage)
                    .WithBaseNotch(notch)
                    .Build();

    }

}
