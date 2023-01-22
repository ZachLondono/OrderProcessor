using ApplicationCore.Features.Orders.Loader.Providers.AllmoxyXMLModels;
using ApplicationCore.Features.Orders.Shared.Domain;
using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Shared.Domain;

internal static class AllmoxyXMLOrderProviderHelpers {

    public static CabinetSideType GetCabinetSideType(string name) => name switch {
        "Unfinished" => CabinetSideType.Unfinished,
        "Finished" => CabinetSideType.Finished,
        "Integrated" => CabinetSideType.IntegratedPanel,
        "Applied" => CabinetSideType.AppliedPanel,
        _ => CabinetSideType.Unfinished
    };

    public static CabinetDrawerBoxMaterial GetDrawerMaterial(string name) => name switch {
        "Pre-Finished Birch" => CabinetDrawerBoxMaterial.SolidBirch,
        "Economy Birch" => CabinetDrawerBoxMaterial.FingerJointBirch,
        _ => CabinetDrawerBoxMaterial.FingerJointBirch
    };

    public static DrawerSlideType GetDrawerSlideType(string name) => name switch {
        "Under Mount" => DrawerSlideType.UnderMount,
        "Side Mount" => DrawerSlideType.SideMount,
        _ => DrawerSlideType.UnderMount
    };

    public static CabinetMaterialCore GetFinishedSideMaterialCore(string name, CabinetMaterialCore boxMaterial) {

        if (boxMaterial == CabinetMaterialCore.Flake) {

            return name switch {
                "veneer" => CabinetMaterialCore.Plywood,
                _ => CabinetMaterialCore.Flake
            };

        } else if (boxMaterial == CabinetMaterialCore.Plywood) {

            return CabinetMaterialCore.Plywood;

        }

        return CabinetMaterialCore.Flake;
    }

    public static CabinetMaterialCore GetMaterialCore(string name) => name switch {
        "pb" => CabinetMaterialCore.Flake,
        "ply" => CabinetMaterialCore.Plywood,
        _ => CabinetMaterialCore.Flake
    };

    public static RollOutBlockPosition GetRollOutBlockPositions(string name) => name switch {
        "Left" => RollOutBlockPosition.Left,
        "Right" => RollOutBlockPosition.Right,
        "Both" => RollOutBlockPosition.Both,
        _ => RollOutBlockPosition.None
    };

    public static Dimension[] GetRollOutPositions(string pos1, string pos2, string pos3, string pos4, string pos5) {

        int count = 0;
        if (pos5 == "Yes") count = 5;
        else if (pos4 == "Yes") count = 4;
        else if (pos3 == "Yes") count = 3;
        else if (pos2 == "Yes") count = 2;
        else if (pos1 == "Yes") count = 1;

        if (count == 0) return Array.Empty<Dimension>();

        var positions = new Dimension[count];
        if (count >= 1) positions[0] = pos1 == "Yes" ? Dimension.FromMillimeters(19) : Dimension.Zero;
        if (count >= 2) positions[1] = pos2 == "Yes" ? Dimension.FromMillimeters(300) : Dimension.Zero;
        if (count >= 3) positions[2] = pos3 == "Yes" ? (count >= 4 ? Dimension.FromMillimeters(581) : Dimension.FromMillimeters(497)) : Dimension.Zero;
        if (count >= 4) positions[3] = pos4 == "Yes" ? Dimension.FromMillimeters(862) : Dimension.Zero;
        if (count >= 5) positions[4] = pos5 == "Yes" ? Dimension.FromMillimeters(1142) : Dimension.Zero;

        return positions;

    }

    public static ShelfDepth GetShelfDepth(string name) => name switch {
        "Full" => ShelfDepth.Full,
        "Half" => ShelfDepth.Half,
        "3/4" => ShelfDepth.ThreeQuarters,
        _ => ShelfDepth.Default
    };

    public static IToeType GetToeType(string name) => name switch {
        "Leg Levelers" => new LegLevelers(Dimension.FromMillimeters(102)),
        "Full Height Sides" => new FurnitureBase(Dimension.FromMillimeters(102)),
        "No Toe" => new NoToe(),
        "Notched" => new Notched(Dimension.FromMillimeters(102)),
        _ => new LegLevelers(Dimension.FromMillimeters(102))
    };

    public static decimal StringToMoney(string str) => decimal.Parse(str.Replace("$", "").Replace(",", ""));

    public static TBuilder InitilizeBuilder<TBuilder, TCabinet>(TBuilder builder, CabinetModelBase model) where TBuilder : CabinetBuilder<TCabinet> where TCabinet : Cabinet {

        CabinetMaterialCore boxCore = GetMaterialCore(model.Cabinet.BoxMaterial.Type);
        CabinetMaterialCore finishCore = GetFinishedSideMaterialCore(model.Cabinet.FinishMaterial.Type, boxCore);

        MDFDoorOptions? mdfOptions = null;
        if (model.Cabinet.Fronts.Type != "Slab") mdfOptions = new(model.Cabinet.Fronts.Style, model.Cabinet.Fronts.Color);

        string finishColor = (model.Cabinet.FinishMaterial.Type == "paint" ? model.Cabinet.BoxMaterial.Finish : model.Cabinet.FinishMaterial.Finish);
        CabinetMaterial boxMaterial = new(model.Cabinet.BoxMaterial.Finish, boxCore);
        CabinetMaterial finishMaterial = new(finishColor, finishCore);
        CabinetSide leftSide = new(GetCabinetSideType(model.Cabinet.LeftSide), mdfOptions);
        CabinetSide rightSide = new(GetCabinetSideType(model.Cabinet.RightSide), mdfOptions);

        string edgeBandingColor = (model.Cabinet.EdgeBandColor == "Match Finish" ? model.Cabinet.FinishMaterial.Finish : model.Cabinet.EdgeBandColor);


        return (TBuilder)builder.WithQty(model.Cabinet.Qty)
                                    .WithUnitPrice(StringToMoney(model.Cabinet.UnitPrice))
                                    .WithBoxMaterial(boxMaterial)
                                    .WithFinishMaterial(finishMaterial)
                                    .WithLeftSide(leftSide)
                                    .WithRightSide(rightSide)
                                    .WithEdgeBandingColor(edgeBandingColor)
                                    .WithWidth(Dimension.FromMillimeters(model.Cabinet.Width))
                                    .WithHeight(Dimension.FromMillimeters(model.Cabinet.Height))
                                    .WithDepth(Dimension.FromMillimeters(model.Cabinet.Depth))
                                    .WithRoom(model.Cabinet.Room)
                                    .WithAssembled(model.Cabinet.Assembled == "Yes");

    }

}