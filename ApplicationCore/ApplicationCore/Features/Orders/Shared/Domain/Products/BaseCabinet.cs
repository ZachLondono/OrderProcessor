using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.ProductPlanner.Contracts;
using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.Products;

internal class BaseCabinet : Cabinet, IPPProductContainer, IDrawerBoxContainer, IDoorContainer {

    public BaseCabinetDoors Doors { get; }
    public IToeType ToeType { get; }
    public HorizontalDrawerBank Drawers { get; }
    public BaseCabinetInside Inside { get; }

    public CabinetDoorGaps DoorGaps { get; set; } = new();

    public static BaseCabinet Create(int qty, decimal unitPrice, string room, bool assembled,
                        Dimension height, Dimension width, Dimension depth,
                        CabinetMaterial boxMaterial, CabinetMaterial finishMaterial, string edgeBandingColor,
                        CabinetSide rightSide, CabinetSide leftSide,
                        BaseCabinetDoors doors, IToeType toeType, HorizontalDrawerBank drawers, BaseCabinetInside inside) {
        return new(Guid.NewGuid(), qty, unitPrice, room, assembled, height, width, depth, boxMaterial, finishMaterial, edgeBandingColor, rightSide, leftSide, doors, toeType, drawers, inside);
    }

    private BaseCabinet(Guid id, int qty, decimal unitPrice, string room, bool assembled,
                        Dimension height, Dimension width, Dimension depth,
                        CabinetMaterial boxMaterial, CabinetMaterial finishMaterial, string edgeBandingColor,
                        CabinetSide rightSide, CabinetSide leftSide,
                        BaseCabinetDoors doors, IToeType toeType, HorizontalDrawerBank drawers, BaseCabinetInside inside)
                        : base(id, qty, unitPrice, room, assembled, height, width, depth, boxMaterial, finishMaterial, edgeBandingColor, rightSide, leftSide) {

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

    public IEnumerable<PPProduct> GetPPProducts() {
        string doorType = (Doors.MDFOptions is null) ? "Slab" : "Buyout";
        yield return new PPProduct(Room, GetProductName(), "Royal2", GetMaterialType(), doorType, "Standard", GetFinishMaterials(), GetEBMaterials(), GetParameters(), GetOverrideParameters(), GetManualOverrideParameters());
    }

    public IEnumerable<DovetailDrawerBox> GetDrawerBoxes() {

        if (Drawers.Quantity > 0) {

            var options = new DrawerBoxOptions("", "", "", "", "Blum", GetNotchFromSlideType(Drawers.SlideType), "", LogoPosition.None);

            var height = Drawers.GetBoxHeight(Dimension.FromMillimeters(3), new List<Dimension>() {
                Dimension.FromMillimeters(64),
                Dimension.FromMillimeters(86),
                Dimension.FromMillimeters(105),
                Dimension.FromMillimeters(137),
                Dimension.FromMillimeters(159),
                Dimension.FromMillimeters(181),
                Dimension.FromMillimeters(210),
                Dimension.FromMillimeters(260),
            });

            var width = Drawers.GetBoxWidth(Width - Dimension.FromMillimeters(19 * 2), Dimension.FromMillimeters(19), (slide) => {
                return Dimension.FromMillimeters(0);
            });

            var depth = Depth - Dimension.FromMillimeters(13 + 9);

            yield return new DovetailDrawerBox(Drawers.Quantity, height, width, depth, "", options, new Dictionary<string, string>());

        }

        if (Inside.RollOutBoxes.Positions.Any()) {

            var options = new DrawerBoxOptions("", "", "", "", "Blum", GetNotchFromSlideType(Inside.RollOutBoxes.SlideType), "", LogoPosition.None);

            var width = Width - Dimension.FromMillimeters(19 * 2);

            switch (Inside.RollOutBoxes.Blocks) {
                case RollOutBlockPosition.Left:
                case RollOutBlockPosition.Right:
                    width -= Dimension.FromInches(1);
                    break;
                case RollOutBlockPosition.Both:
                    width -= Dimension.FromInches(2);
                    break;
            }

            var depth = Depth - Dimension.FromMillimeters(13 + 9);

            yield return new DovetailDrawerBox(Inside.RollOutBoxes.Positions.Length, Dimension.FromInches(4.125), width, depth, "", options, new Dictionary<string, string>());

        }

    }

    public IEnumerable<MDFDoor> GetDoors(Func<MDFDoorBuilder> getBuilder) {

        if (Doors.MDFOptions is null) {
            return Enumerable.Empty<MDFDoor>();
        }

        List<MDFDoor> doors = new();

        if (Doors.Quantity > 0) { 
            Dimension width = (Width - 2 * DoorGaps.EdgeReveal - DoorGaps.HorizontalGap * (Doors.Quantity - 1)) / Doors.Quantity;
            Dimension height = Height - ToeType.ToeHeight - DoorGaps.TopGap - DoorGaps.BottomGap - (Drawers.Quantity > 0 ? Drawers.FaceHeight + DoorGaps.VerticalGap : Dimension.Zero);
            var door = getBuilder().WithQty(Doors.Quantity * Qty)
                                    .WithType(DoorType.Door)
                                    .Build(height, width);
            doors.Add(door);
        }

        if (Drawers.Quantity > 0) {
            Dimension drwWidth = (Width - 2 * DoorGaps.EdgeReveal - DoorGaps.HorizontalGap * (Drawers.Quantity - 1)) / Drawers.Quantity;
            var drawers = getBuilder().WithQty(Drawers.Quantity * Qty)
                                        .WithType(DoorType.DrawerFront)
                                        .Build(Drawers.FaceHeight, drwWidth);
            doors.Add(drawers);
        }

        return doors.ToArray();

    }

    private static string GetNotchFromSlideType(DrawerSlideType slide) => slide switch {
        DrawerSlideType.UnderMount => "Standard Notch",
        DrawerSlideType.SideMount => "",
        _ => ""
    };

    private string GetProductName() {
        string name = $"B{Doors.Quantity}D";
        if (Drawers.Quantity != 0) name += $"{Drawers.Quantity}D";
        return name;
    }

    private Dictionary<string, string> GetParameters() {
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

    private Dictionary<string, string> GetOverrideParameters() {

        var parameters = new Dictionary<string, string>();
        if (ToeType.PSIParameter != "2") {
            parameters.Add("__ToeBaseType", ToeType.PSIParameter);
            if (ToeType.PSIParameter == "3") {
                parameters.Add("__ToeBaseHeight", "0");
            }
        }

        if (Drawers.Quantity != 0 && Drawers.SlideType == DrawerSlideType.SideMount) {
            parameters.Add("_DrawerRunType", "4");
        }

        if (LeftSide.Type != CabinetSideType.IntegratedPanel && RightSide.Type != CabinetSideType.IntegratedPanel && Inside.ShelfDepth == ShelfDepth.Full) {

            Dimension backThickness = Dimension.FromMillimeters(13);
            Dimension backInset = Dimension.FromMillimeters(9);
            Dimension shelfFrontClearance = Dimension.FromMillimeters(8);

            parameters.Add("_HalfDepthShelfW", (Depth - backThickness - backInset - shelfFrontClearance).AsMillimeters().ToString());

        }

        return parameters;

    }

    public Dictionary<string, string> GetManualOverrideParameters() {

        var parameters = new Dictionary<string, string>();

        if (LeftSide.Type != CabinetSideType.IntegratedPanel && RightSide.Type != CabinetSideType.IntegratedPanel) {

            if (Inside.ShelfDepth == ShelfDepth.Half) {

                Dimension newWidth = (Depth - Dimension.FromMillimeters(13) - Dimension.FromMillimeters(9)) / 2;

                parameters.Add("_HalfDepthShelfW", newWidth.AsMillimeters().ToString());

            } else if (Inside.ShelfDepth == ShelfDepth.ThreeQuarters) {

                Dimension newWidth = (Depth - Dimension.FromMillimeters(13) - Dimension.FromMillimeters(9)) * 0.75;

                parameters.Add("_HalfDepthShelfW", newWidth.AsMillimeters().ToString());

            }

        }


        return parameters;

    }

    private string GetHingeSideOption() => Doors.HingeSide switch {
        HingeSide.NotApplicable => "0",
        HingeSide.Left => "1",
        HingeSide.Right => "0",
        _ => "0"
    };

    private string GetRollOutBlockOption() => Inside.RollOutBoxes.Blocks switch {
        RollOutBlockPosition.None => "0",
        RollOutBlockPosition.Left => "1",
        RollOutBlockPosition.Both => "2",
        RollOutBlockPosition.Right => "3",
        _ => "0"
    };

}
