using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Shared.Domain;
using System.Xml.Serialization;

namespace ApplicationCore.Features.Orders.Loader.Providers.AllmoxyXMLModels;

public class BlindWallCabinetModel : CabinetModelBase {

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

    [XmlElement("extendDoorDown")]
    public double ExtendDoorDown { get; set; }


    public override IProduct CreateProduct(ProductBuilderFactory builderFactory) {

        BlindCabinetDoors doors = DoorQty switch {
            1 => new(AllmoxyXMLOrderProviderHelpers.GetHingeSide(HingeSide)),
            2 => new(Shared.Domain.Enums.HingeSide.NotApplicable),
            _ => new(AllmoxyXMLOrderProviderHelpers.GetHingeSide(HingeSide))
        };

        var builder = builderFactory.CreateBlindWallCabinetBuilder();

        return InitializeBuilder<BlindWallCabinetBuilder, BlindWallCabinet>(builder)
                    .WithDoors(doors)
                    .WithAdjustableShelves(AdjShelfQty)
                    .WithBlindSide(AllmoxyXMLOrderProviderHelpers.GetBlindSide(BlindSide))
                    .WithBlindWidth(Dimension.FromMillimeters(BlindWidth))
                    .WithExtendedDoor(Dimension.FromMillimeters(ExtendDoorDown))
                    .Build();

    }

}
