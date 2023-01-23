using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.ProductPlanner.Contracts;
using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.Products;

internal class BlindBaseCabinet : Cabinet, IPPProductContainer {

    public BlindCabinetDoors Doors { get; }
    public HorizontalDrawerBank Drawers { get; }
    public int AdjustableShelves { get; }
    public ShelfDepth ShelfDepth { get; }
    public BlindSide BlindSide { get; }
    public Dimension BlindWidth { get; }
    public IToeType ToeType { get; }

    public static BlindBaseCabinet Create(int qty, decimal unitPrice, string room, bool assembled,
                        Dimension height, Dimension width, Dimension depth,
                        CabinetMaterial boxMaterial, CabinetMaterial finishMaterial, string edgeBandingColor,
                        CabinetSide rightSide, CabinetSide leftSide,
                        BlindCabinetDoors doors, BlindSide blindSide, Dimension blindWidth, int adjustableShelves, ShelfDepth shelfDepth, HorizontalDrawerBank drawers, IToeType toeType) {
        return new(Guid.NewGuid(), qty, unitPrice, room, assembled, height, width, depth, boxMaterial, finishMaterial, edgeBandingColor, rightSide, leftSide, doors, blindSide, blindWidth, adjustableShelves, shelfDepth, drawers, toeType);
    }

    private BlindBaseCabinet(Guid id, int qty, decimal unitPrice, string room, bool assembled,
                        Dimension height, Dimension width, Dimension depth,
                        CabinetMaterial boxMaterial, CabinetMaterial finishMaterial, string edgeBandingColor,
                        CabinetSide rightSide, CabinetSide leftSide,
                        BlindCabinetDoors doors, BlindSide blindSide, Dimension blindWidth, int adjustableShelves, ShelfDepth shelfDepth, HorizontalDrawerBank drawers, IToeType toeType)
                        : base(id, qty, unitPrice, room, assembled, height, width, depth, boxMaterial, finishMaterial, edgeBandingColor, rightSide, leftSide) {

        Doors = doors;
        BlindSide = blindSide;
        AdjustableShelves = adjustableShelves;
        ShelfDepth = shelfDepth;
        Drawers = drawers;
        ToeType = toeType;
        BlindWidth = blindWidth;

    }

    public IEnumerable<PPProduct> GetPPProducts() {
        string doorType = (Doors.MDFOptions is null) ? "Slab" : "Buyout";
        yield return new PPProduct(Room, GetProductName(), "Royal2", GetMaterialType(), doorType, "Standard", GetFinishMaterials(), GetEBMaterials(), GetParameters(), GetParameterOverrides(), GetManualOverrideParameters());
    }

    private string GetProductName() {
        return $"BB{Doors.Quantity}D{GetDrawerCountSkuPart()}{GetBlindSideLetter()}";
    }

    private Dictionary<string, string> GetParameters() {
        var parameters = new Dictionary<string, string>() {
            { "ProductW", Width.AsMillimeters().ToString() },
            { "ProductH", Height.AsMillimeters().ToString() },
            { "ProductD", Depth.AsMillimeters().ToString() },
            { "FinishedLeft", GetSideOption(LeftSide.Type) },
            { "FinishedRight", GetSideOption(RightSide.Type) },
            { "ShelfQ", AdjustableShelves.ToString() },
            { "BlindW", BlindWidth.AsMillimeters().ToString() }
        };

        if (Doors.HingeSide != HingeSide.NotApplicable) {
            parameters.Add("HingeLeft", GetHingeSideOption());
        }

        return parameters;
    }

    private Dictionary<string, string> GetParameterOverrides() {

        var parameters = new Dictionary<string, string>();

        if (ToeType.PSIParameter != "2") {
            parameters.Add("__ToeBaseType", ToeType.PSIParameter);
            if (ToeType.PSIParameter == "3") {
                parameters.Add("__ToeBaseHeight", "0");
            }
        }

        if (LeftSide.Type != CabinetSideType.IntegratedPanel && RightSide.Type != CabinetSideType.IntegratedPanel && ShelfDepth == ShelfDepth.Full) {

            Dimension backThickness = Dimension.FromMillimeters(13);
            Dimension backInset = Dimension.FromMillimeters(9);
            Dimension shelfFrontClearance = Dimension.FromMillimeters(8);

            parameters.Add("_HalfDepthShelfW", (Depth - backThickness - backInset - shelfFrontClearance).AsMillimeters().ToString());

        }

        return parameters;

    }

    public Dictionary<string, string> GetManualOverrideParameters() {

        var parameters = new Dictionary<string, string>();

        if (LeftSide.Type != CabinetSideType.IntegratedPanel && RightSide.Type != CabinetSideType.IntegratedPanel) {

            if (ShelfDepth == ShelfDepth.Half) {

                Dimension newWidth = (Depth - Dimension.FromMillimeters(13) - Dimension.FromMillimeters(9)) / 2;

                parameters.Add("_HalfDepthShelfW", newWidth.AsMillimeters().ToString());

            } else if (ShelfDepth == ShelfDepth.ThreeQuarters) {

                Dimension newWidth = (Depth - Dimension.FromMillimeters(13) - Dimension.FromMillimeters(9)) * 0.75;

                parameters.Add("_HalfDepthShelfW", newWidth.AsMillimeters().ToString());

            }

        }


        return parameters;

    }

    private string GetDrawerCountSkuPart() => Drawers.Quantity switch {
        0 => string.Empty,
        1 => "1D",
        2 => "2D",
        _ => throw new ArgumentOutOfRangeException(nameof(Drawers))
    };

    private string GetBlindSideLetter() => BlindSide switch {
        BlindSide.Left => "L",
        BlindSide.Right => "R",
        _ => throw new ArgumentOutOfRangeException(nameof(BlindSide))
    };

    private string GetHingeSideOption() => Doors.HingeSide switch {
        HingeSide.NotApplicable => "0",
        HingeSide.Left => "1",
        HingeSide.Right => "0",
        _ => "0"
    };

}
