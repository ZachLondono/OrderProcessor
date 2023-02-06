using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.ProductPlanner.Contracts;
using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.Products;

internal class WallDiagonalCornerCabinet : Cabinet, IPPProductContainer, IDoorContainer {

    public Dimension RightWidth { get; }
    public Dimension RightDepth { get; }
    public HingeSide HingeSide { get; }
    public int DoorQty { get; }
    public int AdjustableShelves { get; }
    public Dimension ExtendedDoor { get; }

    public static CabinetDoorGaps DoorGaps { get; set; } = new() {
        TopGap = Dimension.FromMillimeters(3),
        BottomGap = Dimension.Zero,
        EdgeReveal = Dimension.FromMillimeters(3),
        HorizontalGap = Dimension.FromMillimeters(3),
        VerticalGap = Dimension.FromMillimeters(3),
    };

    public WallDiagonalCornerCabinet(Guid id, int qty, decimal unitPrice, int productNumber, string room, bool assembled,
                        Dimension height, Dimension width, Dimension depth,
                        CabinetMaterial boxMaterial, CabinetFinishMaterial finishMaterial, MDFDoorOptions? mdfOptions, string edgeBandingColor,
                        CabinetSide rightSide, CabinetSide leftSide,
                        Dimension rightWidth, Dimension rightDepth, int adjShelfQty, HingeSide hingeSide, int doorQty, Dimension extendedDoor)
                        : base(id, qty, unitPrice, productNumber, room, assembled, height, width, depth, boxMaterial, finishMaterial, mdfOptions, edgeBandingColor, rightSide, leftSide) {

        if (leftSide.Type == CabinetSideType.AppliedPanel || rightSide.Type == CabinetSideType.AppliedPanel)
            throw new InvalidOperationException("Wall cabinet cannot have applied panel sides");

        RightWidth = rightWidth;
        RightDepth = rightDepth;
        AdjustableShelves = adjShelfQty;
        HingeSide = hingeSide;
        DoorQty = doorQty;
        ExtendedDoor = extendedDoor;

        if (DoorQty > 2 || DoorQty < 1) throw new InvalidOperationException("Invalid number of doors");

    }

    public static WallDiagonalCornerCabinet Create(int qty, decimal unitPrice, int productNumber, string room, bool assembled,
                        Dimension height, Dimension width, Dimension depth,
                        CabinetMaterial boxMaterial, CabinetFinishMaterial finishMaterial, MDFDoorOptions? mdfOptions, string edgeBandingColor,
                        CabinetSide rightSide, CabinetSide leftSide,
                        Dimension rightWidth, Dimension rightDepth, int adjShelfQty, HingeSide hingeSide, int doorQty, Dimension extendedDoor)
                        => new(Guid.NewGuid(), qty, unitPrice, productNumber, room, assembled, height, width, depth, boxMaterial, finishMaterial, mdfOptions, edgeBandingColor, rightSide, leftSide, rightWidth, rightDepth, adjShelfQty, hingeSide, doorQty, extendedDoor);

    public IEnumerable<PPProduct> GetPPProducts() {
        string doorType = (MDFDoorOptions is null) ? "Slab" : "Buyout";

        string sku = DoorQty switch {
            1 => "WC1D-M",
            2 => "WC2D-M",
            _ => "WC1D-M"
        };

        yield return new PPProduct(Id, Room, sku, ProductNumber, "Royal2", GetMaterialType(), doorType, "Standard", GetFinishMaterials(), GetEBMaterials(), GetParameters(), GetParameterOverrides(), new());
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

        Dimension height = Height - DoorGaps.TopGap - DoorGaps.BottomGap;

        var door = getBuilder().WithQty(DoorQty * Qty)
                                .WithProductNumber(ProductNumber)
                                .WithFramingBead(MDFDoorOptions.StyleName)
                                .WithPaintColor(MDFDoorOptions.Color == "" ? null : MDFDoorOptions.Color)
                                .Build(height, width);

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

    private Dictionary<string, string> GetParameterOverrides() {

        var parameters = new Dictionary<string, string>();

        if (ExtendedDoor != Dimension.Zero) {
            parameters.Add("ExtendDoorD", ExtendedDoor.AsMillimeters().ToString());
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
