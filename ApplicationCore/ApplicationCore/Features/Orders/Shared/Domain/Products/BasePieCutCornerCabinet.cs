using ApplicationCore.Features.Orders.Details.OrderExport.Handlers.ExtExport.Contracts;
using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.Products;

internal class BasePieCutCornerCabinet : Cabinet, IPPProductContainer, IDoorContainer {

    public Dimension RightWidth { get; }
    public Dimension RightDepth { get; }
    public IToeType ToeType { get; }
    public int AdjustableShelves { get; }
    public HingeSide HingeSide { get; }

    public override string Description => "Pie Cut Corner Base Cabinet";

    public Dimension DoorHeight => Height - ToeType.HeightAdjustment - DoorGaps.TopGap;

    public static CabinetDoorGaps DoorGaps { get; set; } = new() {
        TopGap = Dimension.FromMillimeters(7),
        BottomGap = Dimension.Zero,
        EdgeReveal = Dimension.FromMillimeters(2),
        HorizontalGap = Dimension.FromMillimeters(3),
        VerticalGap = Dimension.FromMillimeters(3),
    };

    public BasePieCutCornerCabinet(Guid id, int qty, decimal unitPrice, int productNumber, string room, bool assembled,
                        Dimension height, Dimension width, Dimension depth,
                        CabinetMaterial boxMaterial, CabinetFinishMaterial finishMaterial, MDFDoorOptions? mdfDoorOptions, string edgeBandingColor,
                        CabinetSide rightSide, CabinetSide leftSide, string comment,
                        Dimension rightWidth, Dimension rightDepth, IToeType toeType, int adjustableShelves, HingeSide hingeSide)
                        : base(id, qty, unitPrice, productNumber, room, assembled, height, width, depth, boxMaterial, finishMaterial, mdfDoorOptions, edgeBandingColor, rightSide, leftSide, comment) {
        RightWidth = rightWidth;
        RightDepth = rightDepth;
        ToeType = toeType;
        AdjustableShelves = adjustableShelves;
        HingeSide = hingeSide;
    }

    public static BasePieCutCornerCabinet Create(int qty, decimal unitPrice, int productNumber, string room, bool assembled,
                        Dimension height, Dimension width, Dimension depth,
                        CabinetMaterial boxMaterial, CabinetFinishMaterial finishMaterial, MDFDoorOptions? mdfDoorOptions, string edgeBandingColor,
                        CabinetSide rightSide, CabinetSide leftSide, string comment,
                        Dimension rightWidth, Dimension rightDepth, IToeType toeType, int adjustableShelves, HingeSide hingeSide)
    => new(Guid.NewGuid(), qty, unitPrice, productNumber, room, assembled, height, width, depth, boxMaterial, finishMaterial, mdfDoorOptions, edgeBandingColor, rightSide, leftSide, comment, rightWidth, rightDepth, toeType, adjustableShelves, hingeSide);

    public IEnumerable<PPProduct> GetPPProducts() {
        string doorType = (MDFDoorOptions is null) ? "Slab" : "Buyout";
        yield return new PPProduct(Id, Room, "BCPC", ProductNumber, "Royal2", GetMaterialType(), doorType, "Standard", Comment, GetFinishMaterials(), GetEBMaterials(), GetParameters(), GetOverrideParameters(), new Dictionary<string, string>());
    }

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
                                        .WithFramingBead(MDFDoorOptions.StyleName)
                                        .WithPaintColor(MDFDoorOptions.Color == "" ? null : MDFDoorOptions.Color)
                                        .Build(height, leftWidth);

        Dimension rightWidth = RightWidth - Depth - bumperWidth - doorThickness - DoorGaps.EdgeReveal;
        MDFDoor rightDoor = getBuilder().WithQty(Qty)
                                        .WithProductNumber(ProductNumber)
                                        .WithFramingBead(MDFDoorOptions.StyleName)
                                        .WithPaintColor(MDFDoorOptions.Color == "" ? null : MDFDoorOptions.Color)
                                        .Build(height, rightWidth);

        return new List<MDFDoor>() { leftDoor, rightDoor };

    }

    public override IEnumerable<Supply> GetSupplies() {

        List<Supply> supplies = new();

        if (ToeType is LegLevelers) {

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

    private Dictionary<string, string> GetParameters() {
        var parameters = new Dictionary<string, string>() {
            { "ProductW", Width.AsMillimeters().ToString() },
            { "ProductWRight", RightWidth.AsMillimeters().ToString() },
            { "ProductH", Height.AsMillimeters().ToString() },
            { "ProductD", Depth.AsMillimeters().ToString() },
            { "ProductDRight", RightDepth.AsMillimeters().ToString() },
            { "FinishedLeft", GetSideOption(LeftSide.Type) },
            { "FinishedRight", GetSideOption(RightSide.Type) },
            { "ShelfQ", AdjustableShelves.ToString() },
            { "AppliedPanel", GetAppliedPanelOption() },
        };

        if (HingeSide != HingeSide.NotApplicable) {
            parameters.Add("HingeLeft", GetHingeSideOption());
        }

        return parameters;
    }

    private string GetHingeSideOption() => HingeSide switch {
        HingeSide.NotApplicable => "0",
        HingeSide.Left => "1",
        HingeSide.Right => "0",
        _ => "0"
    };

    private Dictionary<string, string> GetOverrideParameters() {

        var parameters = new Dictionary<string, string>();
        if (ToeType.PSIParameter != "2") {
            parameters.Add("__ToeBaseType", ToeType.PSIParameter);
            if (ToeType.PSIParameter == "3") {
                parameters.Add("__ToeBaseHeight", "0");
            }
        }

        return parameters;

    }

}
