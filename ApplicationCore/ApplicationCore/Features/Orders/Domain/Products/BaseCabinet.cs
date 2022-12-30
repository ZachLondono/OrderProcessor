using ApplicationCore.Features.Orders.Domain.ValueObjects;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.Orders.Domain.Products;

internal class BaseCabinet : Cabinet {

    public BaseCabinetDoors Doors { get; }
    public IToeType ToeType { get; }
    public HorizontalDrawerBank Drawers { get; }
    public BaseCabinetInside Inside { get; }

    public static BaseCabinet Create(int qty, decimal unitPrice, string room,
                        Dimension height, Dimension width, Dimension depth,
                        CabinetMaterial boxMaterial, CabinetMaterial finishMaterial,
                        CabinetSide rightSide, CabinetSide leftSide,
                        BaseCabinetDoors doors, IToeType toeType, HorizontalDrawerBank drawers, BaseCabinetInside inside) {
        return new(Guid.NewGuid(), qty, unitPrice, room, height, width, depth, boxMaterial, finishMaterial, rightSide, leftSide, doors, toeType, drawers, inside);
    }

    private BaseCabinet(Guid id, int qty, decimal unitPrice, string room,
                        Dimension height, Dimension width, Dimension depth,
                        CabinetMaterial boxMaterial, CabinetMaterial finishMaterial,
                        CabinetSide rightSide, CabinetSide leftSide,
                        BaseCabinetDoors doors, IToeType toeType, HorizontalDrawerBank drawers, BaseCabinetInside inside)
                        : base(id, qty, unitPrice, room, height, width, depth, boxMaterial, finishMaterial, rightSide, leftSide) {

        if (doors.Quantity > 2 || doors.Quantity < 0)
            throw new InvalidOperationException("Invalid number of doors");

        if (doors.Quantity == 1 && drawers.Quantity > 1)
            throw new InvalidOperationException("Base cabinet cannot have more than 1 drawer if it only has 1 door");

        if (drawers.Quantity > 2)
            throw new InvalidOperationException("Base cabinet cannot have more than 2 drawers");

        if (drawers.FaceHeight > Height)
            throw new InvalidOperationException("Invalid drawer face size");

        if (drawers.Quantity != 0 && drawers.FaceHeight == Dimension.Zero)
            throw new InvalidOperationException("Invalid drawer face size");

        if (drawers.Quantity == 0 && inside.RollOutBoxes.Positions.Length > 3)
            throw new InvalidOperationException("Base cabinet cannot have more than 3 roll out drawer boxes");

        if (drawers.Quantity > 1 && inside.RollOutBoxes.Positions.Length > 2)
            throw new InvalidOperationException("Base cabinet with drawer face cannot have more than 2 roll out drawer boxes");

        foreach (var position in inside.RollOutBoxes.Positions) {
            if (position < Dimension.Zero || position > height - drawers.FaceHeight) {
                throw new InvalidOperationException("Roll out box position {position} is invalid for cabinet size");
            }
        }

        Doors = doors;
        ToeType = toeType;
        Drawers = drawers;
        Inside = inside;

    }

    public override Dictionary<string, string> GetParameters() {
        var parameters = new Dictionary<string, string>() {
            { "ProductW", Width.AsMillimeters().ToString() },
            { "ProductH", Height.AsMillimeters().ToString() },
            { "ProductD", Depth.AsMillimeters().ToString() },
            { "FinishedLeft", GetSideOption(LeftSide.Type) },
            { "FinishedRight", GetSideOption(RightSide.Type) },
            { "ShelfQ", Inside.AdjustableShelves.ToString() },
            { "DividerQ", Inside.VerticalDividers.ToString() },
            { "PulloutBlockType", GetRollOutBlockOption() },
            { "AppliedPanel", GetAppliedPanelOption() },
        };

        int posNum = 1;
        foreach (var pos in Inside.RollOutBoxes.Positions) {
            parameters.Add($"Rollout{posNum++}", pos.AsMillimeters().ToString());
        }

        if (Doors.HingeSide != HingeSide.NotApplicable) {
            parameters.Add("HingeLeft", GetHingeSideOption());
        }

        if (Drawers.Quantity != 0) {
            parameters.Add("DrawerH1", Drawers.FaceHeight.AsMillimeters().ToString());
        }

        return parameters;
    }

    public override Dictionary<string, string> GetOverrideParameters() {

        var parameters = new Dictionary<string, string>();
        if (ToeType.PSIParameter != "2") {
            parameters.Add("__ToeBaseType", ToeType.PSIParameter);
        }

        return parameters;

    }

    public override string GetProductName() {

        if (Doors.Quantity == 1) {

            if (Drawers.Quantity == 1) return "B1D1D";
            return "B1D";

        }

        if (Drawers.Quantity == 2) return "B2D2D";
        else if (Drawers.Quantity == 2) return "B2D2D";
        return "B2D";

    }

    private string GetHingeSideOption() => Doors.HingeSide switch {
        HingeSide.NotApplicable => "0",
        HingeSide.Left => "1",
        HingeSide.Right => "0",
        _ => "0"
    };

    private string GetSideOption(CabinetSideType side) => side switch {
        CabinetSideType.AppliedPanel => "0",
        CabinetSideType.Unfinished => "0",
        CabinetSideType.Finished => "1",
        CabinetSideType.IntegratedPanel => "2",
        _ => "0"
    };

    private string GetRollOutBlockOption() => Inside.RollOutBoxes.Blocks switch {
        RollOutBlockPosition.None => "0",
        RollOutBlockPosition.Left => "1",
        RollOutBlockPosition.Both => "2",
        RollOutBlockPosition.Right => "3",
        _ => "0"
    };

    private string GetAppliedPanelOption() {
        if (LeftSide.Type == CabinetSideType.AppliedPanel && RightSide.Type != CabinetSideType.AppliedPanel) {
            return "1";
        } else if (LeftSide.Type == CabinetSideType.AppliedPanel && RightSide.Type == CabinetSideType.AppliedPanel) {
            return "2";
        } else if (LeftSide.Type != CabinetSideType.AppliedPanel && RightSide.Type == CabinetSideType.AppliedPanel) {
            return "3";
        } else return "0";
    }

}
