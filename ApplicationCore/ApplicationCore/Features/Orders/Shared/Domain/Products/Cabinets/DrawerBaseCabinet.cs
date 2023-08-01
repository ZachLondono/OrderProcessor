using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.Exceptions;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.Products.Cabinets;

internal class DrawerBaseCabinet : Cabinet, IDoorContainer, IDovetailDrawerBoxContainer {

    // TODO: add option for no doors

    public ToeType ToeType { get; }
    public VerticalDrawerBank Drawers { get; }
    public CabinetDrawerBoxOptions DrawerBoxOptions { get; }

    public override string GetDescription() => $"{Drawers.FaceHeights.Length} Drawer Cabinet";

    public static CabinetDoorGaps DoorGaps { get; set; } = new() {
        TopGap = Dimension.FromMillimeters(7),
        BottomGap = Dimension.Zero,
        EdgeReveal = Dimension.FromMillimeters(2),
        HorizontalGap = Dimension.FromMillimeters(3),
        VerticalGap = Dimension.FromMillimeters(3),
    };

    public static DrawerBaseCabinet Create(int qty, decimal unitPrice, int productNumber, string room, bool assembled,
                        Dimension height, Dimension width, Dimension depth,
                        CabinetMaterial boxMaterial, CabinetFinishMaterial finishMaterial, CabinetSlabDoorMaterial? slabDoorMaterial, MDFDoorOptions? mdfDoorOptions, string edgeBandingColor,
                        CabinetSideType rightSideType, CabinetSideType leftSideType, string comment,
                        ToeType toeType, VerticalDrawerBank drawers, CabinetDrawerBoxOptions drawerBoxOptions) {
        return new(Guid.NewGuid(), qty, unitPrice, productNumber, room, assembled, height, width, depth, boxMaterial, finishMaterial, slabDoorMaterial, mdfDoorOptions, edgeBandingColor, rightSideType, leftSideType, comment, toeType, drawers, drawerBoxOptions);
    }

    internal DrawerBaseCabinet(Guid id, int qty, decimal unitPrice, int productNumber, string room, bool assembled,
                        Dimension height, Dimension width, Dimension depth,
                        CabinetMaterial boxMaterial, CabinetFinishMaterial finishMaterial, CabinetSlabDoorMaterial? slabDoorMaterial, MDFDoorOptions? mdfDoorOptions, string edgeBandingColor,
                        CabinetSideType rightSideType, CabinetSideType leftSideType, string comment,
                        ToeType toeType, VerticalDrawerBank drawers, CabinetDrawerBoxOptions drawerBoxOptions)
                        : base(id, qty, unitPrice, productNumber, room, assembled, height, width, depth, boxMaterial, finishMaterial, slabDoorMaterial, mdfDoorOptions, edgeBandingColor, rightSideType, leftSideType, comment) {

        if (drawers.FaceHeights.Count() > 5)
            throw new InvalidProductOptionsException("Invalid number of drawers");

        Drawers = drawers;
        ToeType = toeType;
        DrawerBoxOptions = drawerBoxOptions;
    }

    public bool ContainsDoors() => MDFDoorOptions is not null;

    public IEnumerable<MDFDoor> GetDoors(Func<MDFDoorBuilder> getBuilder) {

        if (MDFDoorOptions is null) {
            return Enumerable.Empty<MDFDoor>();
        }

        List<MDFDoor> doors = new();

        Dimension width = Width - 2 * DoorGaps.EdgeReveal;

        foreach (var height in Drawers.FaceHeights) {

            var door = getBuilder().WithQty(1 * Qty)
                                    .WithType(DoorType.Door)
                                    .WithProductNumber(ProductNumber)
                                    .WithFramingBead(MDFDoorOptions.FramingBead)
                                    .WithPaintColor(MDFDoorOptions.PaintColor == "" ? null : MDFDoorOptions.PaintColor)
                                    .Build(height, width);

            doors.Add(door);

        }

        return doors;

    }

    public bool ContainsDovetailDrawerBoxes() => Drawers.FaceHeights.Any();

    public IEnumerable<DovetailDrawerBox> GetDovetailDrawerBoxes(Func<DovetailDrawerBoxBuilder> getBuilder) {

        if (!Drawers.FaceHeights.Any()) {
            return Enumerable.Empty<DovetailDrawerBox>();
        }

        var boxes = new List<DovetailDrawerBox>();

        foreach (var height in Drawers.FaceHeights) {

            var box = getBuilder().WithInnerCabinetDepth(InnerDepth, DrawerBoxOptions.SlideType)
                                    .WithInnerCabinetWidth(InnerWidth, 1, DrawerBoxOptions.SlideType)
                                    .WithDrawerFaceHeight(height)
                                    .WithQty(Qty)
                                    .WithOptions(DrawerBoxOptions.GetDrawerBoxOptions())
                                    .WithProductNumber(ProductNumber)
                                    .Build();

            boxes.Add(box);

        }

        return boxes;

    }

    public override IEnumerable<Supply> GetSupplies() {

        var boxDepth = DovetailDrawerBoxBuilder.GetDrawerBoxDepthFromInnerCabinetDepth(InnerDepth, DrawerBoxOptions.SlideType, false);

        List<Supply> supplies = new() {

            Supply.DrawerPull(Drawers.Qty * Qty),

            DrawerBoxOptions.SlideType switch {
                DrawerSlideType.SideMount => Supply.SidemountSlide(Drawers.Qty * Qty, boxDepth),
                DrawerSlideType.UnderMount => Supply.UndermountSlide(Drawers.Qty * Qty, boxDepth),
                _ => throw new InvalidOperationException("Unknown slide type")
            }

        };

        if (ToeType == ToeType.LegLevelers) {

            supplies.Add(Supply.CabinetLeveler(Qty * 4));

        }

        return supplies;

    }

    protected override string GetProductSku() {
        if (!Drawers.FaceHeights.Any()) return "DB1D";
        return $"DB{Drawers.FaceHeights.Length}D";
    }

    protected override IDictionary<string, string> GetParameters() {
        var parameters = new Dictionary<string, string>() {
            { "ProductW", Width.AsMillimeters().ToString() },
            { "ProductH", Height.AsMillimeters().ToString() },
            { "ProductD", Depth.AsMillimeters().ToString() },
            { "FinishedLeft", GetSideOption(LeftSideType) },
            { "FinishedRight", GetSideOption(RightSideType) },
            { "AppliedPanel", GetAppliedPanelOption() },
        };

        int index = 1;
        foreach (var height in Drawers.FaceHeights) {
            if (index == 5) break; // in 5 drawer cabinets, the fifth drawer must be calculated by PSI
            parameters.Add($"DrawerH{index++}", height.AsMillimeters().ToString());
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

        if (Drawers.FaceHeights.Any() && DrawerBoxOptions.SlideType == DrawerSlideType.SideMount) {
            parameters.Add("_DrawerRunType", "4");
        }

        return parameters;

    }

}