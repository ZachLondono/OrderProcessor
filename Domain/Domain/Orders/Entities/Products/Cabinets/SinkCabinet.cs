using Domain.Orders.Builders;
using Domain.Orders.Components;
using Domain.Orders.Entities.Hardware;
using Domain.Orders.Enums;
using Domain.Orders.ValueObjects;
using Domain.ValueObjects;

namespace Domain.Orders.Entities.Products.Cabinets;

public class SinkCabinet : Cabinet, IMDFDoorContainer, IDovetailDrawerBoxContainer, ISupplyContainer, IDrawerSlideContainer {

    public ToeType ToeType { get; }
    public HingeSide HingeSide { get; }
    public int DoorQty { get; }
    public int FalseDrawerQty { get; }
    public Dimension DrawerFaceHeight { get; }
    public int AdjustableShelves { get; }
    public ShelfDepth ShelfDepth { get; }
    public RollOutOptions RollOutBoxes { get; }
    public CabinetDrawerBoxOptions? DrawerBoxOptions { get; }
    public bool TiltFront { get; }
    public ScoopSides? Scoops { get; }

    public override string GetDescription()
        => $"Sink Cabinet - {DoorQty} Doors{(FalseDrawerQty > 0 ? $", {FalseDrawerQty} False Drawers" : "")}{(RollOutBoxes.Any() ? $", {RollOutBoxes.Qty} Roll Out Drawers" : "")}";

    public override string GetSimpleDescription() => "Sink Cabinet";

    public Dimension DoorHeight => Height - ToeType.ToeHeight - DoorGaps.TopGap - DoorGaps.BottomGap - (FalseDrawerQty > 0 ? DrawerFaceHeight + DoorGaps.VerticalGap : Dimension.Zero);

    public static CabinetDoorGaps DoorGaps { get; set; } = new() {
        TopGap = Dimension.FromMillimeters(7),
        BottomGap = Dimension.Zero,
        EdgeReveal = Dimension.FromMillimeters(2),
        HorizontalGap = Dimension.FromMillimeters(3),
        VerticalGap = Dimension.FromMillimeters(3),
    };

    public SinkCabinet(Guid id, int qty, decimal unitPrice, int productNumber, string room, bool assembled,
                        Dimension height, Dimension width, Dimension depth,
                        CabinetMaterial boxMaterial, CabinetFinishMaterial finishMaterial, CabinetSlabDoorMaterial? slabDoorMaterial, MDFDoorOptions? mdfDoorOptions, string edgeBandingColor,
                        CabinetSideType rightSideType, CabinetSideType leftSideType, string comment,
                        ToeType toeType, HingeSide hingeSide, int doorQty, int falseDrawerQty, Dimension drawerFaceHeight, int adjustableShelves, ShelfDepth shelfDepth, RollOutOptions rollOutBoxes, CabinetDrawerBoxOptions? drawerBoxOptions, bool tiltFront, ScoopSides? scoops)
                        : base(id, qty, unitPrice, productNumber, room, assembled, height, width, depth, boxMaterial, finishMaterial, slabDoorMaterial, mdfDoorOptions, edgeBandingColor, rightSideType, leftSideType, comment) {
        ToeType = toeType;
        HingeSide = hingeSide;
        DoorQty = doorQty;
        FalseDrawerQty = falseDrawerQty;
        DrawerFaceHeight = drawerFaceHeight;
        AdjustableShelves = adjustableShelves;
        ShelfDepth = shelfDepth;
        RollOutBoxes = rollOutBoxes;
        DrawerBoxOptions = drawerBoxOptions;
        TiltFront = tiltFront;
        Scoops = scoops;

        if (RollOutBoxes.Positions.Length > 0 && DrawerBoxOptions is not null && DrawerBoxOptions.SlideType == DrawerSlideType.SideMount) {
            ProductionNotes.Add("PSI may not support roll out drawer boxes with side mount slieds");
        }

    }

    public static SinkCabinet Create(int qty, decimal unitPrice, int productNumber, string room, bool assembled,
                        Dimension height, Dimension width, Dimension depth,
                        CabinetMaterial boxMaterial, CabinetFinishMaterial finishMaterial, CabinetSlabDoorMaterial? slabDoorMaterial, MDFDoorOptions? mdfDoorOptions, string edgeBandingColor,
                        CabinetSideType rightSideType, CabinetSideType leftSideType, string comment,
                        ToeType toeType, HingeSide hingeSide, int doorQty, int falseDrawerQty, Dimension drawerFaceHeight, int adjustableShelves, ShelfDepth shelfDepth, RollOutOptions rollOutBoxes, CabinetDrawerBoxOptions? drawerBoxOptions, bool tiltFront, ScoopSides? scoops)
                        => new(Guid.NewGuid(), qty, unitPrice, productNumber, room, assembled, height, width, depth, boxMaterial, finishMaterial, slabDoorMaterial, mdfDoorOptions, edgeBandingColor, rightSideType, leftSideType, comment, toeType, hingeSide, doorQty, falseDrawerQty, drawerFaceHeight, adjustableShelves, shelfDepth, rollOutBoxes, drawerBoxOptions, tiltFront, scoops);

    public bool ContainsDoors() => MDFDoorOptions is not null;

    public override IEnumerable<string> GetNotes() {

        List<string> notes = [
            $"{DoorQty} Doors",
            $"{FalseDrawerQty} False Drawer Fronts",
            $"{AdjustableShelves} Adjustable Shelves",
            $"{(TiltFront ? "Tilt Front" : "No Tilt Front")}",
            $"{RollOutBoxes.Qty} Interior Roll Out Boxes",
        ];

        if (RollOutBoxes.Qty > 0) {
            switch (RollOutBoxes.Blocks) {
                case RollOutBlockPosition.None:
                    notes.Add("No roll out blocks");
                    break;
                case RollOutBlockPosition.Both:
                    notes.Add("Roll out blocks Left & Right");
                    break;
                case RollOutBlockPosition.Left:
                    notes.Add("Roll out blocks Left");
                    break;
                case RollOutBlockPosition.Right:
                    notes.Add("Roll out blocks Right");
                    break;
            }
        }

        if (Scoops is not null) {

            notes.Add($"Side Scoops {Scoops.FromFront.AsInches()}\" From Front, {Scoops.FromBack.AsInches()}\" From Back, {Scoops.Depth.AsInches()}\" Deep");

        }

        return notes;

    }

    public IEnumerable<MDFDoor> GetDoors(Func<MDFDoorBuilder> getBuilder) {

        if (MDFDoorOptions is null) {
            return Enumerable.Empty<MDFDoor>();
        }

        List<MDFDoor> doors = new();

        if (DoorQty > 0) {
            Dimension width = (Width - 2 * DoorGaps.EdgeReveal - DoorGaps.HorizontalGap * (DoorQty - 1)) / DoorQty;
            Dimension height = Height - ToeType.ToeHeight - DoorGaps.TopGap - DoorGaps.BottomGap - (FalseDrawerQty > 0 ? DrawerFaceHeight + DoorGaps.VerticalGap : Dimension.Zero);
            var door = getBuilder().WithQty(DoorQty * Qty)
                                    .WithType(DoorType.Door)
                                    .WithProductNumber(ProductNumber)
                                    .WithFramingBead(MDFDoorOptions.FramingBead)
                                    .WithPaintColor(MDFDoorOptions.PaintColor == "" ? null : MDFDoorOptions.PaintColor)
                                    .Build(height, width);
            doors.Add(door);
        }

        if (FalseDrawerQty > 0) {
            Dimension drwWidth = (Width - 2 * DoorGaps.EdgeReveal - DoorGaps.HorizontalGap * (FalseDrawerQty - 1)) / FalseDrawerQty;
            var drawers = getBuilder().WithQty(FalseDrawerQty * Qty)
                                        .WithType(DoorType.DrawerFront)
                                        .WithProductNumber(ProductNumber)
                                        .WithFramingBead(MDFDoorOptions.FramingBead)
                                        .WithPaintColor(MDFDoorOptions.PaintColor == "" ? null : MDFDoorOptions.PaintColor)
                                        .Build(DrawerFaceHeight, drwWidth);
            doors.Add(drawers);
        }

        return doors.ToArray();

    }

    public bool ContainsDovetailDrawerBoxes() => DrawerBoxOptions is not null && RollOutBoxes.Qty != 0;

    public IEnumerable<DovetailDrawerBox> GetDovetailDrawerBoxes(Func<DovetailDrawerBoxBuilder> getBuilder) {

        if (DrawerBoxOptions is null) {
            return [];
        }

        if (!RollOutBoxes.Positions.Any()) {

            return Enumerable.Empty<DovetailDrawerBox>();

        }

        int rollOutQty = RollOutBoxes.Positions.Length * Qty;
        var boxHeight = Dimension.FromMillimeters(104);

        var box = getBuilder().WithInnerCabinetDepth(InnerDepth, DrawerBoxOptions.SlideType, true)
                                .WithInnerCabinetWidth(InnerWidth, RollOutBoxes.Blocks, DrawerBoxOptions.SlideType)
                                .WithBoxHeight(boxHeight)
                                .WithQty(rollOutQty)
                                .WithOptions(DrawerBoxOptions.GetDrawerBoxOptions())
                                .WithProductNumber(ProductNumber)
                                .Build();

        return [ box ];

    }

    public IEnumerable<Supply> GetSupplies() {

        List<Supply> supplies = [];

        if (AdjustableShelves > 0) {

            supplies.Add(Supply.LockingShelfPeg(AdjustableShelves * 4 * Qty));

        }

        if (DoorQty > 0) {

            // supplies.Add(Supply.DoorPull(DoorQty * Qty));
            supplies.AddRange(Supply.StandardHinge(DoorHeight, DoorQty * Qty));

        }

        if (FalseDrawerQty > 0) {

            // supplies.Add(Supply.DrawerPull(FalseDrawerQty * Qty));

        }

        if (RollOutBoxes.Qty > 0 && DrawerBoxOptions is not null && DrawerBoxOptions.SlideType == DrawerSlideType.UnderMount) {
            
            supplies.Add(Supply.CabinetDrawerClips(RollOutBoxes.Qty * Qty));

        }

        return supplies;

    }

    public IEnumerable<DrawerSlide> GetDrawerSlides() {

        if (DrawerBoxOptions is null) {
            return [];
        }

        List<DrawerSlide> slides = [];

        if (RollOutBoxes.Qty > 0) {

            var boxDepth = DovetailDrawerBoxBuilder.GetDrawerBoxDepthFromInnerCabinetDepth(InnerDepth, DrawerBoxOptions.SlideType, true);
            boxDepth = Dimension.FromMillimeters(Math.Round(boxDepth.AsMillimeters()));

            switch (DrawerBoxOptions.SlideType) {
                case DrawerSlideType.UnderMount:
                    slides.Add(DrawerSlide.UndermountSlide(RollOutBoxes.Qty * Qty, boxDepth));
                    break;

                case DrawerSlideType.SideMount:
                    slides.Add(DrawerSlide.SidemountSlide(RollOutBoxes.Qty * Qty, boxDepth));
                    break;
            }

        }

        return slides;

    }

    public override string GetProductSku() => $"S{DoorQty}D{FalseDrawerQty}FD";

    protected override IDictionary<string, string> GetParameters() {
        var parameters = new Dictionary<string, string>() {
            { "ProductW", Width.AsMillimeters().ToString() },
            { "ProductH", Height.AsMillimeters().ToString() },
            { "ProductD", Depth.AsMillimeters().ToString() },
            { "FinishedLeft", GetSideOption(LeftSideType) },
            { "FinishedRight", GetSideOption(RightSideType) },
            { "ShelfQ", AdjustableShelves.ToString() },
            { "PulloutBlockType", GetRollOutBlockOption() },
            { "AppliedPanel", GetAppliedPanelOption() },
            { "TiltFront", TiltFront ? "1" : "0" },
            { "_SinkScoopYN", Scoops is not null ? "1" : "0" }
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

        if (Scoops is not null) {
            parameters.Add("_SinkScoopFrontD", Scoops.FromFront.AsMillimeters().ToString());
            parameters.Add("_SinkScoopBackD", Scoops.FromBack.AsMillimeters().ToString());
            parameters.Add("_SinkScoopDepth", Scoops.Depth.AsMillimeters().ToString());
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

        if (RollOutBoxes.Positions.Any() && DrawerBoxOptions is not null && DrawerBoxOptions.SlideType == DrawerSlideType.SideMount) {
            parameters.Add("_DrawerRunType", "4");
        }

        if (LeftSideType != CabinetSideType.IntegratedPanel && RightSideType != CabinetSideType.IntegratedPanel && ShelfDepth == ShelfDepth.Full) {

            Dimension backThickness = Dimension.FromMillimeters(13);
            Dimension backInset = Dimension.FromMillimeters(9);
            Dimension shelfFrontClearance = Dimension.FromMillimeters(8);

            parameters.Add("_HalfDepthShelfW", (Depth - backThickness - backInset - shelfFrontClearance).AsMillimeters().ToString());

        }

        return parameters;

    }

    protected override IDictionary<string, string> GetManualOverrideParameters() {

        var parameters = new Dictionary<string, string>();

        if (LeftSideType != CabinetSideType.IntegratedPanel && RightSideType != CabinetSideType.IntegratedPanel) {

            if (ShelfDepth == ShelfDepth.Half) {

                Dimension newWidth = (Depth - Dimension.FromMillimeters(13) - Dimension.FromMillimeters(9)) / 2;

                parameters.Add("_HalfDepthShelfW", newWidth.AsMillimeters().ToString());

            } else if (ShelfDepth == ShelfDepth.ThreeQuarters) {

                Dimension newWidth = (Depth - Dimension.FromMillimeters(13) - Dimension.FromMillimeters(9)) * 0.75;

                parameters.Add("_HalfDepthShelfW", newWidth.AsMillimeters().ToString());

            }

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
