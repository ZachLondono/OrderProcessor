using ApplicationCore.Features.Orders.Domain.ValueObjects;
using ApplicationCore.Features.ProductPlanner.Contracts;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.Orders.Domain.Products;

internal class SinkCabinet : Cabinet, IPPProductContainer, IDoorContainer {

    public IToeType ToeType { get; }
    public HingeSide HingeSide { get; }
    public int DoorQty { get; }
    public int FalseDrawerQty { get; }
    public Dimension DrawerFaceHeight { get; }
    public int AdjustableShelves { get; }
    public RollOutOptions RollOutBoxes { get; }
    public MDFDoorOptions? MDFOptions { get; }

    public SinkCabinet(Guid id, int qty, decimal unitPrice, string room, bool assembled,
                        Dimension height, Dimension width, Dimension depth,
                        CabinetMaterial boxMaterial, CabinetMaterial finishMaterial, string edgeBandingColor,
                        CabinetSide rightSide, CabinetSide leftSide,
                        IToeType toeType, HingeSide hingeSide, int doorQty, int falseDrawerQty, Dimension drawerFaceHeight, int adjustableShelves, RollOutOptions rollOutBoxes, MDFDoorOptions? mdfOptions)
                        : base(id, qty, unitPrice, room, assembled, height, width, depth, boxMaterial, finishMaterial, edgeBandingColor, rightSide, leftSide) {
        ToeType = toeType;
        HingeSide = hingeSide;
        DoorQty = doorQty;
        FalseDrawerQty = falseDrawerQty;
        DrawerFaceHeight = drawerFaceHeight;
        AdjustableShelves = adjustableShelves;
        RollOutBoxes = rollOutBoxes;
        MDFOptions = mdfOptions;
    }

    public static SinkCabinet Create(int qty, decimal unitPrice, string room, bool assembled,
                        Dimension height, Dimension width, Dimension depth,
                        CabinetMaterial boxMaterial, CabinetMaterial finishMaterial, string edgeBandingColor,
                        CabinetSide rightSide, CabinetSide leftSide,
                        IToeType toeType, HingeSide hingeSide, int doorQty, int falseDrawerQty, Dimension drawerFaceHeight, int adjustableShelves, RollOutOptions rollOutBoxes, MDFDoorOptions? mdfOptions)
                        => new(Guid.NewGuid(), qty, unitPrice, room, assembled, height, width, depth, boxMaterial, finishMaterial, edgeBandingColor, rightSide, leftSide, toeType, hingeSide, doorQty, falseDrawerQty, drawerFaceHeight, adjustableShelves, rollOutBoxes, mdfOptions);

    public IEnumerable<MDFDoor> GetDoors() {
        throw new NotImplementedException();
    }

    public IEnumerable<PPProduct> GetPPProducts() {
        string doorType = (MDFOptions is null) ? "Slab" : "Buyout";
        yield return new PPProduct(Room, GetProductName(), "Royal2", GetMaterialType(), doorType, "Standard", GetFinishMaterials(), GetEBMaterials(), GetParameters(), GetOverrideParameters(), new());
    }

    private string GetProductName() => $"S{DoorQty}D{FalseDrawerQty}FD";

    private Dictionary<string, string> GetParameters() {
        var parameters = new Dictionary<string, string>() {
            { "ProductW", Width.AsMillimeters().ToString() },
            { "ProductH", Height.AsMillimeters().ToString() },
            { "ProductD", Depth.AsMillimeters().ToString() },
            { "FinishedLeft", GetSideOption(LeftSide.Type) },
            { "FinishedRight", GetSideOption(RightSide.Type) },
            { "ShelfQ", AdjustableShelves.ToString() },
            { "PulloutBlockType", GetRollOutBlockOption() },
            { "AppliedPanel", GetAppliedPanelOption() },
        };

        int posNum = 1;
        foreach (var pos in RollOutBoxes.Positions) {
            parameters.Add($"Rollout{posNum++}", pos.AsMillimeters().ToString());
        }

        if (HingeSide != HingeSide.NotApplicable) {
            parameters.Add("HingeLeft", GetHingeSideOption());
        }

        if (FalseDrawerQty != 0) {
            parameters.Add("DrawerH1", DrawerFaceHeight.AsMillimeters().ToString());
        }

        return parameters;
    }

    private Dictionary<string, string> GetOverrideParameters() {

        var parameters = new Dictionary<string, string>();
        if (ToeType.PSIParameter != "2") {
            parameters.Add("__ToeBaseType", ToeType.PSIParameter);
        }

        if (RollOutBoxes.Positions.Any() && RollOutBoxes.SlideType == DrawerSlideType.SideMount) {
            parameters.Add("_DrawerRunType", "4");
        }

        return parameters;

    }

    private string GetHingeSideOption() => HingeSide switch {
        HingeSide.NotApplicable => "0",
        HingeSide.Left => "1",
        HingeSide.Right => "0",
        _ => "0"
    };

    private string GetRollOutBlockOption() => RollOutBoxes.Blocks switch {
        RollOutBlockPosition.None => "0",
        RollOutBlockPosition.Left => "1",
        RollOutBlockPosition.Both => "2",
        RollOutBlockPosition.Right => "3",
        _ => "0"
    };
}
