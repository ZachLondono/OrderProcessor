using ApplicationCore.Features.Orders.Details.OrderExport.Handlers.ExtExport.Contracts;
using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.Products;

internal class BaseDiagonalCornerCabinet : Cabinet, IPPProductContainer, IDoorContainer {

    public Dimension RightWidth { get; }
    public Dimension RightDepth { get; }
    public ToeType ToeType { get; }
    public HingeSide HingeSide { get; }
    public int DoorQty { get; }
    public int AdjustableShelves { get; }

    public Dimension DoorHeight => Height - ToeType.HeightAdjustment - DoorGaps.TopGap - DoorGaps.BottomGap;

    public override string GetDescription() => "Diagonal Corner Base Cabinet";

    public static CabinetDoorGaps DoorGaps { get; set; } = new() {
        TopGap = Dimension.FromMillimeters(7),
        BottomGap = Dimension.Zero,
        EdgeReveal = Dimension.FromMillimeters(3),
        HorizontalGap = Dimension.FromMillimeters(3),
        VerticalGap = Dimension.FromMillimeters(3),
    };

    internal BaseDiagonalCornerCabinet(Guid id, int qty, decimal unitPrice, int productNumber, string room, bool assembled,
                        Dimension height, Dimension width, Dimension depth,
                        CabinetMaterial boxMaterial, CabinetFinishMaterial finishMaterial, MDFDoorOptions? mdfDoorOptions, string edgeBandingColor,
                        CabinetSideType rightSideType, CabinetSideType leftSideType, string comment,
                        Dimension rightWidth, Dimension rightDepth, ToeType toeType, int adjShelfQty, HingeSide hingeSide, int doorQty)
                        : base(id, qty, unitPrice, productNumber, room, assembled, height, width, depth, boxMaterial, finishMaterial, mdfDoorOptions, edgeBandingColor, rightSideType, leftSideType, comment) {
        RightWidth = rightWidth;
        RightDepth = rightDepth;
        ToeType = toeType;
        AdjustableShelves = adjShelfQty;
        HingeSide = hingeSide;
        DoorQty = doorQty;

        if (DoorQty > 2 || DoorQty < 1) throw new InvalidOperationException("Invalid number of doors");

    }

    public static BaseDiagonalCornerCabinet Create(int qty, decimal unitPrice, int productNumber, string room, bool assembled,
                        Dimension height, Dimension width, Dimension depth,
                        CabinetMaterial boxMaterial, CabinetFinishMaterial finishMaterial, MDFDoorOptions? mdfDoorOptions, string edgeBandingColor,
                        CabinetSideType rightSideType, CabinetSideType leftSideType, string comment,
                        Dimension rightWidth, Dimension rightDepth, ToeType toeType, int adjShelfQty, HingeSide hingeSide, int doorQty)
                        => new(Guid.NewGuid(), qty, unitPrice, productNumber, room, assembled, height, width, depth, boxMaterial, finishMaterial, mdfDoorOptions, edgeBandingColor, rightSideType, leftSideType, comment, rightWidth, rightDepth, toeType, adjShelfQty, hingeSide, doorQty);

    public IEnumerable<PPProduct> GetPPProducts() {
        string doorType = (MDFDoorOptions is null) ? "Slab" : "Buyout";

        string sku = DoorQty switch {
            1 => "BC1D-M",
            2 => "BC2D-M",
            _ => "BC1D-M"
        };

        yield return new PPProduct(Id, Qty, Room, sku, ProductNumber, "Royal2", GetMaterialType(), doorType, "Standard", Comment, GetFinishMaterials(), GetEBMaterials(), GetParameters(), GetOverrideParameters(), new Dictionary<string, string>());
    }

    public IEnumerable<MDFDoor> GetDoors(Func<MDFDoorBuilder> getBuilder) {

        if (MDFDoorOptions is null) {
            return Enumerable.Empty<MDFDoor>();
        }

        Dimension a = Width - RightDepth - Dimension.FromMillimeters(19);
        Dimension b = RightWidth - Depth - Dimension.FromMillimeters(19);

        Dimension diagOpening = Dimension.Sqrt(a * a + b * b);

        Dimension width = RoundToHalfMM(diagOpening - 2 * DoorGaps.EdgeReveal);
        if (DoorQty == 2) {
            width = RoundToHalfMM((width - DoorGaps.HorizontalGap) / 2);
        }

        Dimension height = DoorHeight;

        var door = getBuilder().WithQty(DoorQty * Qty)
                                .WithProductNumber(ProductNumber)
                                .WithFramingBead(MDFDoorOptions.FramingBead)
                                .WithPaintColor(MDFDoorOptions.PaintColor == "" ? null : MDFDoorOptions.PaintColor)
                                .Build(height, width);

        return new List<MDFDoor>() { door };

    }

    public override IEnumerable<Supply> GetSupplies() {

        var supplies = new List<Supply>();

        if (AdjustableShelves > 0) {

            supplies.Add(Supply.LockingShelfPeg(AdjustableShelves * 5 * Qty));

        }

        if (DoorQty > 0) {

            supplies.Add(Supply.DoorPull(DoorQty * Qty));
            supplies.AddRange(Supply.CrossCornerHinge(DoorHeight, DoorQty * Qty));

        }

        if (ToeType == ToeType.LegLevelers) {

            supplies.Add(Supply.CabinetLeveler(Qty * 5));

        }

        return supplies;

    }

    public static Dimension RoundToHalfMM(Dimension dim) {

        var value = Math.Round(dim.AsMillimeters() * 2, MidpointRounding.AwayFromZero) / 2;

        return Dimension.FromMillimeters(value);

    }

    private Dictionary<string, string> GetParameters() {
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
            { "LazySusan", "N" },
        };

        if (HingeSide != HingeSide.NotApplicable) {
            parameters.Add("HingeLeft", GetHingeSideOption());
        }

        return parameters;
    }

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

    private string GetHingeSideOption() => HingeSide switch {
        HingeSide.NotApplicable => "0",
        HingeSide.Left => "1",
        HingeSide.Right => "0",
        _ => "0"
    };

}
