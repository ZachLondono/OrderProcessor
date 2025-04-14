using Domain.Orders.Builders;
using Domain.Orders.Components;
using Domain.Orders.Entities.Hardware;
using Domain.Orders.Enums;
using Domain.Orders.Exceptions;
using Domain.Orders.ValueObjects;
using Domain.ValueObjects;

namespace Domain.Orders.Entities.Products.Cabinets;

public class TallCabinet : GarageCabinet, IMDFDoorContainer, IDovetailDrawerBoxContainer, ISupplyContainer, IDrawerSlideContainer {

    public TallCabinetDoors Doors { get; }
    public ToeType ToeType { get; }
    public TallCabinetInside Inside { get; }
    public CabinetDrawerBoxOptions? DrawerBoxOptions { get; }
    public CabinetBaseNotch? BaseNotch { get; }

    public Dimension LowerDoorHeight => Doors.UpperQuantity > 0 ? Doors.LowerDoorHeight : Height - ToeType.ToeHeight - DoorGaps.TopGap - DoorGaps.BottomGap;
    public Dimension UpperDoorHeight => Height - ToeType.ToeHeight - DoorGaps.TopGap - DoorGaps.BottomGap - Doors.LowerDoorHeight - DoorGaps.VerticalGap;

    public override string GetDescription()
        => $"Tall {(IsGarage ? "Garage " : "")}Cabinet - {(Doors.UpperQuantity > 0 ? $"{Doors.UpperQuantity} Upper Doors, {Doors.LowerQuantity} Lower Doors" : $"{Doors.LowerQuantity} Full Height Doors")}{(Inside.RollOutBoxes.Any() ? $", {Inside.RollOutBoxes.Qty} Roll Out Drawers" : "")}";

    public override string GetSimpleDescription() => "Tall Cabinet";

    public static CabinetDoorGaps DoorGaps { get; set; } = new() {
        TopGap = Dimension.FromMillimeters(3),
        BottomGap = Dimension.Zero,
        EdgeReveal = Dimension.FromMillimeters(2),
        HorizontalGap = Dimension.FromMillimeters(3),
        VerticalGap = Dimension.FromMillimeters(3),
    };

    public static TallCabinet Create(int qty, decimal unitPrice, int productNumber, string room, bool assembled,
                        Dimension height, Dimension width, Dimension depth,
                        CabinetMaterial boxMaterial, CabinetFinishMaterial finishMaterial, CabinetDoorConfiguration doorConfiguration, string edgeBandingColor,
                        CabinetSideType rightSideType, CabinetSideType leftSideType, string comment,
                        TallCabinetDoors doors, ToeType toeType, TallCabinetInside inside, CabinetDrawerBoxOptions? drawerBoxOptions, CabinetBaseNotch? baseNotch) {
        return new(Guid.NewGuid(), qty, unitPrice, productNumber, room, assembled, height, width, depth, boxMaterial, finishMaterial, doorConfiguration, edgeBandingColor, rightSideType, leftSideType, comment, doors, toeType, inside, drawerBoxOptions, baseNotch);
    }

    public TallCabinet(Guid id, int qty, decimal unitPrice, int productNumber, string room, bool assembled,
                        Dimension height, Dimension width, Dimension depth,
                        CabinetMaterial boxMaterial, CabinetFinishMaterial finishMaterial, CabinetDoorConfiguration doorConfiguration, string edgeBandingColor,
                        CabinetSideType rightSideType, CabinetSideType leftSideType, string comment,
                        TallCabinetDoors doors, ToeType toeType, TallCabinetInside inside, CabinetDrawerBoxOptions? drawerBoxOptions, CabinetBaseNotch? baseNotch)
                        : base(id, qty, unitPrice, productNumber, room, assembled, height, width, depth, boxMaterial, finishMaterial, doorConfiguration, edgeBandingColor, rightSideType, leftSideType, comment) {

        if (doors.UpperQuantity > 2 || doors.UpperQuantity < 0 || doors.LowerQuantity > 2 || doors.LowerQuantity < 0)
            throw new InvalidProductOptionsException("Invalid number of doors");

        foreach (var position in inside.RollOutBoxes.Positions) {
            if (position < Dimension.Zero || position > height) {
                throw new InvalidProductOptionsException("Roll out box position {position} is invalid for cabinet size");
            }
        }

        Doors = doors;
        ToeType = toeType;
        Inside = inside;
        DrawerBoxOptions = drawerBoxOptions;
        BaseNotch = baseNotch;

        if (BaseNotch is not null && BaseNotch.Height != Dimension.Zero && BaseNotch.Depth != Dimension.Zero) {
            ProductionNotes.Add("Check back panel length. PSI bug causes back panel to be too short.");
        }

        if (Inside.RollOutBoxes.Positions.Length > 0 && DrawerBoxOptions is not null && DrawerBoxOptions.SlideType == DrawerSlideType.SideMount) {
            ProductionNotes.Add("PSI may not support roll out drawer boxes with side mount slieds");
        }

    }

    public bool ContainsDoors() => DoorConfiguration.IsMDF;

    public override IEnumerable<string> GetNotes() {

        List<string> notes = [
            $"{Doors.LowerQuantity} Lower Doors",
            $"{Doors.UpperQuantity} Upper Doors",
        ];

        if (Doors.UpperQuantity > 0) {
            notes.Add($"{Doors.LowerDoorHeight.AsInches():0.00}\" Lower Door Height");
        }

        notes.Add($"{Inside.AdjustableShelvesLower} Adjustable Shelves, Lower");
        notes.Add($"{Inside.AdjustableShelvesUpper} Adjustable Shelves, Upper");
        notes.Add($"{Inside.VerticalDividersLower} Vertical Dividers, Lower");
        notes.Add($"{Inside.VerticalDividersUpper} Vertical Dividers, Upper");

        notes.Add($"{Inside.RollOutBoxes.Qty} Interior Roll Out Boxes");
        if (Inside.RollOutBoxes.Qty > 0) {
            switch (Inside.RollOutBoxes.Blocks) {
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
        
        if (BaseNotch is not null) {
            var notchHeight = Math.Round(BaseNotch.Height.AsInches(), 2);
            var notchDepth = Math.Round(BaseNotch.Depth.AsInches(), 2);
            notes.Add($"Base Notch: {notchHeight}\"H x {notchDepth}\"D");
        }

        return notes;

    }

    public IEnumerable<MDFDoor> GetDoors(Func<MDFDoorBuilder> getBuilder)
        => DoorConfiguration.Match(
            slab => [],
            mdf => {

				List<MDFDoor> doors = new();

				if (Doors.UpperQuantity > 0) {

					var builder = getBuilder();

					Dimension width = (Width - 2 * DoorGaps.EdgeReveal - DoorGaps.HorizontalGap * (Doors.UpperQuantity - 1)) / Doors.UpperQuantity;
					Dimension height = UpperDoorHeight;

					doors.Add(builder.WithQty(Doors.UpperQuantity * Qty)
									.WithProductNumber(ProductNumber)
									.WithFramingBead(mdf.FramingBead)
									.WithPaintColor(mdf.PaintColor == "" ? null : mdf.PaintColor)
									.Build(height, width));

				}

				if (Doors.LowerQuantity > 0) {

					var builder = getBuilder();

					Dimension height = LowerDoorHeight;
					Dimension width = (Width - 2 * DoorGaps.EdgeReveal - DoorGaps.HorizontalGap * (Doors.LowerQuantity - 1)) / Doors.LowerQuantity;

					doors.Add(builder.WithQty(Doors.LowerQuantity * Qty)
									.WithProductNumber(ProductNumber)
									.WithFramingBead(mdf.FramingBead)
									.WithPaintColor(mdf.PaintColor == "" ? null : mdf.PaintColor)
									.Build(height, width));

				}

				return doors;

			},
			byothers => []);

    public bool ContainsDovetailDrawerBoxes() => DrawerBoxOptions is not null && Inside.RollOutBoxes.Any();

    public IEnumerable<DovetailDrawerBox> GetDovetailDrawerBoxes(Func<DovetailDrawerBoxBuilder> getBuilder) {

        if (DrawerBoxOptions is null) {
            return [];
        }

        if (!Inside.RollOutBoxes.Any()) {

            return [];

        }

        int rollOutQty = Inside.RollOutBoxes.Positions.Length * Qty;
        var boxHeight = Dimension.FromMillimeters(104);

        var box = getBuilder().WithInnerCabinetDepth(InnerDepth, DrawerBoxOptions.SlideType, true)
                                .WithInnerCabinetWidth(InnerWidth, Inside.RollOutBoxes.Blocks, DrawerBoxOptions.SlideType)
                                .WithBoxHeight(boxHeight)
                                .WithQty(rollOutQty)
                                .WithOptions(DrawerBoxOptions.GetDrawerBoxOptions())
                                .WithProductNumber(ProductNumber)
                                .Build();

        return [ box ];

    }

    public IEnumerable<Supply> GetSupplies() {

        List<Supply> supplies = [];

        if (ToeType == ToeType.LegLevelers) {

            supplies.Add(Supply.CabinetLeveler(Qty * 4));

        }

        if (Doors.LowerQuantity > 0) {

            // supplies.Add(Supply.DoorPull(Doors.LowerQuantity * Qty));
            supplies.AddRange(Supply.FullOverlayHinge(LowerDoorHeight, Doors.LowerQuantity * Qty));

        }

        if (Doors.UpperQuantity > 0) {

            // supplies.Add(Supply.DoorPull(Doors.UpperQuantity * Qty));
            supplies.AddRange(Supply.FullOverlayHinge(UpperDoorHeight, Doors.UpperQuantity * Qty));

        }

        if (Inside.AdjustableShelvesUpper > 0 || Inside.AdjustableShelvesLower > 0) {

            supplies.Add(Supply.LockingShelfPeg((Inside.AdjustableShelvesUpper + Inside.AdjustableShelvesLower) * Qty * 4));

        }

        if (Inside.RollOutBoxes.Qty > 0) {

            switch (Inside.RollOutBoxes.Blocks) {
                case RollOutBlockPosition.Left:
                case RollOutBlockPosition.Right:
                    supplies.Add(Supply.PullOutBlock(Inside.RollOutBoxes.Qty * Qty * 2));
                    break;
                case RollOutBlockPosition.Both:
                    supplies.Add(Supply.PullOutBlock(Inside.RollOutBoxes.Qty * Qty * 2 * 2));
                    break;
            }

            if (DrawerBoxOptions is not null && DrawerBoxOptions.SlideType == DrawerSlideType.UnderMount) {
                // supplies.Add(Supply.DrawerPull(Inside.RollOutBoxes.Qty * Qty));
            }

        }

        return supplies;

    }

    public IEnumerable<DrawerSlide> GetDrawerSlides() {

        if (DrawerBoxOptions is null) {
            return [];
        }

        List<DrawerSlide> slides = [];

        if (Inside.RollOutBoxes.Qty > 0) {

            var depth = DovetailDrawerBoxBuilder.GetDrawerBoxDepthFromInnerCabinetDepth(InnerDepth, DrawerBoxOptions.SlideType, true);
            depth = Dimension.FromMillimeters(Math.Round(depth.AsMillimeters()));
    
            switch (DrawerBoxOptions.SlideType) {
    
                case DrawerSlideType.UnderMount:
                    slides.Add(DrawerSlide.UndermountSlide(Inside.RollOutBoxes.Qty * Qty, depth));
                    break;
    
                case DrawerSlideType.SideMount:
                    slides.Add(DrawerSlide.SidemountSlide(Inside.RollOutBoxes.Qty * Qty, depth));
                    break;
    
            }

        }

        return slides;

    }

    public override string GetProductSku() {
        string name = $"{(IsGarage ? "G" : "")}T{Doors.LowerQuantity + Doors.UpperQuantity}D";
        if (Doors.UpperQuantity != 0) name += "2S";
        return name;
    }

    protected override IDictionary<string, string> GetParameters() {
        var parameters = new Dictionary<string, string>() {
            { "ProductW", Width.AsMillimeters().ToString() },
            { "ProductH", Height.AsMillimeters().ToString() },
            { "ProductD", Depth.AsMillimeters().ToString() },
            { "FinishedLeft", GetSideOption(LeftSideType) },
            { "FinishedRight", GetSideOption(RightSideType) },
            { "ShelfQ", Inside.AdjustableShelvesLower.ToString() },
            { "ShelfQUpSect", Inside.AdjustableShelvesUpper.ToString() },
            { "DividerQ", Inside.VerticalDividersLower.ToString() },
            { "DividerQUp", Inside.VerticalDividersLower.ToString() },
            { "DoorHTallBot", Doors.LowerDoorHeight.AsMillimeters().ToString() },
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

        if (Inside.RollOutBoxes.Positions.Length == 0 && DrawerBoxOptions is not null && DrawerBoxOptions.SlideType == DrawerSlideType.SideMount) {
            parameters.Add("_DrawerRunType", "4");
        }

        if (BaseNotch is not null) {
            parameters.Add("_BottomNotchD", BaseNotch.Depth.AsMillimeters().ToString());
            parameters.Add("_BottomNotchH", BaseNotch.Height.AsMillimeters().ToString());
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