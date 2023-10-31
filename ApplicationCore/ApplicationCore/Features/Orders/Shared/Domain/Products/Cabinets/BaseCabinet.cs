using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.Components;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.Exceptions;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.Products.Cabinets;

internal class BaseCabinet : GarageCabinet, IDovetailDrawerBoxContainer, IMDFDoorContainer {

    public BaseCabinetDoors Doors { get; }
    public ToeType ToeType { get; }
    public HorizontalDrawerBank Drawers { get; }
    public BaseCabinetInside Inside { get; }
    public CabinetDrawerBoxOptions DrawerBoxOptions { get; }
    public CabinetBaseNotch? BaseNotch { get; }

    public Dimension DoorHeight => Height - ToeType.ToeHeight - DoorGaps.TopGap - DoorGaps.BottomGap - (Drawers.Quantity > 0 ? Drawers.FaceHeight + DoorGaps.VerticalGap : Dimension.Zero);

    public override string GetDescription() => $"Base {(IsGarage ? "Garage " : "")}Cabinet - {Doors.Quantity} Doors, {Drawers.Quantity} Drawers";

    public static CabinetDoorGaps DoorGaps { get; set; } = new() {
        TopGap = Dimension.FromMillimeters(7),
        BottomGap = Dimension.Zero,
        EdgeReveal = Dimension.FromMillimeters(2),
        HorizontalGap = Dimension.FromMillimeters(3),
        VerticalGap = Dimension.FromMillimeters(3),
    };

    public static BaseCabinet Create(int qty, decimal unitPrice, int productNumber, string room, bool assembled,
                        Dimension height, Dimension width, Dimension depth,
                        CabinetMaterial boxMaterial, CabinetFinishMaterial finishMaterial, CabinetSlabDoorMaterial? slabDoorMaterial, MDFDoorOptions? mdfDoorOptions, string edgeBandingColor,
                        CabinetSideType rightSideType, CabinetSideType leftSideType, string comment,
                        BaseCabinetDoors doors, ToeType toeType, HorizontalDrawerBank drawers, BaseCabinetInside inside, CabinetDrawerBoxOptions drawerBoxOptions, CabinetBaseNotch? baseNotch) {
        return new(Guid.NewGuid(), qty, unitPrice, productNumber, room, assembled, height, width, depth, boxMaterial, finishMaterial, slabDoorMaterial, mdfDoorOptions, edgeBandingColor, rightSideType, leftSideType, comment, doors, toeType, drawers, inside, drawerBoxOptions, baseNotch);
    }

    internal BaseCabinet(Guid id, int qty, decimal unitPrice, int productNumber, string room, bool assembled,
                        Dimension height, Dimension width, Dimension depth,
                        CabinetMaterial boxMaterial, CabinetFinishMaterial finishMaterial, CabinetSlabDoorMaterial? slabDoorMaterial, MDFDoorOptions? mdfDoorOptions, string edgeBandingColor,
                        CabinetSideType rightSideType, CabinetSideType leftSideType, string comment,
                        BaseCabinetDoors doors, ToeType toeType, HorizontalDrawerBank drawers, BaseCabinetInside inside, CabinetDrawerBoxOptions drawerBoxOptions, CabinetBaseNotch? baseNotch)
                        : base(id, qty, unitPrice, productNumber, room, assembled, height, width, depth, boxMaterial, finishMaterial, slabDoorMaterial, mdfDoorOptions, edgeBandingColor, rightSideType, leftSideType, comment) {

        if (doors.Quantity > 2 || doors.Quantity < 0)
            throw new InvalidProductOptionsException("Invalid number of doors");

        if (doors.Quantity == 1 && drawers.Quantity > 1)
            throw new InvalidProductOptionsException("Base cabinet cannot have more than 1 drawer if it only has 1 door");

        if (drawers.Quantity > 2)
            throw new InvalidProductOptionsException("Base cabinet cannot have more than 2 drawers");

        if (drawers.FaceHeight > Height)
            throw new InvalidProductOptionsException("Invalid drawer face size");

        if (drawers.Quantity != 0 && drawers.FaceHeight == Dimension.Zero)
            throw new InvalidProductOptionsException("Invalid drawer face size");

        if (drawers.Quantity == 0 && inside.RollOutBoxes.Positions.Length > 3)
            throw new InvalidProductOptionsException("Base cabinet cannot have more than 3 roll out drawer boxes");

        if (drawers.Quantity > 1 && inside.RollOutBoxes.Positions.Length > 2)
            throw new InvalidProductOptionsException("Base cabinet with drawer face cannot have more than 2 roll out drawer boxes");

        foreach (var position in inside.RollOutBoxes.Positions) {
            if (position < Dimension.Zero || position > Height - drawers.FaceHeight) {
                throw new InvalidProductOptionsException("Roll out box position {position} is invalid for cabinet size");
            }
        }

        Doors = doors;
        ToeType = toeType;
        Drawers = drawers;
        Inside = inside;
        DrawerBoxOptions = drawerBoxOptions;
        BaseNotch = baseNotch;
    }

    public bool ContainsDoors() => MDFDoorOptions is not null;

    public IEnumerable<MDFDoor> GetDoors(Func<MDFDoorBuilder> getBuilder) {

        if (MDFDoorOptions is null) {
            return Enumerable.Empty<MDFDoor>();
        }

        List<MDFDoor> doors = new();

        if (Doors.Quantity > 0) {
            Dimension width = (Width - 2 * DoorGaps.EdgeReveal - DoorGaps.HorizontalGap * (Doors.Quantity - 1)) / Doors.Quantity;
            Dimension height = DoorHeight;
            var door = getBuilder().WithQty(Doors.Quantity * Qty)
                                    .WithProductNumber(ProductNumber)
                                    .WithType(DoorType.Door)
                                    .WithFramingBead(MDFDoorOptions.FramingBead)
                                    .WithPaintColor(MDFDoorOptions.PaintColor == "" ? null : MDFDoorOptions.PaintColor)
                                    .Build(height, width);
            doors.Add(door);
        }

        if (Drawers.Quantity > 0) {
            Dimension drwWidth = (Width - 2 * DoorGaps.EdgeReveal - DoorGaps.HorizontalGap * (Drawers.Quantity - 1)) / Drawers.Quantity;
            var drawers = getBuilder().WithQty(Drawers.Quantity * Qty)
                                        .WithProductNumber(ProductNumber)
                                        .WithType(DoorType.DrawerFront)
                                        .WithFramingBead(MDFDoorOptions.FramingBead)
                                        .WithPaintColor(MDFDoorOptions.PaintColor == "" ? null : MDFDoorOptions.PaintColor)
                                        .Build(Drawers.FaceHeight, drwWidth);
            doors.Add(drawers);
        }

        return doors.ToArray();

    }

    public bool ContainsDovetailDrawerBoxes() => Drawers.Any() || Inside.RollOutBoxes.Any();

    public IEnumerable<DovetailDrawerBox> GetDovetailDrawerBoxes(Func<DovetailDrawerBoxBuilder> getBuilder) {

        List<DovetailDrawerBox> boxes = new();

        if (Drawers.Any()) {

            int drawerQty = Drawers.Quantity * Qty;

            var box = getBuilder().WithInnerCabinetDepth(InnerDepth, DrawerBoxOptions.SlideType)
                                    .WithInnerCabinetWidth(InnerWidth, Drawers.Quantity, DrawerBoxOptions.SlideType)
                                    .WithDrawerFaceHeight(Drawers.FaceHeight)
                                    .WithQty(drawerQty)
                                    .WithOptions(DrawerBoxOptions.GetDrawerBoxOptions())
                                    .WithProductNumber(ProductNumber)
                                    .Build();

            boxes.Add(box);

        }

        if (Inside.RollOutBoxes.Any()) {

            int rollOutQty = Inside.RollOutBoxes.Qty * Qty;
            var boxHeight = Dimension.FromMillimeters(104);

            var box = getBuilder().WithInnerCabinetDepth(InnerDepth, DrawerBoxOptions.SlideType, true)
                                    .WithInnerCabinetWidth(InnerWidth, Inside.RollOutBoxes.Blocks, DrawerBoxOptions.SlideType)
                                    .WithBoxHeight(boxHeight)
                                    .WithQty(rollOutQty)
                                    .WithOptions(DrawerBoxOptions.GetDrawerBoxOptions())
                                    .WithProductNumber(ProductNumber)
                                    .Build();

            boxes.Add(box);

        }

        return boxes;

    }

    public override IEnumerable<Supply> GetSupplies() {

        List<Supply> supplies = new();

        if (ToeType == ToeType.LegLevelers) {

            supplies.Add(Supply.CabinetLeveler(Qty * 4));

        }

        if (Inside.AdjustableShelves > 0) {

            supplies.Add(Supply.LockingShelfPeg(Inside.AdjustableShelves * 4 * Qty));

        }

        if (Doors.Quantity > 0) {

            supplies.Add(Supply.DoorPull(Doors.Quantity * Qty));
            supplies.AddRange(Supply.StandardHinge(DoorHeight, Doors.Quantity * Qty));

        }

        if (Drawers.Quantity > 0) {

            supplies.Add(Supply.DrawerPull(Drawers.Quantity * Qty));

        }

        Dimension boxDepth = DovetailDrawerBoxBuilder.GetDrawerBoxDepthFromInnerCabinetDepth(InnerDepth, DrawerBoxOptions.SlideType, true);

        if (Drawers.Quantity > 0) {

            switch (DrawerBoxOptions.SlideType) {
                case DrawerSlideType.UnderMount:
                    supplies.Add(Supply.UndermountSlide(Drawers.Quantity * Qty, boxDepth));
                    break;
                case DrawerSlideType.SideMount:
                    supplies.Add(Supply.SidemountSlide(Drawers.Quantity * Qty, boxDepth));
                    break;
            }

        }

        if (Inside.RollOutBoxes.Qty > 0) {

            switch (DrawerBoxOptions.SlideType) {
                case DrawerSlideType.UnderMount:
                    supplies.Add(Supply.UndermountSlide(Inside.RollOutBoxes.Qty * Qty, boxDepth));
                    break;
                case DrawerSlideType.SideMount:
                    supplies.Add(Supply.SidemountSlide(Inside.RollOutBoxes.Qty * Qty, boxDepth));
                    break;
            }

            switch (Inside.RollOutBoxes.Blocks) {
                case RollOutBlockPosition.Left:
                case RollOutBlockPosition.Right:
                    supplies.Add(Supply.PullOutBlock(Inside.RollOutBoxes.Qty * Qty));
                    break;
                case RollOutBlockPosition.Both:
                    supplies.Add(Supply.PullOutBlock(Inside.RollOutBoxes.Qty * Qty * 2));
                    break;
            }

        }

        return supplies;

    }

    public override string GetProductSku() {
        string name = $"{(IsGarage ? "G" : "")}B{Doors.Quantity}D";
        if (Drawers.Quantity != 0) name += $"{Drawers.Quantity}D";
        return name;
    }

    protected override IDictionary<string, string> GetParameters() {
        var parameters = new Dictionary<string, string>() {
            { "ProductW", Width.AsMillimeters().ToString() },
            { "ProductH", Height.AsMillimeters().ToString() },
            { "ProductD", Depth.AsMillimeters().ToString() },
            { "FinishedLeft", GetSideOption(LeftSideType) },
            { "FinishedRight", GetSideOption(RightSideType) },
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

    protected override IDictionary<string, string> GetParameterOverrides() {

        var parameters = new Dictionary<string, string>();
        if (ToeType.PSIParameter != "2") {
            parameters.Add("__ToeBaseType", ToeType.PSIParameter);
            if (ToeType.PSIParameter == "3") {
                parameters.Add("__ToeBaseHeight", "0");
            }
        }

        if (Drawers.Quantity != 0 && DrawerBoxOptions.SlideType == DrawerSlideType.SideMount) {
            parameters.Add("_DrawerRunType", "4");
        }

        if (LeftSideType != CabinetSideType.IntegratedPanel && RightSideType != CabinetSideType.IntegratedPanel && Inside.ShelfDepth == ShelfDepth.Full) {

            Dimension backThickness = Dimension.FromMillimeters(13);
            Dimension backInset = Dimension.FromMillimeters(9);
            Dimension shelfFrontClearance = Dimension.FromMillimeters(8);

            parameters.Add("_HalfDepthShelfW", (Depth - backThickness - backInset - shelfFrontClearance).AsMillimeters().ToString());

        }

        if (BaseNotch is not null) {
            parameters.Add("_BottomNotchD", BaseNotch.Depth.AsMillimeters().ToString());
            parameters.Add("_BottomNotchH", BaseNotch.Height.AsMillimeters().ToString());
        }

        return parameters;

    }

    protected override IDictionary<string, string> GetManualOverrideParameters() {

        var parameters = new Dictionary<string, string>();

        if (LeftSideType != CabinetSideType.IntegratedPanel && RightSideType != CabinetSideType.IntegratedPanel) {

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
