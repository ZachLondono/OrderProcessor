using Domain.Orders.Builders;
using Domain.Orders.Components;
using Domain.Orders.Entities.Hardware;
using Domain.Orders.Enums;
using Domain.Orders.ValueObjects;
using Domain.ValueObjects;

namespace Domain.Orders.Entities.Products.Cabinets;

public class TrashCabinet : Cabinet, IMDFDoorContainer, IDovetailDrawerBoxContainer, ISupplyContainer, IDrawerSlideContainer {

    public Dimension DrawerFaceHeight { get; set; }
    public TrashPulloutConfiguration TrashPulloutConfiguration { get; set; }
    public ToeType ToeType { get; }
    public CabinetDrawerBoxOptions? DrawerBoxOptions { get; }

    public override string GetDescription() => TrashPulloutConfiguration switch {
        TrashPulloutConfiguration.OneCan => $"One Can Trash Pullout Cabinet",
        TrashPulloutConfiguration.TwoCans => $"Two Can Trash Pullout Cabinet",
        _ => "Trash Pullout Cabinet"
    };

    public override string GetSimpleDescription() => "Trash Cabinet";

    public static CabinetDoorGaps DoorGaps { get; set; } = new() {
        TopGap = Dimension.FromMillimeters(7),
        BottomGap = Dimension.Zero,
        EdgeReveal = Dimension.FromMillimeters(2),
        HorizontalGap = Dimension.FromMillimeters(3),
        VerticalGap = Dimension.FromMillimeters(3),
    };

    public TrashCabinet(Guid id, int qty, decimal unitPrice, int productNumber, string room, bool assembled,
                        Dimension height, Dimension width, Dimension depth,
                        CabinetMaterial boxMaterial, CabinetFinishMaterial finishMaterial, CabinetDoorConfiguration doorConfiguration, string edgeBandingColor,
                        CabinetSideType rightSideType, CabinetSideType leftSideType, string comment,
                        Dimension drawerFaceHeight, TrashPulloutConfiguration trashPulloutConfiguration, CabinetDrawerBoxOptions? drawerBoxOptions, ToeType toeType)
        : base(id, qty, unitPrice, productNumber, room, assembled, height, width, depth, boxMaterial, finishMaterial, doorConfiguration, edgeBandingColor, rightSideType, leftSideType, comment) {
        DrawerFaceHeight = drawerFaceHeight;
        TrashPulloutConfiguration = trashPulloutConfiguration;
        DrawerBoxOptions = drawerBoxOptions;
        ToeType = toeType;
    }

    public static TrashCabinet Create(int qty, decimal unitPrice, int productNumber, string room, bool assembled,
                        Dimension height, Dimension width, Dimension depth,
                        CabinetMaterial boxMaterial, CabinetFinishMaterial finishMaterial, CabinetDoorConfiguration doorConfiguration, string edgeBandingColor,
                        CabinetSideType rightSideType, CabinetSideType leftSideType, string comment,
                        Dimension drawerFaceHeight, TrashPulloutConfiguration trashPulloutConfiguration, CabinetDrawerBoxOptions? drawerBoxOptions, ToeType toeType)
        => new(Guid.NewGuid(), qty, unitPrice, productNumber, room, assembled, height, width, depth, boxMaterial, finishMaterial, doorConfiguration, edgeBandingColor, rightSideType, leftSideType, comment, drawerFaceHeight, trashPulloutConfiguration, drawerBoxOptions, toeType);

    public bool ContainsDoors() => DoorConfiguration.IsMDF;

    public override IEnumerable<string> GetNotes() {

        List<string> notes = [];

        switch (TrashPulloutConfiguration) {
            case TrashPulloutConfiguration.OneCan:
                notes.Add("One trash can");
                break;
            case TrashPulloutConfiguration.TwoCans:
                notes.Add("Two trash can");
                break;
        }

        return notes;

    }

    public IEnumerable<MDFDoor> GetDoors(Func<MDFDoorBuilder> getBuilder)
        => DoorConfiguration.Match(
            slab => [],
            mdf => {

				List<MDFDoor> doors = [];

				Dimension width = Width - 2 * DoorGaps.EdgeReveal;
				Dimension height = Height - ToeType.ToeHeight - DoorGaps.TopGap - DoorGaps.BottomGap - DrawerFaceHeight - DoorGaps.VerticalGap;
				var door = getBuilder().WithQty(Qty)
										.WithProductNumber(ProductNumber)
										.WithType(DoorType.Door)
										.WithFramingBead(mdf.FramingBead)
                                        .WithFinish(mdf.Finish)
                                        .Build(height, width);
				doors.Add(door);

				var drawers = getBuilder().WithQty(Qty)
											.WithProductNumber(ProductNumber)
											.WithType(DoorType.DrawerFront)
											.WithFramingBead(mdf.FramingBead)
                                            .WithFinish(mdf.Finish)
                                            .Build(DrawerFaceHeight, width);
				doors.Add(drawers);

				return doors;

			},
			byothers => []);

	public bool ContainsDovetailDrawerBoxes() => DrawerBoxOptions is not null;

    public IEnumerable<DovetailDrawerBox> GetDovetailDrawerBoxes(Func<DovetailDrawerBoxBuilder> getBuilder) {

        if (DrawerBoxOptions is null) {
            return [];
        }

        var box = getBuilder().WithInnerCabinetDepth(InnerDepth, DrawerBoxOptions.SlideType)
                                .WithInnerCabinetWidth(InnerWidth, 1, DrawerBoxOptions.SlideType)
                                .WithDrawerFaceHeight(DrawerFaceHeight)
                                .WithQty(Qty)
                                .WithOptions(DrawerBoxOptions.GetDrawerBoxOptions())
                                .WithProductNumber(ProductNumber)
                                .Build();

        return [ box ];

    }

    public IEnumerable<Supply> GetSupplies() {

        List<Supply> supplies = [
            // Supply.DoorPull(Qty),
            // Supply.DrawerPull(Qty)
        ];

        if (ToeType == ToeType.LegLevelers) {

            supplies.Add(Supply.CabinetLeveler(4 * Qty));

        }

        switch (TrashPulloutConfiguration) {
            case TrashPulloutConfiguration.OneCan:
                supplies.Add(Supply.SingleTrashPullout(Qty));
                break;

            case TrashPulloutConfiguration.TwoCans:
                supplies.Add(Supply.DoubleTrashPullout(Qty));
                break;
        }

        if (DrawerBoxOptions is not null && DrawerBoxOptions.SlideType == DrawerSlideType.UnderMount) {
            supplies.Add(Supply.CabinetDrawerClips(Qty));
        }

        return supplies;

    }

    public IEnumerable<DrawerSlide> GetDrawerSlides() {

        if (DrawerBoxOptions is null) {
            return [];
        }

        List<DrawerSlide> slides = [];

        var boxDepth = DovetailDrawerBoxBuilder.GetDrawerBoxDepthFromInnerCabinetDepth(InnerDepth, DrawerBoxOptions.SlideType);
        boxDepth = Dimension.FromMillimeters(Math.Round(boxDepth.AsMillimeters()));
        switch (DrawerBoxOptions.SlideType) {
            case DrawerSlideType.UnderMount:
                slides.Add(DrawerSlide.UndermountSlide(Qty, boxDepth));
                break;

            case DrawerSlideType.SideMount:
                slides.Add(DrawerSlide.SidemountSlide(Qty, boxDepth));
                break;
        }

        return slides;

    }

    public override string GetProductSku() => "BT1D1D";

    protected override IDictionary<string, string> GetParameters() {
        var parameters = new Dictionary<string, string> {
            ["ProductW"] = Width.AsMillimeters().ToString(),
            ["ProductH"] = Height.AsMillimeters().ToString(),
            ["ProductD"] = Depth.AsMillimeters().ToString(),
            ["FinishedLeft"] = GetSideOption(LeftSideType),
            ["FinishedRight"] = GetSideOption(RightSideType),
            ["AppliedPanel"] = GetAppliedPanelOption(),
            ["DrawerH1"] = DrawerFaceHeight.AsMillimeters().ToString(),
            ["TrashType"] = TrashPulloutConfiguration switch {
                TrashPulloutConfiguration.OneCan => "1",
                TrashPulloutConfiguration.TwoCans => "2",
                _ => "0"
            }
        };

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

        if (DrawerBoxOptions is not null && DrawerBoxOptions.SlideType == DrawerSlideType.SideMount) {
            parameters.Add("_DrawerRunType", "4");
        }

        return parameters;

    }

}