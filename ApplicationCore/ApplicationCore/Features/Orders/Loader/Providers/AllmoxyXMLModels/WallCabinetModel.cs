using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Shared.Domain;
using System.Xml.Serialization;

namespace ApplicationCore.Features.Orders.Loader.Providers.AllmoxyXMLModels;

public class WallCabinetModel : CabinetModelBase {

    [XmlElement("hingeSide")]
    public string HingeSide { get; set; } = string.Empty;

    [XmlElement("doorQty")]
    public int DoorQty { get; set; }

    [XmlElement("adjShelfQty")]
    public int AdjShelfQty { get; set; }

    [XmlElement("vertDivQty")]
    public int VerticalDividerQty { get; set; }

    [XmlElement("extendDoorDown")]
    public double ExtendDoorDown { get; set; }

    [XmlElement("finishedBottom")]
    public string FinishedBottom { get; set; } = string.Empty;

    public override IProduct CreateProduct(ProductBuilderFactory builderFactory) {

        Dimension doorExtendDown = Dimension.FromMillimeters(ExtendDoorDown);
        WallCabinetDoors doors = DoorQty switch {
            0 => WallCabinetDoors.NoDoors(),
            1 => new(AllmoxyXMLOrderProviderHelpers.GetHingeSide(HingeSide), doorExtendDown),
            2 => new(Shared.Domain.Enums.HingeSide.NotApplicable, doorExtendDown),
            _ => new(AllmoxyXMLOrderProviderHelpers.GetHingeSide(HingeSide), doorExtendDown)
        };

        WallCabinetInside inside = new(AdjShelfQty, VerticalDividerQty);
        bool finishBottom = (FinishedBottom == AllmoxyXMLOrderProviderHelpers.XML_BOOL_TRUE);

        var builder = builderFactory.CreateWallCabinetBuilder();

        return InitializeBuilder<WallCabinetBuilder, WallCabinet>(builder)
                    .WithDoors(doors)
                    .WithInside(inside)
                    .WithFinishBottom(finishBottom)
                    .Build();

    }

}
