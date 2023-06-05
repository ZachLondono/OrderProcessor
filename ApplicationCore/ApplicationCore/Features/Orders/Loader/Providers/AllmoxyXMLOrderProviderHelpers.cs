using ApplicationCore.Features.Orders.Shared.Domain;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
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

    public static CabinetMaterialCore GetFinishedSideMaterialCore(string name, CabinetMaterialCore boxMaterial) => name switch {
        "pb" => CabinetMaterialCore.ParticleBoard,
        "ply" => CabinetMaterialCore.Plywood,
        "match" => boxMaterial,
        _ => throw new InvalidOperationException($"Unrecognized finish material core '{name}'")
    };

    public static CabinetMaterialCore GetMaterialCore(string name) => name switch {
        "pb" => CabinetMaterialCore.ParticleBoard,
        "ply" => CabinetMaterialCore.Plywood,
        _ => throw new InvalidOperationException($"Unrecognized material core '{name}'")
    };

    public static CabinetMaterialCore GetSlabDoorMaterialCoreFromFinishType(string finishType, CabinetMaterialCore cabFinishCore) => finishType switch {
        "match" or "paint" => cabFinishCore,
        "mela" => CabinetMaterialCore.ParticleBoard,
        "veneer" => CabinetMaterialCore.Plywood,
        _ => throw new InvalidOperationException("Unexpected cabinet door finish type")
        // "laminate" => cabBoxCore?? cabFinishCore??
    };

    public static CabinetMaterialFinishType GetMaterialFinishType(string name) => name switch {
        "mela" => CabinetMaterialFinishType.Melamine,
        "veneer" => CabinetMaterialFinishType.Veneer,
        "paint" => CabinetMaterialFinishType.Paint,
        "none" => CabinetMaterialFinishType.None,
        _ => throw new InvalidOperationException($"Unrecognized material finish type '{name}'")
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

    public static ToeType GetToeType(string name) => name switch {
        "Leg Levelers" => ToeType.LegLevelers,
        "Full Height Sides" => ToeType.FurnitureBase,
        "No Toe" => ToeType.NoToe,
        "Notched" => ToeType.Notched,
        _ => ToeType.LegLevelers
    };

    public static decimal StringToMoney(string str) {

        if (string.IsNullOrWhiteSpace(str)) {
            return 0;
        }

        return decimal.Parse(str.Replace("$", "").Replace(",", ""));

    }

}