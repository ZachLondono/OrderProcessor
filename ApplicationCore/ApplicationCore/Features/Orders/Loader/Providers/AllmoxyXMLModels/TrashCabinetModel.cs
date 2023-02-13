using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Shared.Domain;
using System.Xml.Serialization;

namespace ApplicationCore.Features.Orders.Loader.Providers.AllmoxyXMLModels;

public class TrashCabinetModel : CabinetModelBase {

    [XmlElement("toeType")]
    public string ToeType { get; set; } = string.Empty;

    [XmlElement("drawerMaterial")]
    public string DrawerMaterial { get; set; } = string.Empty;

    [XmlElement("drawerSlide")]
    public string DrawerSlide { get; set; } = string.Empty;

    [XmlElement("drawerFaceHeight")]
    public double DrawerFaceHeight { get; set; }

    [XmlElement("trashPulloutQty")]
    public int TrashPulloutQty { get; set; }

    public override IProduct CreateProduct(ProductBuilderFactory builderFactory) {

        var toeType = AllmoxyXMLOrderProviderHelpers.GetToeType(ToeType);
        var faceHeight = Dimension.FromMillimeters(DrawerFaceHeight);
        var dbMaterial = AllmoxyXMLOrderProviderHelpers.GetDrawerMaterial(DrawerMaterial);
        var slideType = AllmoxyXMLOrderProviderHelpers.GetDrawerSlideType(DrawerSlide);
        var trashConfig = TrashPulloutQty switch {
            1 => TrashPulloutConfiguration.OneCan,
            2 => TrashPulloutConfiguration.TwoCans,
            _ => throw new InvalidOperationException("Unrecognized trash pullout qty")
        };

        var builder = builderFactory.CreateTrashCabinetBuilder();

        return InitilizeBuilder<TrashCabinetBuilder, TrashCabinet>(builder)
                .WithDrawerFaceHeight(faceHeight)
                .WithDrawerBoxMaterial(dbMaterial)
                .WithSlideType(slideType)
                .WithToeType(toeType)
                .WithTrashPulloutConfiguration(trashConfig)
                .Build();

    }

}