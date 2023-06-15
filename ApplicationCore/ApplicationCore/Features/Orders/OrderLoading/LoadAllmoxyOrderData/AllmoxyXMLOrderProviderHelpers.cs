using ApplicationCore.Features.Orders.Shared.Domain;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.Orders.OrderLoading.LoadAllmoxyOrderData; 

internal static class AllmoxyXMLOrderProviderHelpers {

    public const string XML_BOOL_TRUE = "Yes";

    public const string PARTICLE_BOARD_CORE_CODE = "pb";
    public const string PLYWOOD_CORE_CODE = "ply";
    public const string MATCH_CORE_CODE = "match";

    public const string MELAMINE_FINISH_CODE = "mela";
    public const string VENEER_FINISH_CODE = "veneer";
    public const string PAINT_FINISH_CODE = "paint";
    public const string MATCH_FINISH_CODE = "match";
    public const string NO_FINISH_CODE = "none";

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
        PARTICLE_BOARD_CORE_CODE => CabinetMaterialCore.ParticleBoard,
        PLYWOOD_CORE_CODE => CabinetMaterialCore.Plywood,
        MATCH_CORE_CODE => boxMaterial,
        _ => throw new InvalidOperationException($"Unrecognized finish material core '{name}'")
    };

    public static CabinetMaterialCore GetMaterialCore(string name) => name switch {
        PARTICLE_BOARD_CORE_CODE => CabinetMaterialCore.ParticleBoard,
        PLYWOOD_CORE_CODE => CabinetMaterialCore.Plywood,
        _ => throw new InvalidOperationException($"Unrecognized material core '{name}'")
    };

    public static CabinetMaterialCore GetSlabDoorMaterialCoreFromFinishType(string finishType, CabinetMaterialCore cabFinishCore) => finishType switch {
        MATCH_FINISH_CODE or PAINT_FINISH_CODE => cabFinishCore,
        MELAMINE_FINISH_CODE => CabinetMaterialCore.ParticleBoard,
        VENEER_FINISH_CODE => CabinetMaterialCore.Plywood,
        _ => throw new InvalidOperationException("Unexpected cabinet door finish type")
        // "laminate" => cabBoxCore?? cabFinishCore??
    };

    public static CabinetMaterialFinishType GetMaterialFinishType(string name) => name switch {
        MELAMINE_FINISH_CODE => CabinetMaterialFinishType.Melamine,
        VENEER_FINISH_CODE => CabinetMaterialFinishType.Veneer,
        PAINT_FINISH_CODE => CabinetMaterialFinishType.Paint,
        NO_FINISH_CODE => CabinetMaterialFinishType.None,
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
        if (pos5 == XML_BOOL_TRUE) count = 5;
        else if (pos4 == XML_BOOL_TRUE) count = 4;
        else if (pos3 == XML_BOOL_TRUE) count = 3;
        else if (pos2 == XML_BOOL_TRUE) count = 2;
        else if (pos1 == XML_BOOL_TRUE) count = 1;

        if (count == 0) return Array.Empty<Dimension>();

        var positions = new Dimension[count];
        if (count >= 1) positions[0] = pos1 == XML_BOOL_TRUE ? Dimension.FromMillimeters(19) : Dimension.Zero;
        if (count >= 2) positions[1] = pos2 == XML_BOOL_TRUE ? Dimension.FromMillimeters(300) : Dimension.Zero;
        if (count >= 3) positions[2] = pos3 == XML_BOOL_TRUE ? count >= 4 ? Dimension.FromMillimeters(581) : Dimension.FromMillimeters(497) : Dimension.Zero;
        if (count >= 4) positions[3] = pos4 == XML_BOOL_TRUE ? Dimension.FromMillimeters(862) : Dimension.Zero;
        if (count >= 5) positions[4] = pos5 == XML_BOOL_TRUE ? Dimension.FromMillimeters(1142) : Dimension.Zero;

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

    public static HingeSide GetHingeSide(string value) => value switch {
        "Left" => HingeSide.Left,
        "Right" => HingeSide.Right,
        "N/A" => HingeSide.NotApplicable,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unexpected hinge side value")
    };

    public static BlindSide GetBlindSide(string value) => value switch {
        "Left" => BlindSide.Left,
        "Right" => BlindSide.Right,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unexpected blind side value")
    };

    public static decimal StringToMoney(string str) {

        if (string.IsNullOrWhiteSpace(str)) {
            return 0;
        }

        return decimal.Parse(str.Replace("$", "").Replace(",", ""));

    }

}