using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.Products;

internal class BasePieCutCornerCabinet : Cabinet, IDoorContainer {

    public Dimension RightWidth { get; }
    public Dimension RightDepth { get; }
    public ToeType ToeType { get; }
    public int AdjustableShelves { get; }
    public HingeSide HingeSide { get; }

    public override string GetDescription() => "Pie Cut Corner Base Cabinet";

    public Dimension DoorHeight => Height - ToeType.HeightAdjustment - DoorGaps.TopGap;

    public static CabinetDoorGaps DoorGaps { get; set; } = new() {
        TopGap = Dimension.FromMillimeters(7),
        BottomGap = Dimension.Zero,
        EdgeReveal = Dimension.FromMillimeters(2),
        HorizontalGap = Dimension.FromMillimeters(3),
        VerticalGap = Dimension.FromMillimeters(3),
    };

    internal BasePieCutCornerCabinet(Guid id, int qty, decimal unitPrice, int productNumber, string room, bool assembled,
                        Dimension height, Dimension width, Dimension depth,
                        CabinetMaterial boxMaterial, CabinetFinishMaterial finishMaterial, CabinetSlabDoorMaterial? slabDoorMaterial, MDFDoorOptions? mdfDoorOptions, string edgeBandingColor,
                        CabinetSideType rightSideType, CabinetSideType leftSideType, string comment,
                        Dimension rightWidth, Dimension rightDepth, ToeType toeType, int adjustableShelves, HingeSide hingeSide)
                        : base(id, qty, unitPrice, productNumber, room, assembled, height, width, depth, boxMaterial, finishMaterial, slabDoorMaterial, mdfDoorOptions, edgeBandingColor, rightSideType, leftSideType, comment) {
        RightWidth = rightWidth;
        RightDepth = rightDepth;
        ToeType = toeType;
        AdjustableShelves = adjustableShelves;
        HingeSide = hingeSide;
    }

    public static BasePieCutCornerCabinet Create(int qty, decimal unitPrice, int productNumber, string room, bool assembled,
                        Dimension height, Dimension width, Dimension depth,
                        CabinetMaterial boxMaterial, CabinetFinishMaterial finishMaterial, CabinetSlabDoorMaterial? slabDoorMaterial, MDFDoorOptions? mdfDoorOptions, string edgeBandingColor,
                        CabinetSideType rightSideType, CabinetSideType leftSideType, string comment,
                        Dimension rightWidth, Dimension rightDepth, ToeType toeType, int adjustableShelves, HingeSide hingeSide)
    => new(Guid.NewGuid(), qty, unitPrice, productNumber, room, assembled, height, width, depth, boxMaterial, finishMaterial, slabDoorMaterial, mdfDoorOptions, edgeBandingColor, rightSideType, leftSideType, comment, rightWidth, rightDepth, toeType, adjustableShelves, hingeSide);

    public IEnumerable<MDFDoor> GetDoors(Func<MDFDoorBuilder> getBuilder) {

        if (MDFDoorOptions is null) {
            return Enumerable.Empty<MDFDoor>();
        }

        Dimension height = DoorHeight;
        Dimension doorThickness = Dimension.FromMillimeters(19);
        Dimension bumperWidth = Dimension.FromMillimeters(3);

        Dimension leftWidth = Width - RightDepth - bumperWidth - doorThickness - DoorGaps.EdgeReveal;
        MDFDoor leftDoor = getBuilder().WithQty(Qty)
                                        .WithProductNumber(ProductNumber)
                                        .WithFramingBead(MDFDoorOptions.FramingBead)
                                        .WithPaintColor(MDFDoorOptions.PaintColor == "" ? null : MDFDoorOptions.PaintColor)
                                        .Build(height, leftWidth);

        Dimension rightWidth = RightWidth - Depth - bumperWidth - doorThickness - DoorGaps.EdgeReveal;
        MDFDoor rightDoor = getBuilder().WithQty(Qty)
                                        .WithProductNumber(ProductNumber)
                                        .WithFramingBead(MDFDoorOptions.FramingBead)
                                        .WithPaintColor(MDFDoorOptions.PaintColor == "" ? null : MDFDoorOptions.PaintColor)
                                        .Build(height, rightWidth);

        return new List<MDFDoor>() { leftDoor, rightDoor };

    }

    public override IEnumerable<Supply> GetSupplies() {

        List<Supply> supplies = new();

        if (ToeType == ToeType.LegLevelers) {

            supplies.Add(Supply.CabinetLeveler(Qty * 5));

        }

        if (AdjustableShelves > 0) {

            supplies.Add(Supply.LockingShelfPeg(AdjustableShelves * 5 * Qty));

        }

        supplies.Add(Supply.DoorPull(Qty));

        supplies.AddRange(Supply.StandardHinge(DoorHeight, Qty));

        //supplies.Add(Supply.LazySusan(Qty));

        return supplies;

    }

    protected override string GetProductSku() => "BCPC";

    protected override IDictionary<string, string> GetParameters() {
        var parameters = new Dictionary<string, string>() {
            { "ProductW", Width.AsMillimeters().ToString() },
            { "ProductWRight", RightWidth.AsMillimeters().ToString() },
            { "ProductH", Height.AsMillimeters().ToString() },
            { "ProductD", Depth.AsMillimeters().ToString() },
            { "ProductDRight", RightDepth.AsMillimeters().ToString() },
            { "FinishedLeft", GetSideOption(LeftSideType) },
            { "FinishedRight", GetSideOption(RightSideType) },
            { "ShelfQ", AdjustableShelves.ToString() },
            { "AppliedPanel", GetAppliedPanelOption() },
        };

        if (HingeSide != HingeSide.NotApplicable) {
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

        return parameters;

    }

    private string GetHingeSideOption() => HingeSide switch {
        HingeSide.NotApplicable => "0",
        HingeSide.Left => "1",
        HingeSide.Right => "0",
        _ => "0"
    };

}
