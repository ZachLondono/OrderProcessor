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
    public CabinetDrawerBoxOptions DrawerBoxOptions { get; }

    public override string GetDescription() => TrashPulloutConfiguration switch {
        TrashPulloutConfiguration.OneCan => $"One Can Trash Pullout Cabinet",
        TrashPulloutConfiguration.TwoCans => $"Two Can Trash Pullout Cabinet",
        _ => "Trash Pullout Cabinet"
    };


    public static CabinetDoorGaps DoorGaps { get; set; } = new() {
        TopGap = Dimension.FromMillimeters(7),
        BottomGap = Dimension.Zero,
        EdgeReveal = Dimension.FromMillimeters(2),
        HorizontalGap = Dimension.FromMillimeters(3),
        VerticalGap = Dimension.FromMillimeters(3),
    };

    public TrashCabinet(Guid id, int qty, decimal unitPrice, int productNumber, string room, bool assembled,
                        Dimension height, Dimension width, Dimension depth,
                        CabinetMaterial boxMaterial, CabinetFinishMaterial finishMaterial, CabinetSlabDoorMaterial? slabDoorMaterial, MDFDoorOptions? mdfDoorOptions, string edgeBandingColor,
                        CabinetSideType rightSideType, CabinetSideType leftSideType, string comment,
                        Dimension drawerFaceHeight, TrashPulloutConfiguration trashPulloutConfiguration, CabinetDrawerBoxOptions drawerBoxOptions, ToeType toeType)
        : base(id, qty, unitPrice, productNumber, room, assembled, height, width, depth, boxMaterial, finishMaterial, slabDoorMaterial, mdfDoorOptions, edgeBandingColor, rightSideType, leftSideType, comment) {
        DrawerFaceHeight = drawerFaceHeight;
        TrashPulloutConfiguration = trashPulloutConfiguration;
        DrawerBoxOptions = drawerBoxOptions;
        ToeType = toeType;
    }

    public static TrashCabinet Create(int qty, decimal unitPrice, int productNumber, string room, bool assembled,
                        Dimension height, Dimension width, Dimension depth,
                        CabinetMaterial boxMaterial, CabinetFinishMaterial finishMaterial, CabinetSlabDoorMaterial? slabDoorMaterial, MDFDoorOptions? mdfDoorOptions, string edgeBandingColor,
                        CabinetSideType rightSideType, CabinetSideType leftSideType, string comment,
                        Dimension drawerFaceHeight, TrashPulloutConfiguration trashPulloutConfiguration, CabinetDrawerBoxOptions drawerBoxOptions, ToeType toeType)
        => new(Guid.NewGuid(), qty, unitPrice, productNumber, room, assembled, height, width, depth, boxMaterial, finishMaterial, slabDoorMaterial, mdfDoorOptions, edgeBandingColor, rightSideType, leftSideType, comment, drawerFaceHeight, trashPulloutConfiguration, drawerBoxOptions, toeType);

    public bool ContainsDoors() => MDFDoorOptions is not null;

    public IEnumerable<MDFDoor> GetDoors(Func<MDFDoorBuilder> getBuilder) {

        if (MDFDoorOptions is null) {
            return Enumerable.Empty<MDFDoor>();
        }

        List<MDFDoor> doors = new();

        Dimension width = Width - 2 * DoorGaps.EdgeReveal;
        Dimension height = Height - ToeType.ToeHeight - DoorGaps.TopGap - DoorGaps.BottomGap - DrawerFaceHeight - DoorGaps.VerticalGap;
        var door = getBuilder().WithQty(Qty)
                                .WithProductNumber(ProductNumber)
                                .WithType(DoorType.Door)
                                .WithFramingBead(MDFDoorOptions.FramingBead)
                                .WithPaintColor(MDFDoorOptions.PaintColor == "" ? null : MDFDoorOptions.PaintColor)
                                .Build(height, width);
        doors.Add(door);

        var drawers = getBuilder().WithQty(Qty)
                                    .WithProductNumber(ProductNumber)
                                    .WithType(DoorType.DrawerFront)
                                    .WithFramingBead(MDFDoorOptions.FramingBead)
                                    .WithPaintColor(MDFDoorOptions.PaintColor == "" ? null : MDFDoorOptions.PaintColor)
                                    .Build(DrawerFaceHeight, width);
        doors.Add(drawers);

        return doors;

    }

    public bool ContainsDovetailDrawerBoxes() => true;

    public IEnumerable<DovetailDrawerBox> GetDovetailDrawerBoxes(Func<DovetailDrawerBoxBuilder> getBuilder) {

        var box = getBuilder().WithInnerCabinetDepth(InnerDepth, DrawerBoxOptions.SlideType)
                                .WithInnerCabinetWidth(InnerWidth, 1, DrawerBoxOptions.SlideType)
                                .WithDrawerFaceHeight(DrawerFaceHeight)
                                .WithQty(Qty)
                                .WithOptions(DrawerBoxOptions.GetDrawerBoxOptions())
                                .WithProductNumber(ProductNumber)
                                .Build();

        return new DovetailDrawerBox[] { box };

    }

    public IEnumerable<Supply> GetSupplies() {

        List<Supply> supplies = [
            Supply.DoorPull(Qty),
            Supply.DrawerPull(Qty)
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

        return supplies;

    }

    public IEnumerable<DrawerSlide> GetDrawerSlides() {

        List<DrawerSlide> slides = [];

        var boxDepth = DovetailDrawerBoxBuilder.GetDrawerBoxDepthFromInnerCabinetDepth(InnerDepth, DrawerBoxOptions.SlideType);
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

        if (DrawerBoxOptions.SlideType == DrawerSlideType.SideMount) {
            parameters.Add("_DrawerRunType", "4");
        }

        return parameters;

    }

}