using Domain.Orders.Builders;
using Domain.Orders.Entities;
using Domain.Orders.Entities.Products.Cabinets;
using Domain.Orders.ValueObjects;
using Domain.Orders.Entities.Products;
using Domain.ValueObjects;
using OneOf;
using System.Xml.Serialization;

namespace OrderLoading.LoadAllmoxyOrderData.AllmoxyXMLModels.Cabinets;

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

    [XmlAttribute("isGarage")]
    public bool IsGarage { get; set; } = false;

    public override OneOf<IProduct, AdditionalItem> CreateProductOrItem(ProductBuilderFactory builderFactory) {

        Dimension doorExtendDown = Dimension.FromMillimeters(ExtendDoorDown);
        WallCabinetDoors doors = DoorQty switch {
            0 => WallCabinetDoors.NoDoors(),
            1 => new(AllmoxyXMLOrderProviderHelpers.GetHingeSide(HingeSide), doorExtendDown),
            2 => new(Domain.Orders.Enums.HingeSide.NotApplicable, doorExtendDown),
            _ => new(AllmoxyXMLOrderProviderHelpers.GetHingeSide(HingeSide), doorExtendDown)
        };

        WallCabinetInside inside = new(AdjShelfQty, VerticalDividerQty);
        bool finishBottom = FinishedBottom == AllmoxyXMLOrderProviderHelpers.XML_BOOL_TRUE;

        var builder = builderFactory.CreateWallCabinetBuilder();

        return InitializeBuilder<WallCabinetBuilder, WallCabinet>(builder)
                    .WithDoors(doors)
                    .WithInside(inside)
                    .WithFinishBottom(finishBottom)
                    .WithIsGarage(IsGarage)
                    .Build();

    }

}
