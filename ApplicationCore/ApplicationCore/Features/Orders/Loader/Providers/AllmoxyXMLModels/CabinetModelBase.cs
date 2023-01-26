using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Shared.Domain;
using System.Xml.Serialization;

namespace ApplicationCore.Features.Orders.Loader.Providers.AllmoxyXMLModels;

public abstract class CabinetModelBase : ProductModel {

    [XmlAttribute("groupNumber")]
    public int GroupNumber { get; set; }

    [XmlAttribute("lineNumber")]
    public int LineNumber { get; set; }

    [XmlElement("cabinet")]
    public CabinetModel Cabinet { get; set; } = new();

    public TBuilder InitilizeBuilder<TBuilder, TCabinet>(TBuilder builder) where TBuilder : CabinetBuilder<TCabinet> where TCabinet : Cabinet {

        CabinetMaterialCore boxCore = AllmoxyXMLOrderProviderHelpers.GetMaterialCore(Cabinet.BoxMaterial.Type);
        CabinetMaterialCore finishCore = AllmoxyXMLOrderProviderHelpers.GetFinishedSideMaterialCore(Cabinet.FinishMaterial.Type, boxCore);

        MDFDoorOptions? mdfOptions = null;
        if (Cabinet.Fronts.Type != "Slab") mdfOptions = new(Cabinet.Fronts.Style, Cabinet.Fronts.Color);

        string finishColor = (Cabinet.FinishMaterial.Type == "paint" ? Cabinet.BoxMaterial.Finish : Cabinet.FinishMaterial.Finish);
        string? finishPaintColor = (Cabinet.FinishMaterial.Type == "paint" ? Cabinet.FinishMaterial.Finish : null);
        CabinetMaterial boxMaterial = new(Cabinet.BoxMaterial.Finish, boxCore);
        CabinetFinishMaterial finishMaterial = new(finishColor, finishCore, finishPaintColor);
        CabinetSide leftSide = new(AllmoxyXMLOrderProviderHelpers.GetCabinetSideType(Cabinet.LeftSide), mdfOptions);
        CabinetSide rightSide = new(AllmoxyXMLOrderProviderHelpers.GetCabinetSideType(Cabinet.RightSide), mdfOptions);

        string edgeBandingColor;
        if (Cabinet.EdgeBandColor == "Match Finish") {

            if (Cabinet.FinishMaterial.Type == "paint")
                edgeBandingColor = Cabinet.BoxMaterial.Finish;
            else {
                edgeBandingColor = Cabinet.FinishMaterial.Finish;
            }

        } else {
            edgeBandingColor = Cabinet.EdgeBandColor;
        }

        return (TBuilder)builder.WithQty(Cabinet.Qty)
                                    .WithUnitPrice(AllmoxyXMLOrderProviderHelpers.StringToMoney(Cabinet.UnitPrice))
                                    .WithBoxMaterial(boxMaterial)
                                    .WithFinishMaterial(finishMaterial)
                                    .WithLeftSide(leftSide)
                                    .WithRightSide(rightSide)
                                    .WithEdgeBandingColor(edgeBandingColor)
                                    .WithWidth(Dimension.FromMillimeters(Cabinet.Width))
                                    .WithHeight(Dimension.FromMillimeters(Cabinet.Height))
                                    .WithDepth(Dimension.FromMillimeters(Cabinet.Depth))
                                    .WithRoom(Cabinet.Room)
                                    .WithAssembled(Cabinet.Assembled == "Yes");

    }

}
