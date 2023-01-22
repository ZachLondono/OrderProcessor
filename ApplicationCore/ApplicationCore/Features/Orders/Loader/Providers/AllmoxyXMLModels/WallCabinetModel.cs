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
        
        MDFDoorOptions? mdfOptions = null;
        if (Cabinet.Fronts.Type != "Slab") mdfOptions = new(Cabinet.Fronts.Style, Cabinet.Fronts.Color);

        bool hingeLeft = (HingeSide == "Left");
        Dimension doorExtendDown = Dimension.FromMillimeters(ExtendDoorDown);
        WallCabinetDoors doors = DoorQty switch {
            0 => WallCabinetDoors.NoDoors(),
            1 => new(hingeLeft ? Shared.Domain.Enums.HingeSide.Left : Shared.Domain.Enums.HingeSide.Right, doorExtendDown, mdfOptions),
            2 => new(doorExtendDown, mdfOptions),
            _ => new(hingeLeft ? Shared.Domain.Enums.HingeSide.Left : Shared.Domain.Enums.HingeSide.Right, doorExtendDown, mdfOptions)
        };

        WallCabinetInside inside = new(AdjShelfQty, VerticalDividerQty);
        bool finishBottom = (FinishedBottom == "Yes");

        var builder = builderFactory.CreateWallCabinetBuilder();

        return AllmoxyXMLOrderProviderHelpers.InitilizeBuilder<WallCabinetBuilder, WallCabinet>(builder, this)
                    .WithDoors(doors)
                    .WithInside(inside)
                    .WithFinishBottom(finishBottom)
                    .Build();

    }

}
