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

    public override IProduct CreateProduct(ProductBuilderFactory builderFactory) {

        MDFDoorOptions? mdfOptions = null;
        if (Cabinet.Fronts.Type != "Slab") mdfOptions = new(Cabinet.Fronts.Style, Cabinet.Fronts.Color);

        bool hingeLeft = (HingeSide == "Left");
        BlindCabinetDoors doors = DoorQty switch {
            1 => new(hingeLeft ? Shared.Domain.Enums.HingeSide.Left : Shared.Domain.Enums.HingeSide.Right, mdfOptions),
            2 => new(mdfOptions),
            _ => new(hingeLeft ? Shared.Domain.Enums.HingeSide.Left : Shared.Domain.Enums.HingeSide.Right, mdfOptions)
        };

        var builder = builderFactory.CreateBlindWallCabinetBuilder();

        return InitilizeBuilder<BlindWallCabinetBuilder, BlindWallCabinet>(builder)
                    .WithDoors(doors)
                    .WithAdjustableShelves(AdjShelfQty)
                    .WithBlindSide(BlindSide == "Left" ? Shared.Domain.Enums.BlindSide.Left : Shared.Domain.Enums.BlindSide.Right)
                    .WithBlindWidth(Dimension.FromMillimeters(BlindWidth))
                    .Build();

    }

}
