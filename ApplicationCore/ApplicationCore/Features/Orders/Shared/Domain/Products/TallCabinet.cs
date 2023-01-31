using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.ProductPlanner.Contracts;
using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.Products;

internal class TallCabinet : Cabinet, IPPProductContainer, IDoorContainer, IDrawerBoxContainer {

    public TallCabinetDoors Doors { get; }
    public IToeType ToeType { get; }
    public TallCabinetInside Inside { get; }

    public static CabinetDoorGaps DoorGaps { get; set; } = new() {
        TopGap = Dimension.FromMillimeters(3),
        BottomGap = Dimension.Zero,
        EdgeReveal = Dimension.FromMillimeters(2),
        HorizontalGap = Dimension.FromMillimeters(3),
        VerticalGap = Dimension.FromMillimeters(3),
    };

    public static TallCabinet Create(int qty, decimal unitPrice, int productNumber, string room, bool assembled,
                        Dimension height, Dimension width, Dimension depth,
                        CabinetMaterial boxMaterial, CabinetFinishMaterial finishMaterial, MDFDoorOptions? mdfDoorOptions, string edgeBandingColor,
                        CabinetSide rightSide, CabinetSide leftSide,
                        TallCabinetDoors doors, IToeType toeType, TallCabinetInside inside) {
        return new(Guid.NewGuid(), qty, unitPrice, productNumber, room, assembled, height, width, depth, boxMaterial, finishMaterial, mdfDoorOptions, edgeBandingColor, rightSide, leftSide, doors, toeType, inside);
    }

    private TallCabinet(Guid id, int qty, decimal unitPrice, int productNumber, string room, bool assembled,
                        Dimension height, Dimension width, Dimension depth,
                        CabinetMaterial boxMaterial, CabinetFinishMaterial finishMaterial, MDFDoorOptions? mdfDoorOptions, string edgeBandingColor,
                        CabinetSide rightSide, CabinetSide leftSide,
                        TallCabinetDoors doors, IToeType toeType, TallCabinetInside inside)
                        : base(id, qty, unitPrice, productNumber, room, assembled, height, width, depth, boxMaterial, finishMaterial, mdfDoorOptions, edgeBandingColor, rightSide, leftSide) {

        if (doors.UpperQuantity > 2 || doors.UpperQuantity < 0 || doors.LowerQuantity > 2 || doors.LowerQuantity < 0)
            throw new InvalidOperationException("Invalid number of doors");

        foreach (var position in inside.RollOutBoxes.Positions) {
            if (position < Dimension.Zero || position > height) {
                throw new InvalidOperationException("Roll out box position {position} is invalid for cabinet size");
            }
        }

        Doors = doors;
        ToeType = toeType;
        Inside = inside;

    }

    public IEnumerable<PPProduct> GetPPProducts() {
        string doorType = (MDFDoorOptions is null) ? "Slab" : "Buyout";
        yield return new PPProduct(Room, GetProductName(), ProductNumber, "Royal2", GetMaterialType(), doorType, "Standard", GetFinishMaterials(), GetEBMaterials(), GetParameters(), GetOverrideParameters(), new());
    }

    public IEnumerable<MDFDoor> GetDoors(Func<MDFDoorBuilder> getBuilder) {

        if (MDFDoorOptions is null) {
            return Enumerable.Empty<MDFDoor>();
        }

        List<MDFDoor> doors = new();

        if (Doors.UpperQuantity > 0) {

            var builder = getBuilder();

            Dimension width = (Width - 2 * DoorGaps.EdgeReveal - DoorGaps.HorizontalGap * (Doors.UpperQuantity - 1)) / Doors.UpperQuantity;
            Dimension height = Height - ToeType.ToeHeight - DoorGaps.TopGap - DoorGaps.BottomGap - Doors.LowerDoorHeight - DoorGaps.VerticalGap;

            doors.Add(builder.WithQty(Doors.UpperQuantity * Qty)
                            .WithProductNumber(ProductNumber)
                            .WithFramingBead(MDFDoorOptions.StyleName)
                            .WithPaintColor(MDFDoorOptions.Color == "" ? null : MDFDoorOptions.Color)
                            .Build(height, width));

        }

        if (Doors.LowerQuantity > 0) {

            var builder = getBuilder();

            Dimension height = Doors.UpperQuantity > 0 ? Doors.LowerDoorHeight : Height - ToeType.ToeHeight - DoorGaps.TopGap - DoorGaps.BottomGap;
            Dimension width = (Width - 2 * DoorGaps.EdgeReveal - DoorGaps.HorizontalGap * (Doors.LowerQuantity - 1)) / Doors.LowerQuantity;

            doors.Add(builder.WithQty(Doors.LowerQuantity * Qty)
                            .WithProductNumber(ProductNumber)
                            .WithFramingBead(MDFDoorOptions.StyleName)
                            .WithPaintColor(MDFDoorOptions.Color == "" ? null : MDFDoorOptions.Color)
                            .Build(height, width));

        }

        return doors;

    }

    public IEnumerable<DovetailDrawerBox> GetDrawerBoxes(Func<DovetailDrawerBoxBuilder> getBuilder) {
        
        if (!Inside.RollOutBoxes.Positions.Any()) {

            return Enumerable.Empty<DovetailDrawerBox>();

        }

        var options = new DrawerBoxOptions("", "", "", "", "Blum", GetNotchFromSlideType(Inside.RollOutBoxes.SlideType), "", LogoPosition.None);

        var insideWidth = Width - Construction.SideThickness * 2;
        var insideDepth = Depth - (Construction.BackThickness + Construction.BackInset);
        int rollOutQty = Inside.RollOutBoxes.Positions.Length * Qty;
        var boxHeight = Dimension.FromMillimeters(104);

        var box = getBuilder().WithInnerCabinetDepth(insideDepth, Inside.RollOutBoxes.SlideType, true)
                                .WithInnerCabinetWidth(insideWidth, Inside.RollOutBoxes.Blocks, Inside.RollOutBoxes.SlideType)
                                .WithBoxHeight(boxHeight)
                                .WithQty(rollOutQty)
                                .WithOptions(options)
                                .WithProductNumber(ProductNumber)
                                .Build();

        return new DovetailDrawerBox[] { box };

    }

    private static string GetNotchFromSlideType(DrawerSlideType slide) => slide switch {
        DrawerSlideType.UnderMount => "Standard Notch",
        DrawerSlideType.SideMount => "",
        _ => ""
    };

    private string GetProductName() {
        string name = $"T{Doors.LowerQuantity + Doors.UpperQuantity}D";
        if (Doors.UpperQuantity != 0) name += "2S";
        return name;
    }

    private Dictionary<string, string> GetParameters() {
        var parameters = new Dictionary<string, string>() {
            { "ProductW", Width.AsMillimeters().ToString() },
            { "ProductH", Height.AsMillimeters().ToString() },
            { "ProductD", Depth.AsMillimeters().ToString() },
            { "FinishedLeft", GetSideOption(LeftSide.Type) },
            { "FinishedRight", GetSideOption(RightSide.Type) },
            { "ShelfQ", Inside.AdjustableShelvesLower.ToString() },
            { "ShelfQUpSect", Inside.AdjustableShelvesUpper.ToString() },
            { "DividerQ", Inside.VerticalDividersLower.ToString() },
            { "DividerQUp", Inside.VerticalDividersLower.ToString() },
            { "DoorHTallBot", Doors.LowerDoorHeight.AsMillimeters().ToString() },
            { "PulloutBlockType", GetRollOutBlockOption() },
            { "AppliedPanel", GetAppliedPanelOption() },
        };

        int posNum = 1;
        foreach (var pos in Inside.RollOutBoxes.Positions) {
            parameters.Add($"Rollout{posNum++}", pos.AsMillimeters().ToString());
        }

        if (Doors.HingeSide != HingeSide.NotApplicable) {
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

        if (!Inside.RollOutBoxes.Positions.Any() && Inside.RollOutBoxes.SlideType == DrawerSlideType.SideMount) {
            parameters.Add("_DrawerRunType", "4");
        }

        return parameters;

    }

    private string GetHingeSideOption() => Doors.HingeSide switch {
        HingeSide.NotApplicable => "0",
        HingeSide.Left => "1",
        HingeSide.Right => "0",
        _ => "0"
    };

    private string GetRollOutBlockOption() => Inside.RollOutBoxes.Blocks switch {
        RollOutBlockPosition.None => "0",
        RollOutBlockPosition.Left => "1",
        RollOutBlockPosition.Both => "2",
        RollOutBlockPosition.Right => "3",
        _ => "0"
    };

}