using Domain.Orders.Builders;
using Domain.Orders.Components;
using Domain.Orders.Entities.Hardware;
using Domain.Orders.Enums;
using Domain.Orders.ValueObjects;
using Domain.ValueObjects;

namespace Domain.Orders.Entities.Products.Cabinets;

public class BlindBaseCabinet : GarageCabinet, IMDFDoorContainer, IDovetailDrawerBoxContainer, ISupplyContainer, IDrawerSlideContainer {

    public BlindCabinetDoors Doors { get; }
    public HorizontalDrawerBank Drawers { get; }
    public int AdjustableShelves { get; }
    public ShelfDepth ShelfDepth { get; }
    public BlindSide BlindSide { get; }
    public Dimension BlindWidth { get; }
    public ToeType ToeType { get; }
    public CabinetDrawerBoxOptions? DrawerBoxOptions { get; }

    public Dimension DoorHeight => Height - ToeType.ToeHeight - DoorGaps.TopGap - DoorGaps.BottomGap - (Drawers.Quantity > 0 ? Drawers.FaceHeight + DoorGaps.VerticalGap : Dimension.Zero);

    public override string GetDescription() => $"Blind {(IsGarage ? "Garage " : "")}Base Cabinet - {Doors.Quantity} Doors, {Drawers.Quantity} Drawers";
    public override string GetSimpleDescription() => "Blind Base Cabinet";

    public static CabinetDoorGaps DoorGaps { get; set; } = new() {
        TopGap = Dimension.FromMillimeters(7),
        BottomGap = Dimension.Zero,
        EdgeReveal = Dimension.FromMillimeters(2),
        HorizontalGap = Dimension.FromMillimeters(3),
        VerticalGap = Dimension.FromMillimeters(3),
    };

    public static BlindBaseCabinet Create(int qty, decimal unitPrice, int productNumber, string room, bool assembled,
                        Dimension height, Dimension width, Dimension depth,
                        CabinetMaterial boxMaterial, CabinetFinishMaterial finishMaterial, CabinetDoorConfiguration doorConfiguration, string edgeBandingColor,
                        CabinetSideType rightSideType, CabinetSideType leftSideType, string comment,
                        BlindCabinetDoors doors, BlindSide blindSide, Dimension blindWidth, int adjustableShelves, ShelfDepth shelfDepth, HorizontalDrawerBank drawers, ToeType toeType, CabinetDrawerBoxOptions? drawerBoxOptions) {
        return new(Guid.NewGuid(), qty, unitPrice, productNumber, room, assembled, height, width, depth, boxMaterial, finishMaterial, doorConfiguration, edgeBandingColor, rightSideType, leftSideType, comment, doors, blindSide, blindWidth, adjustableShelves, shelfDepth, drawers, toeType, drawerBoxOptions);
    }

    public BlindBaseCabinet(Guid id, int qty, decimal unitPrice, int productNumber, string room, bool assembled,
                        Dimension height, Dimension width, Dimension depth,
                        CabinetMaterial boxMaterial, CabinetFinishMaterial finishMaterial, CabinetDoorConfiguration doorConfiguration, string edgeBandingColor,
                        CabinetSideType rightSideType, CabinetSideType leftSideType, string comment,
                        BlindCabinetDoors doors, BlindSide blindSide, Dimension blindWidth, int adjustableShelves, ShelfDepth shelfDepth, HorizontalDrawerBank drawers, ToeType toeType, CabinetDrawerBoxOptions? drawerBoxOptions)
                        : base(id, qty, unitPrice, productNumber, room, assembled, height, width, depth, boxMaterial, finishMaterial, doorConfiguration, edgeBandingColor, rightSideType, leftSideType, comment) {

        Doors = doors;
        BlindSide = blindSide;
        AdjustableShelves = adjustableShelves;
        ShelfDepth = shelfDepth;
        Drawers = drawers;
        ToeType = toeType;
        BlindWidth = blindWidth;
        DrawerBoxOptions = drawerBoxOptions;

        if (LeftSideType == CabinetSideType.ConfirmatFinished || RightSideType == CabinetSideType.ConfirmatFinished) {
            ProductionNotes.Add("Confirmat finished (Garage finished) sides will not work with non-garage SKUs. Set cabinet sides to unfinished and then change material to the finished material.");
        }

    }

    public override string GetProductSku() => $"BB{Doors.Quantity}D{GetDrawerCountSkuPart()}{GetBlindSideLetter()}";

    public bool ContainsDoors() => DoorConfiguration.IsMDF;

    public override IEnumerable<string> GetNotes() {

        (string doorComment, string dwrComment) = DoorConfiguration.Match(
            slab => (
                $"{Doors.Quantity} Slab Doors",
				$"{Drawers.Quantity} Slab Drawer Fronts"
            ),
            mdf => (
                $"{Doors.Quantity} MDF Doors",
				$"{Drawers.Quantity} MDF Drawer Fronts"
            ),
            byothers => (
                $"{Doors.Quantity} Doors, by Others",
				$"{Drawers.Quantity} Drawer Fronts, by Others"
            ));

        return [
            doorComment,
            dwrComment,
            $"{AdjustableShelves} Adjustable Shelves",
            $"Blind Width: {BlindWidth.AsInches():0.00}\"",
            $"Blind Side: {BlindSide}",
            $"Lazy Susan: None"
        ];

    }

    public IEnumerable<MDFDoor> GetDoors(Func<MDFDoorBuilder> getBuilder)
        => DoorConfiguration.Match(
            slab => [],
            mdf => {

				List<MDFDoor> doors = [];

				if (Doors.Quantity > 0) {
					Dimension width = (Width - BlindWidth - DoorGaps.EdgeReveal - DoorGaps.HorizontalGap / 2 - DoorGaps.HorizontalGap * (Doors.Quantity - 1)) / Doors.Quantity;
					Dimension height = DoorHeight;
					var door = getBuilder().WithQty(Doors.Quantity * Qty)
											.WithProductNumber(ProductNumber)
											.WithType(DoorType.Door)
											.WithFramingBead(mdf.FramingBead)
											.WithPaintColor(mdf.PaintColor == "" ? null : mdf.PaintColor)
											.Build(height, width);
					doors.Add(door);
				}

				if (Drawers.Quantity > 0) {
					Dimension drwWidth = (Width - BlindWidth - DoorGaps.EdgeReveal - DoorGaps.HorizontalGap / 2 - DoorGaps.HorizontalGap * (Drawers.Quantity - 1)) / Drawers.Quantity;
					var drawers = getBuilder().WithQty(Drawers.Quantity * Qty)
												.WithProductNumber(ProductNumber)
												.WithType(DoorType.DrawerFront)
												.WithFramingBead(mdf.FramingBead)
												.WithPaintColor(mdf.PaintColor == "" ? null : mdf.PaintColor)
												.Build(Drawers.FaceHeight, drwWidth);
					doors.Add(drawers);
				}

				return doors;

			},
			byothers => []);
        
    public bool ContainsDovetailDrawerBoxes() => DrawerBoxOptions is not null && Drawers.Any();

    public IEnumerable<DovetailDrawerBox> GetDovetailDrawerBoxes(Func<DovetailDrawerBoxBuilder> getBuilder) {

        if (DrawerBoxOptions is null) return [];

        if (!Drawers.Any()) {
            return Enumerable.Empty<DovetailDrawerBox>();
        }

        int drawerQty = Drawers.Quantity * Qty;

        var box = getBuilder().WithInnerCabinetDepth(InnerDepth, DrawerBoxOptions.SlideType)
                                .WithInnerCabinetWidth(InnerWidth - BlindWidth, Drawers.Quantity, DrawerBoxOptions.SlideType)
                                .WithDrawerFaceHeight(Drawers.FaceHeight)
                                .WithQty(drawerQty)
                                .WithOptions(DrawerBoxOptions.GetDrawerBoxOptions())
                                .WithProductNumber(ProductNumber)
                                .Build();

        return new List<DovetailDrawerBox>() { box };

    }

    public IEnumerable<Supply> GetSupplies() {

        List<Supply> supplies = [];

        if (AdjustableShelves > 0) {

            supplies.Add(Supply.LockingShelfPeg(AdjustableShelves * Qty * 4));

        }

        if (Doors.Quantity > 0) {

            if ((BlindSide == BlindSide.Left && Doors.HingeSide == HingeSide.Left)
                || (BlindSide == BlindSide.Right && Doors.HingeSide == HingeSide.Right)) {

                supplies.AddRange(Supply.BlindCornerHinge(DoorHeight, Qty));

            } else if (Doors.HingeSide == HingeSide.NotApplicable) {

                supplies.AddRange(Supply.BlindCornerHinge(DoorHeight, Qty));
                supplies.AddRange(Supply.StandardHinge(DoorHeight, Qty));

            } else {

                supplies.AddRange(Supply.StandardHinge(DoorHeight, Qty));

            }

        }

        if (Drawers.Quantity > 0) {

            // supplies.Add(Supply.DrawerPull(Drawers.Quantity * Qty));

        }

        if (ToeType == ToeType.LegLevelers) {

            supplies.Add(Supply.CabinetLeveler(Qty * 4));

        }

        if (Drawers.Quantity > 0 && DrawerBoxOptions is not null && DrawerBoxOptions.SlideType == DrawerSlideType.UnderMount) {
            supplies.Add(Supply.CabinetDrawerClips(Drawers.Quantity * Qty));
        }

        return supplies;

    }

    public IEnumerable<DrawerSlide> GetDrawerSlides() {

        if (DrawerBoxOptions is null) {
            return [];
        }

        List<DrawerSlide> slides = [];

        if (Drawers.Quantity > 0) {

            var boxDepth = DovetailDrawerBoxBuilder.GetDrawerBoxDepthFromInnerCabinetDepth(InnerDepth, DrawerBoxOptions.SlideType, false);
            boxDepth = Dimension.FromMillimeters(Math.Round(boxDepth.AsMillimeters()));

            switch (DrawerBoxOptions.SlideType) {
                case DrawerSlideType.UnderMount:
                    slides.Add(DrawerSlide.UndermountSlide(Drawers.Quantity * Qty, boxDepth));
                    break;

                case DrawerSlideType.SideMount:
                    slides.Add(DrawerSlide.SidemountSlide(Drawers.Quantity * Qty, boxDepth));
                    break;
            }

        }

        return slides;

    }

    protected override IDictionary<string, string> GetParameters() {
        var parameters = new Dictionary<string, string>() {
            { "ProductW", Width.AsMillimeters().ToString() },
            { "ProductH", Height.AsMillimeters().ToString() },
            { "ProductD", Depth.AsMillimeters().ToString() },
            { "FinishedLeft", GetSideOption(LeftSideType) },
            { "FinishedRight", GetSideOption(RightSideType) },
            { "ShelfQ", AdjustableShelves.ToString() },
            { "BlindW", BlindWidth.AsMillimeters().ToString() }
        };

        if (Doors.HingeSide != HingeSide.NotApplicable) {
            parameters.Add("HingeLeft", GetHingeSideOption());
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

    private string GetDrawerCountSkuPart() => Drawers.Quantity switch {
        0 => string.Empty,
        1 => "1D",
        2 => "2D",
        _ => throw new ArgumentOutOfRangeException(nameof(Drawers))
    };

    private string GetBlindSideLetter() => BlindSide switch {
        BlindSide.Left => "L",
        BlindSide.Right => "R",
        _ => throw new ArgumentOutOfRangeException(nameof(BlindSide))
    };

    private string GetHingeSideOption() => Doors.HingeSide switch {
        HingeSide.NotApplicable => "0",
        HingeSide.Left => "1",
        HingeSide.Right => "0",
        _ => "0"
    };

}
