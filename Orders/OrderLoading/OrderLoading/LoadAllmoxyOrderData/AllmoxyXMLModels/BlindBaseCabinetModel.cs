using OrderLoading.LoadAllmoxyOrderData;
using Domain.Orders.Builders;
using Domain.Orders.Entities;
using Domain.Orders.Entities.Products.Cabinets;
using Domain.Orders.ValueObjects;
using Domain.Orders.Entities.Products;
using Domain.ValueObjects;
using OneOf;
using System.Xml.Serialization;

namespace OrderLoading.LoadAllmoxyOrderData.AllmoxyXMLModels;

public class BlindBaseCabinetModel : CabinetModelBase {

    [XmlElement("toeType")]
    public string ToeType { get; set; } = string.Empty;

    [XmlElement("hingeSide")]
    public string HingeSide { get; set; } = string.Empty;

    [XmlElement("blindSide")]
    public string BlindSide { get; set; } = string.Empty;

    [XmlElement("blindWidth")]
    public double BlindWidth { get; set; }

    [XmlElement("doorQty")]
    public int DoorQty { get; set; }

    [XmlElement("adjShelfQty")]
    public int AdjShelfQty { get; set; }

    [XmlElement("shelfDepth")]
    public string ShelfDepth { get; set; } = string.Empty;

    [XmlElement("drawerQty")]
    public int DrawerQty { get; set; }

    [XmlElement("drawerMaterial")]
    public string DrawerMaterial { get; set; } = string.Empty;

    [XmlElement("drawerSlide")]
    public string DrawerSlide { get; set; } = string.Empty;

    [XmlElement("drawerFaceHeight")]
    public double DrawerFaceHeight { get; set; }

    public override OneOf<IProduct, AdditionalItem> CreateProductOrItem(ProductBuilderFactory builderFactory) {

        BlindCabinetDoors doors = DoorQty switch {
            1 => new(AllmoxyXMLOrderProviderHelpers.GetHingeSide(HingeSide)),
            2 => new(Domain.Orders.Enums.HingeSide.NotApplicable),
            _ => new(AllmoxyXMLOrderProviderHelpers.GetHingeSide(HingeSide))
        };

        HorizontalDrawerBank drawers = new() {
            FaceHeight = Dimension.FromMillimeters(DrawerFaceHeight),
            Quantity = DrawerQty
        };

        var boxOptions = new CabinetDrawerBoxOptions(AllmoxyXMLOrderProviderHelpers.GetDrawerMaterial(DrawerMaterial), AllmoxyXMLOrderProviderHelpers.GetDrawerSlideType(DrawerSlide));

        var builder = builderFactory.CreateBlindBaseCabinetBuilder();

        return InitializeBuilder<BlindBaseCabinetBuilder, BlindBaseCabinet>(builder)
                .WithBlindSide(AllmoxyXMLOrderProviderHelpers.GetBlindSide(BlindSide))
                .WithBlindWidth(Dimension.FromMillimeters(BlindWidth))
                .WithAdjustableShelves(AdjShelfQty)
                .WithDrawers(drawers)
                .WithToeType(AllmoxyXMLOrderProviderHelpers.GetToeType(ToeType))
                .WithDoors(doors)
                .WithShelfDepth(AllmoxyXMLOrderProviderHelpers.GetShelfDepth(ShelfDepth))
                .WithBoxOptions(boxOptions)
                .Build();

    }

}