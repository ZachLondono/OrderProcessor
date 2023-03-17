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

    public int GetProductNumber() => int.Parse($"{GroupNumber}{LineNumber:00}");

    public TBuilder InitilizeBuilder<TBuilder, TCabinet>(TBuilder builder) where TBuilder : CabinetBuilder<TCabinet> where TCabinet : Cabinet {

        CabinetMaterialCore boxCore = AllmoxyXMLOrderProviderHelpers.GetMaterialCore(Cabinet.BoxMaterial.Core);
        CabinetMaterialFinishType boxFinishType = AllmoxyXMLOrderProviderHelpers.GetMaterialFinishType(Cabinet.BoxMaterial.Type);
        CabinetMaterialCore finishCore = AllmoxyXMLOrderProviderHelpers.GetFinishedSideMaterialCore(Cabinet.FinishMaterial.Core, boxCore);
        CabinetMaterialFinishType finishFinishType = AllmoxyXMLOrderProviderHelpers.GetMaterialFinishType(Cabinet.FinishMaterial.Type);

        MDFDoorOptions? mdfOptions = null;
        if (Cabinet.Fronts.Type != "Slab") mdfOptions = new("MDF", Dimension.FromInches(0.75), Cabinet.Fronts.Style, "Eased", "Flat", Dimension.Zero, Cabinet.Fronts.Color);

        string finishColor = (Cabinet.FinishMaterial.Type == "paint" ? Cabinet.BoxMaterial.Finish : Cabinet.FinishMaterial.Finish);
        string? finishPaintColor = (Cabinet.FinishMaterial.Type == "paint" ? Cabinet.FinishMaterial.Finish : null);
        CabinetMaterial boxMaterial = new(Cabinet.BoxMaterial.Finish, boxFinishType, boxCore);
        CabinetFinishMaterial finishMaterial = new(finishColor, finishFinishType, finishCore, finishPaintColor);
        CabinetSideType leftSideType = AllmoxyXMLOrderProviderHelpers.GetCabinetSideType(Cabinet.LeftSide);
        CabinetSideType rightSideType = AllmoxyXMLOrderProviderHelpers.GetCabinetSideType(Cabinet.RightSide);

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
                                    .WithProductNumber(GetProductNumber())
                                    .WithBoxMaterial(boxMaterial)
                                    .WithFinishMaterial(finishMaterial)
                                    .WithLeftSideType(leftSideType)
                                    .WithRightSideType(rightSideType)
                                    .WithEdgeBandingColor(edgeBandingColor)
                                    .WithWidth(Dimension.FromMillimeters(Cabinet.Width))
                                    .WithHeight(Dimension.FromMillimeters(Cabinet.Height))
                                    .WithDepth(Dimension.FromMillimeters(Cabinet.Depth))
                                    .WithRoom(Cabinet.Room)
                                    .WithMDFDoorOptions(mdfOptions)
                                    .WithAssembled(Cabinet.Assembled == "Yes")
                                    .WithComment(Cabinet.Comment);

    }

}
