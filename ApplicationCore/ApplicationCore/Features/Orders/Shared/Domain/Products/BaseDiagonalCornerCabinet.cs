using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.ProductPlanner.Contracts;
using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.Products;

internal class BaseDiagonalCornerCabinet : Cabinet, IPPProductContainer, IDoorContainer {

    public Dimension RightWidth { get; }
    public Dimension RightDepth { get; }
    public IToeType ToeType { get; }
    public HingeSide HingeSide { get; }
    public int DoorQty { get; }
    public MDFDoorOptions? MDFOptions { get; }
    public int AdjustableShelves { get; }

    public static CabinetDoorGaps DoorGaps { get; set; } = new() {
        TopGap = Dimension.FromMillimeters(7),
        BottomGap = Dimension.Zero,
        EdgeReveal = Dimension.FromMillimeters(3),
        HorizontalGap = Dimension.FromMillimeters(3),
        VerticalGap = Dimension.FromMillimeters(3),
    };

    public BaseDiagonalCornerCabinet(Guid id, int qty, decimal unitPrice, string room, bool assembled,
                        Dimension height, Dimension width, Dimension depth,
                        CabinetMaterial boxMaterial, CabinetFinishMaterial finishMaterial, string edgeBandingColor,
                        CabinetSide rightSide, CabinetSide leftSide,
                        Dimension rightWidth, Dimension rightDepth, IToeType toeType, int adjShelfQty, HingeSide hingeSide, int doorQty, MDFDoorOptions? mdfOptions)
                        : base(id, qty, unitPrice, room, assembled, height, width, depth, boxMaterial, finishMaterial, edgeBandingColor, rightSide, leftSide) {
        RightWidth = rightWidth;
        RightDepth = rightDepth;
        ToeType = toeType;
        AdjustableShelves = adjShelfQty;
        HingeSide = hingeSide;
        DoorQty = doorQty;
        MDFOptions = mdfOptions;

        if (DoorQty > 2 || DoorQty < 1) throw new InvalidOperationException("Invalid number of doors");

    }

    public static BaseDiagonalCornerCabinet Create(int qty, decimal unitPrice, string room, bool assembled,
                        Dimension height, Dimension width, Dimension depth,
                        CabinetMaterial boxMaterial, CabinetFinishMaterial finishMaterial, string edgeBandingColor,
                        CabinetSide rightSide, CabinetSide leftSide,
                        Dimension rightWidth, Dimension rightDepth, IToeType toeType, int adjShelfQty, HingeSide hingeSide, int doorQty, MDFDoorOptions? mdfOptions)
                        => new(Guid.NewGuid(), qty, unitPrice, room, assembled, height, width, depth, boxMaterial, finishMaterial, edgeBandingColor, rightSide, leftSide, rightWidth, rightDepth, toeType, adjShelfQty, hingeSide, doorQty, mdfOptions);

    public IEnumerable<PPProduct> GetPPProducts() {
        string doorType = (MDFOptions is null) ? "Slab" : "Buyout";

        string sku = DoorQty switch {
            1 => "BC1D-M",
            2 => "BC2D-M",
            _ => "BC1D-M"
        };

        yield return new PPProduct(Room, sku, "Royal2", GetMaterialType(), doorType, "Standard", GetFinishMaterials(), GetEBMaterials(), GetParameters(), GetOverrideParameters(), new());
    }

    public IEnumerable<MDFDoor> GetDoors(Func<MDFDoorBuilder> getBuilder) {

        if (MDFOptions is null) {
            return Enumerable.Empty<MDFDoor>();
        }

        Dimension a = Width - RightDepth - Dimension.FromMillimeters(19);
        Dimension b = RightWidth - Depth - Dimension.FromMillimeters(19);

        Dimension diagOpening = Dimension.Sqrt(a * a + b * b);

        Dimension width = RoundToHalfMM(diagOpening - 2 * DoorGaps.EdgeReveal);
        if (DoorQty == 2) {
            width = RoundToHalfMM((width - DoorGaps.HorizontalGap) / 2);
        }

        Dimension height = Height - ToeType.HeightAdjustment - DoorGaps.TopGap - DoorGaps.BottomGap;

        var door = getBuilder().WithQty(DoorQty * Qty).Build(height, width);

        return new List<MDFDoor>() { door };

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
            { "FinishedLeft", GetSideOption(LeftSide.Type) },
            { "FinishedRight", GetSideOption(RightSide.Type) },
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
