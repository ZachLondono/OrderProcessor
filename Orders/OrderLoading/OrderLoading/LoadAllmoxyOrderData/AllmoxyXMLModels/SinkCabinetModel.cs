using OrderLoading.LoadAllmoxyOrderData;
using Domain.Orders.Builders;
using Domain.Orders.Entities;
using Domain.Orders.Enums;
using Domain.Orders.Entities.Products.Cabinets;
using Domain.Orders.ValueObjects;
using Domain.Orders.Entities.Products;
using Domain.ValueObjects;
using OneOf;
using System.Xml.Serialization;

namespace OrderLoading.LoadAllmoxyOrderData.AllmoxyXMLModels;

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

    [XmlElement("tiltFront")]
    public string TiltFront { get; set; } = string.Empty;

    [XmlElement("scoopSides")]
    public string ScoopSides { get; set; } = string.Empty;

    [XmlElement("scoopDepth")]
    public double ScoopDepth { get; set; }

    [XmlElement("scoopFromFront")]
    public double ScoopFromFront { get; set; }

    [XmlElement("scoopFromBack")]
    public double ScoopFromBack { get; set; }

    public override OneOf<IProduct, AdditionalItem> CreateProductOrItem(ProductBuilderFactory builderFactory) {

        Dimension[] rollOutBoxPositions = AllmoxyXMLOrderProviderHelpers.GetRollOutPositions(RollOuts.Pos1, RollOuts.Pos2, RollOuts.Pos3, RollOuts.Pos4, RollOuts.Pos5);
        bool scoopFront = true;
        RollOutBlockPosition rollOutBlocks = AllmoxyXMLOrderProviderHelpers.GetRollOutBlockPositions(RollOuts.Blocks);

        var boxOptions = new CabinetDrawerBoxOptions(AllmoxyXMLOrderProviderHelpers.GetDrawerMaterial(DrawerMaterial), AllmoxyXMLOrderProviderHelpers.GetDrawerSlideType(DrawerSlide));

        var rollOutOptions = new RollOutOptions(rollOutBoxPositions, scoopFront, rollOutBlocks);

        var shelfDepth = AllmoxyXMLOrderProviderHelpers.GetShelfDepth(ShelfDepth);

        bool tiltFront = (TiltFront == AllmoxyXMLOrderProviderHelpers.XML_BOOL_TRUE);

        ScoopSides? scoops = null;
        if (ScoopSides == AllmoxyXMLOrderProviderHelpers.XML_BOOL_TRUE) {
            scoops = new(Dimension.FromMillimeters(ScoopDepth), Dimension.FromMillimeters(ScoopFromFront), Dimension.FromMillimeters(ScoopFromBack));
        }

        var builder = builderFactory.CreateSinkCabinetBuilder();

        return InitializeBuilder<SinkCabinetBuilder, SinkCabinet>(builder)
                    .WithRollOutBoxes(rollOutOptions)
                    .WithToeType(AllmoxyXMLOrderProviderHelpers.GetToeType(ToeType))
                    .WithHingeSide(AllmoxyXMLOrderProviderHelpers.GetHingeSide(HingeSide))
                    .WithDoorQty(DoorQty)
                    .WithFalseDrawerQty(DrawerQty)
                    .WithDrawerFaceHeight(Dimension.FromMillimeters(DrawerFaceHeight))
                    .WithAdjustableShelves(AdjShelfQty)
                    .WithShelfDepth(shelfDepth)
                    .WithBoxOptions(boxOptions)
                    .WithTiltFront(tiltFront)
                    .WithScoops(scoops)
                    .Build();
    }

}