using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.ProductPlanner.Contracts;
using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.Products;

internal class BlindBaseCabinet : Cabinet, IPPProductContainer, IDoorContainer {

    public BlindCabinetDoors Doors { get; }
    public HorizontalDrawerBank Drawers { get; }
    public int AdjustableShelves { get; }
    public ShelfDepth ShelfDepth { get; }
    public BlindSide BlindSide { get; }
    public Dimension BlindWidth { get; }
    public IToeType ToeType { get; }

    public static CabinetDoorGaps DoorGaps { get; set; } = new() {
        TopGap = Dimension.FromMillimeters(7),
        BottomGap = Dimension.Zero,
        EdgeReveal = Dimension.FromMillimeters(2),
        HorizontalGap = Dimension.FromMillimeters(3),
        VerticalGap = Dimension.FromMillimeters(3),
    };

    public static BlindBaseCabinet Create(int qty, decimal unitPrice, int productNumber, string room, bool assembled,
                        Dimension height, Dimension width, Dimension depth,
                        CabinetMaterial boxMaterial, CabinetFinishMaterial finishMaterial, MDFDoorOptions? mdfDoorOptions, string edgeBandingColor,
                        CabinetSide rightSide, CabinetSide leftSide,
                        BlindCabinetDoors doors, BlindSide blindSide, Dimension blindWidth, int adjustableShelves, ShelfDepth shelfDepth, HorizontalDrawerBank drawers, IToeType toeType) {
        return new(Guid.NewGuid(), qty, unitPrice, productNumber, room, assembled, height, width, depth, boxMaterial, finishMaterial, mdfDoorOptions, edgeBandingColor, rightSide, leftSide, doors, blindSide, blindWidth, adjustableShelves, shelfDepth, drawers, toeType);
    }

    private BlindBaseCabinet(Guid id, int qty, decimal unitPrice, int productNumber, string room, bool assembled,
                        Dimension height, Dimension width, Dimension depth,
                        CabinetMaterial boxMaterial, CabinetFinishMaterial finishMaterial, MDFDoorOptions? mdfDoorOptions, string edgeBandingColor,
                        CabinetSide rightSide, CabinetSide leftSide,
                        BlindCabinetDoors doors, BlindSide blindSide, Dimension blindWidth, int adjustableShelves, ShelfDepth shelfDepth, HorizontalDrawerBank drawers, IToeType toeType)
                        : base(id, qty, unitPrice, productNumber, room, assembled, height, width, depth, boxMaterial, finishMaterial, mdfDoorOptions, edgeBandingColor, rightSide, leftSide) {

        Doors = doors;
        BlindSide = blindSide;
        AdjustableShelves = adjustableShelves;
        ShelfDepth = shelfDepth;
        Drawers = drawers;
        ToeType = toeType;
        BlindWidth = blindWidth;

    }

    public IEnumerable<PPProduct> GetPPProducts() {
        string doorType = (MDFDoorOptions is null) ? "Slab" : "Buyout";
        yield return new PPProduct(Id, Room, GetProductName(), ProductNumber, "Royal2", GetMaterialType(), doorType, "Standard", GetFinishMaterials(), GetEBMaterials(), GetParameters(), GetParameterOverrides(), GetManualOverrideParameters());
    }

    private string GetProductName() {
        return $"BB{Doors.Quantity}D{GetDrawerCountSkuPart()}{GetBlindSideLetter()}";
    }

    public IEnumerable<MDFDoor> GetDoors(Func<MDFDoorBuilder> getBuilder) {

        if (MDFDoorOptions is null) {
            return Enumerable.Empty<MDFDoor>();
        }

        List<MDFDoor> doors = new();

        if (Doors.Quantity > 0) {
            Dimension width = (Width - BlindWidth - DoorGaps.EdgeReveal - DoorGaps.HorizontalGap / 2 - DoorGaps.HorizontalGap * (Doors.Quantity - 1)) / Doors.Quantity;
            Dimension height = Height - ToeType.ToeHeight - DoorGaps.TopGap - DoorGaps.BottomGap - (Drawers.Quantity > 0 ? Drawers.FaceHeight + DoorGaps.VerticalGap : Dimension.Zero);
            var door = getBuilder().WithQty(Doors.Quantity * Qty)
                                    .WithProductNumber(ProductNumber)
                                    .WithType(DoorType.Door)
                                    .WithFramingBead(MDFDoorOptions.StyleName)
                                    .WithPaintColor(MDFDoorOptions.Color == "" ? null : MDFDoorOptions.Color)
                                    .Build(height, width);
            doors.Add(door);
        }

        if (Drawers.Quantity > 0) {
            Dimension drwWidth = (Width - BlindWidth - DoorGaps.EdgeReveal - DoorGaps.HorizontalGap / 2 - DoorGaps.HorizontalGap * (Drawers.Quantity - 1)) / Drawers.Quantity;
            var drawers = getBuilder().WithQty(Drawers.Quantity * Qty)
                                        .WithProductNumber(ProductNumber)
                                        .WithType(DoorType.DrawerFront)
                                        .WithFramingBead(MDFDoorOptions.StyleName)
                                        .WithPaintColor(MDFDoorOptions.Color == "" ? null : MDFDoorOptions.Color)
                                        .Build(Drawers.FaceHeight, drwWidth);
            doors.Add(drawers);
        }

        return doors;

    }

    public IEnumerable<DovetailDrawerBox> GetDrawerBoxes(Func<DovetailDrawerBoxBuilder> getBuilder) {

        if (Drawers.Quantity == 0) {
            return Enumerable.Empty<DovetailDrawerBox>();
        }


        var insideWidth = Width - Construction.SideThickness * 2 - BlindWidth;
        var insideDepth = Depth - (Construction.BackThickness + Construction.BackInset);

        var options = new DrawerBoxOptions("", "", "", "", "Blum", GetNotchFromSlideType(Drawers.SlideType), "", LogoPosition.None);

        int drawerQty = Drawers.Quantity * Qty;

        var box = getBuilder().WithInnerCabinetDepth(insideDepth, Drawers.SlideType)
                                .WithInnerCabinetWidth(insideWidth, Drawers.Quantity, Drawers.SlideType)
                                .WithDrawerFaceHeight(Drawers.FaceHeight)
                                .WithQty(drawerQty)
                                .WithOptions(options)
                                .WithProductNumber(ProductNumber)
                                .Build();

        return new List<DovetailDrawerBox>() { box };

    }

    private static string GetNotchFromSlideType(DrawerSlideType slide) => slide switch {
        DrawerSlideType.UnderMount => "Standard Notch",
        DrawerSlideType.SideMount => "",
        _ => ""
    };

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
