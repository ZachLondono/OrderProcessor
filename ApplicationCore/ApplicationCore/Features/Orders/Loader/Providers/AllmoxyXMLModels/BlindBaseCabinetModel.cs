using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Shared.Domain;
using System.Xml.Serialization;

namespace ApplicationCore.Features.Orders.Loader.Providers.AllmoxyXMLModels;

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

    public override IProduct CreateProduct(ProductBuilderFactory builderFactory) {

        MDFDoorOptions? mdfOptions = null;
        if (Cabinet.Fronts.Type != "Slab") mdfOptions = new(Cabinet.Fronts.Style, Cabinet.Fronts.Color);

        bool hingeLeft = (HingeSide == "Left");
        BlindCabinetDoors doors = DoorQty switch {
            1 => new(hingeLeft ? Shared.Domain.Enums.HingeSide.Left : Shared.Domain.Enums.HingeSide.Right, mdfOptions),
            2 => new(mdfOptions),
            _ => new(hingeLeft ? Shared.Domain.Enums.HingeSide.Left : Shared.Domain.Enums.HingeSide.Right, mdfOptions)
        };

        HorizontalDrawerBank drawers = new() {
            BoxMaterial = AllmoxyXMLOrderProviderHelpers.GetDrawerMaterial(DrawerMaterial),
            FaceHeight = Dimension.FromMillimeters(DrawerFaceHeight),
            Quantity = DrawerQty,
            SlideType = AllmoxyXMLOrderProviderHelpers.GetDrawerSlideType(DrawerSlide)
        };

        var blindSide = (BlindSide == "Left" ? Shared.Domain.Enums.BlindSide.Left : Shared.Domain.Enums.BlindSide.Right);

        var shelfDepth = AllmoxyXMLOrderProviderHelpers.GetShelfDepth(ShelfDepth);

        var builder = builderFactory.CreateBlindBaseCabinetBuilder();

        return InitilizeBuilder<BlindBaseCabinetBuilder, BlindBaseCabinet>(builder)
                .WithBlindSide(blindSide)
                .WithBlindWidth(Dimension.FromMillimeters(BlindWidth))
                .WithAdjustableShelves(AdjShelfQty)
                .WithDrawers(drawers)
                .WithToeType(AllmoxyXMLOrderProviderHelpers.GetToeType(ToeType))
                .WithDoors(doors)
                .Build();

    }

}