using ApplicationCore.Features.Orders.OrderLoading.LoadAllmoxyOrderData;
using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.Products.Cabinets;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Shared.Domain;
using System.Xml.Serialization;

namespace ApplicationCore.Features.Orders.OrderLoading.LoadAllmoxyOrderData.AllmoxyXMLModels;

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

    public override IProduct CreateProduct(ProductBuilderFactory builderFactory) {

        TallCabinetInside inside;
        Dimension[] rollOutBoxPositions = AllmoxyXMLOrderProviderHelpers.GetRollOutPositions(RollOuts.Pos1, RollOuts.Pos2, RollOuts.Pos3, RollOuts.Pos4, RollOuts.Pos5);
        RollOutBlockPosition rollOutBlocks = AllmoxyXMLOrderProviderHelpers.GetRollOutBlockPositions(RollOuts.Blocks);
        if (rollOutBoxPositions.Length != 0) {
            var rollOutOptions = new RollOutOptions(rollOutBoxPositions, true, rollOutBlocks);
            inside = new(UpperAdjShelfQty, LowerAdjShelfQty, UpperVerticalDividerQty, rollOutOptions);
        } else inside = new(UpperAdjShelfQty, LowerAdjShelfQty, UpperVerticalDividerQty, LowerVerticalDividerQty);

        TallCabinetDoors doors;
        HingeSide hingeSide = LowerDoorQty == 1 ? AllmoxyXMLOrderProviderHelpers.GetHingeSide(HingeSide) : Shared.Domain.Enums.HingeSide.NotApplicable;
        if (LowerDoorQty == 0) {
            doors = TallCabinetDoors.NoDoors();
        } else if (UpperDoorQty != 0) {
            doors = new(Dimension.FromMillimeters(LowerDoorHeight), hingeSide);
        } else {
            doors = new(hingeSide);
        }

        var boxOptions = new CabinetDrawerBoxOptions(AllmoxyXMLOrderProviderHelpers.GetDrawerMaterial(DrawerMaterial), AllmoxyXMLOrderProviderHelpers.GetDrawerSlideType(DrawerSlide));

        var builder = builderFactory.CreateTallCabinetBuilder();

        return InitializeBuilder<TallCabinetBuilder, TallCabinet>(builder)
                    .WithDoors(doors)
                    .WithToeType(AllmoxyXMLOrderProviderHelpers.GetToeType(ToeType))
                    .WithInside(inside)
                    .WithBoxOptions(boxOptions)
                    .Build();

    }

}
