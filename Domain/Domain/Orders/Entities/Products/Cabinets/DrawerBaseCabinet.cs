using Domain.Orders.Builders;
using Domain.Orders.Components;
using Domain.Orders.Entities.Hardware;
using Domain.Orders.Enums;
using Domain.Orders.Exceptions;
using Domain.Orders.ValueObjects;
using Domain.ValueObjects;

namespace Domain.Orders.Entities.Products.Cabinets;

public class DrawerBaseCabinet : GarageCabinet, IMDFDoorContainer, IDovetailDrawerBoxContainer, ISupplyContainer, IDrawerSlideContainer {

    // TODO: add option for no doors

    public ToeType ToeType { get; }
    public VerticalDrawerBank Drawers { get; }
    public CabinetDrawerBoxOptions? DrawerBoxOptions { get; }

    public override string GetDescription() => $"{Drawers.FaceHeights.Length} Drawer {(IsGarage ? "Garage " : "")}Cabinet";
    public override string GetSimpleDescription() => "Drawer Base Cabinet";

    public static CabinetDoorGaps DoorGaps { get; set; } = new() {
        TopGap = Dimension.FromMillimeters(7),
        BottomGap = Dimension.Zero,
        EdgeReveal = Dimension.FromMillimeters(2),
        HorizontalGap = Dimension.FromMillimeters(3),
        VerticalGap = Dimension.FromMillimeters(3),
    };

    public static DrawerBaseCabinet Create(int qty, decimal unitPrice, int productNumber, string room, bool assembled,
                        Dimension height, Dimension width, Dimension depth,
                        CabinetMaterial boxMaterial, CabinetFinishMaterial finishMaterial, CabinetDoorConfiguration doorConfiguration, string edgeBandingColor,
                        CabinetSideType rightSideType, CabinetSideType leftSideType, string comment,
                        ToeType toeType, VerticalDrawerBank drawers, CabinetDrawerBoxOptions? drawerBoxOptions) {
        return new(Guid.NewGuid(), qty, unitPrice, productNumber, room, assembled, height, width, depth, boxMaterial, finishMaterial, doorConfiguration, edgeBandingColor, rightSideType, leftSideType, comment, toeType, drawers, drawerBoxOptions);
    }

    public DrawerBaseCabinet(Guid id, int qty, decimal unitPrice, int productNumber, string room, bool assembled,
                        Dimension height, Dimension width, Dimension depth,
                        CabinetMaterial boxMaterial, CabinetFinishMaterial finishMaterial, CabinetDoorConfiguration doorConfiguration, string edgeBandingColor,
                        CabinetSideType rightSideType, CabinetSideType leftSideType, string comment,
                        ToeType toeType, VerticalDrawerBank drawers, CabinetDrawerBoxOptions? drawerBoxOptions)
                        : base(id, qty, unitPrice, productNumber, room, assembled, height, width, depth, boxMaterial, finishMaterial, doorConfiguration, edgeBandingColor, rightSideType, leftSideType, comment) {

        if (drawers.FaceHeights.Length > 5)
            throw new InvalidProductOptionsException("Invalid number of drawers");

        Drawers = drawers;
        ToeType = toeType;
        DrawerBoxOptions = drawerBoxOptions;
    }

    public bool ContainsDoors() => DoorConfiguration.IsMDF;

    public override IEnumerable<string> GetNotes() {

        var drawerType = DoorConfiguration.Match(
            slab => "Slab Drawers Fronts",
            mdf => "MDF Drawer Fronts",
            byothers => "Drawer Fronts by others");

        return [
            $"{Drawers.Qty} Drawers",
            drawerType
        ];

    }

    public IEnumerable<MDFDoor> GetDoors(Func<MDFDoorBuilder> getBuilder)
        => DoorConfiguration.Match(
            slab => [],
            mdf => {

                List<MDFDoor> doors = new();

                Dimension width = Width - 2 * DoorGaps.EdgeReveal;

                foreach (var height in Drawers.FaceHeights) {

                    var door = getBuilder().WithQty(1 * Qty)
                                            .WithType(DoorType.Door)
                                            .WithProductNumber(ProductNumber)
                                            .WithFramingBead(mdf.FramingBead)
                                            .WithFinish(mdf.Finish)
                                            .Build(height, width);

                    doors.Add(door);

                }

                return doors;

            },
            byothers => []);

	public bool ContainsDovetailDrawerBoxes() => DrawerBoxOptions is not null && Drawers.FaceHeights.Length != 0;

    public IEnumerable<DovetailDrawerBox> GetDovetailDrawerBoxes(Func<DovetailDrawerBoxBuilder> getBuilder) {

        if (DrawerBoxOptions is null) {
            return [];
        }

        if (!Drawers.FaceHeights.Any()) {
            return Enumerable.Empty<DovetailDrawerBox>();
        }

        var boxes = new List<DovetailDrawerBox>();

        for (int i = 0; i < Drawers.FaceHeights.Length; i++) {

            var height = Drawers.FaceHeights[i];

            var verticalClearance = GetVerticalDrawerBoxClearance(i, Drawers.FaceHeights.Length, Construction);

            var box = getBuilder().WithInnerCabinetDepth(InnerDepth, DrawerBoxOptions.SlideType)
                                    .WithInnerCabinetWidth(InnerWidth, 1, DrawerBoxOptions.SlideType)
                                    .WithDrawerFaceHeight(height, verticalClearance)
                                    .WithQty(Qty)
                                    .WithOptions(DrawerBoxOptions.GetDrawerBoxOptions())
                                    .WithProductNumber(ProductNumber)
                                    .Build();

            boxes.Add(box);

        }

        return boxes;

    }

    public static Dimension GetVerticalDrawerBoxClearance(int boxIndex, int boxCount, CabinetConstruction construction) {

        Dimension clrTandemTop = Dimension.FromMillimeters(6);

        Dimension topClearance;
        if (boxIndex == 0) {

            topClearance = construction.TopThickness - DoorGaps.TopGap + clrTandemTop;

        } else {

            topClearance = construction.TopThickness / 2 - DoorGaps.VerticalGap / 2 + clrTandemTop;

        }

        Dimension botClearance;
        if (boxIndex == (boxCount - 1)) {

            botClearance = Dimension.FromMillimeters(19) + construction.BottomThickness + DoorGaps.BottomGap;

        } else {

            botClearance = Dimension.FromMillimeters(15) + construction.TopThickness / 2 + DoorGaps.VerticalGap / 2;

        }

        Dimension verticalClearance = topClearance + botClearance;

        return verticalClearance;

    }

    public IEnumerable<Supply> GetSupplies() {

        List<Supply> supplies = [];

        if (DrawerBoxOptions is not null) {
            // Supply.DrawerPull(Drawers.Qty * Qty),
            supplies.Add(Supply.CabinetDrawerClips(Drawers.Qty * Qty));
        }

        if (ToeType == ToeType.LegLevelers) {

            supplies.Add(Supply.CabinetLeveler(Qty * 4));

        }

        return supplies;

    }

    public IEnumerable<DrawerSlide> GetDrawerSlides() {

        if (DrawerBoxOptions is null) return [];

        var boxDepth = DovetailDrawerBoxBuilder.GetDrawerBoxDepthFromInnerCabinetDepth(InnerDepth, DrawerBoxOptions.SlideType, false);
        boxDepth = Dimension.FromMillimeters(Math.Round(boxDepth.AsMillimeters()));
        return [

            DrawerBoxOptions.SlideType switch {
                DrawerSlideType.SideMount => DrawerSlide.SidemountSlide(Drawers.Qty * Qty, boxDepth),
                DrawerSlideType.UnderMount => DrawerSlide.UndermountSlide(Drawers.Qty * Qty, boxDepth),
                _ => throw new InvalidOperationException("Unknown slide type")
            }

        ];

    }

    public override string GetProductSku() {
        if (!Drawers.FaceHeights.Any()) return $"{(IsGarage ? "G" : "")}DB1D";
        return $"{(IsGarage ? "G" : "")}DB{Drawers.FaceHeights.Length}D";
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

        if (Drawers.FaceHeights.Any() && DrawerBoxOptions is not null && DrawerBoxOptions.SlideType == DrawerSlideType.SideMount) {
            parameters.Add("_DrawerRunType", "4");
        }

        return parameters;

    }

}