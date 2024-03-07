using Domain.Orders.Builders;
using Domain.Orders.Entities;
using Domain.Orders.Entities.Products.Cabinets;
using Domain.Orders.ValueObjects;
using Domain.Orders.Entities.Products;
using Domain.ValueObjects;
using OneOf;
using System.Xml.Serialization;

namespace OrderLoading.LoadAllmoxyOrderData.AllmoxyXMLModels;

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

    [XmlAttribute("isGarage")]
    public bool IsGarage { get; set; } = false;

    public override OneOf<IProduct, AdditionalItem> CreateProductOrItem(ProductBuilderFactory builderFactory) {

        BlindCabinetDoors doors = DoorQty switch {
            1 => new(AllmoxyXMLOrderProviderHelpers.GetHingeSide(HingeSide)),
            2 => new(Domain.Orders.Enums.HingeSide.NotApplicable),
            _ => new(AllmoxyXMLOrderProviderHelpers.GetHingeSide(HingeSide))
        };

        var builder = builderFactory.CreateBlindWallCabinetBuilder();

        return InitializeBuilder<BlindWallCabinetBuilder, BlindWallCabinet>(builder)
                    .WithDoors(doors)
                    .WithAdjustableShelves(AdjShelfQty)
                    .WithBlindSide(AllmoxyXMLOrderProviderHelpers.GetBlindSide(BlindSide))
                    .WithBlindWidth(Dimension.FromMillimeters(BlindWidth))
                    .WithExtendedDoor(Dimension.FromMillimeters(ExtendDoorDown))
                    .WithIsGarage(IsGarage)
                    .Build();

    }

}
