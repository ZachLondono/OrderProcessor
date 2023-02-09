using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.ProductPlanner.Contracts;
using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.Products;

internal class DrawerBaseCabinet : Cabinet, IPPProductContainer, IDoorContainer, IDrawerBoxContainer {

    public IToeType ToeType { get; }
    public VerticalDrawerBank Drawers { get; }

    public override string Description => $"{Drawers.FaceHeights.Count()} Drawer Cabinet";

    public static CabinetDoorGaps DoorGaps { get; set; } = new() {
        TopGap = Dimension.FromMillimeters(7),
        BottomGap = Dimension.Zero,
        EdgeReveal = Dimension.FromMillimeters(2),
        HorizontalGap = Dimension.FromMillimeters(3),
        VerticalGap = Dimension.FromMillimeters(3),
    };

    public static DrawerBaseCabinet Create(int qty, decimal unitPrice, int productNumber, string room, bool assembled,
                        Dimension height, Dimension width, Dimension depth,
                        CabinetMaterial boxMaterial, CabinetFinishMaterial finishMaterial, MDFDoorOptions? mdfDoorOptions, string edgeBandingColor,
                        CabinetSide rightSide, CabinetSide leftSide, string comment,
                        IToeType toeType, VerticalDrawerBank drawers) {
        return new(Guid.NewGuid(), qty, unitPrice, productNumber, room, assembled, height, width, depth, boxMaterial, finishMaterial, mdfDoorOptions, edgeBandingColor, rightSide, leftSide, comment, toeType, drawers);
    }

    private DrawerBaseCabinet(Guid id, int qty, decimal unitPrice, int productNumber, string room, bool assembled,
                        Dimension height, Dimension width, Dimension depth,
                        CabinetMaterial boxMaterial, CabinetFinishMaterial finishMaterial, MDFDoorOptions? mdfDoorOptions, string edgeBandingColor,
                        CabinetSide rightSide, CabinetSide leftSide, string comment,
                        IToeType toeType, VerticalDrawerBank drawers)
                        : base(id, qty, unitPrice, productNumber, room, assembled, height, width, depth, boxMaterial, finishMaterial, mdfDoorOptions, edgeBandingColor, rightSide, leftSide, comment) {

        if (drawers.FaceHeights.Count() > 5)
            throw new InvalidOperationException("Invalid number of drawers");

        Drawers = drawers;
        ToeType = toeType;
    }

    public IEnumerable<PPProduct> GetPPProducts() {
        // TODO: add option for no doors
        string doorType = (MDFDoorOptions is null) ? "Slab" : "Buyout";
        yield return new PPProduct(Id, Room, GetProductName(), ProductNumber, "Royal2", GetMaterialType(), doorType, "Standard", Comment, GetFinishMaterials(), GetEBMaterials(), GetParameters(), GetOverrideParameters(), new());
    }

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
                                    .WithFramingBead(MDFDoorOptions.StyleName)
                                    .WithPaintColor(MDFDoorOptions.Color == "" ? null : MDFDoorOptions.Color)
                                    .Build(height, width);

            doors.Add(door);

        }

        return doors;

    }

    public IEnumerable<DovetailDrawerBox> GetDrawerBoxes(Func<DovetailDrawerBoxBuilder> getBuilder) {

        if (!Drawers.FaceHeights.Any()) {
            return Enumerable.Empty<DovetailDrawerBox>();
        }

        var insideWidth = Width - Construction.SideThickness * 2;
        var insideDepth = Depth - (Construction.BackThickness + Construction.BackInset);

        var options = new DrawerBoxOptions("", "", "", "", "Blum", GetNotchFromSlideType(Drawers.SlideType), "", LogoPosition.None);

        var boxes = new List<DovetailDrawerBox>();

        foreach (var height in Drawers.FaceHeights) { 

            var box = getBuilder().WithInnerCabinetDepth(insideDepth, Drawers.SlideType)
                                    .WithInnerCabinetWidth(insideWidth, 1, Drawers.SlideType)
                                    .WithDrawerFaceHeight(height)
                                    .WithQty(Qty)
                                    .WithOptions(options)
                                    .WithProductNumber(ProductNumber)
                                    .Build();

            boxes.Add(box);

        }

        return boxes;

    }

    private static string GetNotchFromSlideType(DrawerSlideType slide) => slide switch {
        DrawerSlideType.UnderMount => "Standard Notch",
        DrawerSlideType.SideMount => "",
        _ => ""
    };

    private string GetProductName() {
        if (!Drawers.FaceHeights.Any()) return "DB1D";
        return $"DB{Drawers.FaceHeights.Count()}D";
    }

    private Dictionary<string, string> GetParameters() {
        var parameters = new Dictionary<string, string>() {
            { "ProductW", Width.AsMillimeters().ToString() },
            { "ProductH", Height.AsMillimeters().ToString() },
            { "ProductD", Depth.AsMillimeters().ToString() },
            { "FinishedLeft", GetSideOption(LeftSide.Type) },
            { "FinishedRight", GetSideOption(RightSide.Type) },
            { "AppliedPanel", GetAppliedPanelOption() },
        };

        int index = 1;
        foreach (var height in Drawers.FaceHeights) {
            if (index == 5) break; // in 5 drawer cabinets, the fifth drawer must be calculated by PSI
            parameters.Add($"DrawerH{index++}", height.AsMillimeters().ToString());
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

        if (Drawers.FaceHeights.Any() && Drawers.SlideType == DrawerSlideType.SideMount) {
            parameters.Add("_DrawerRunType", "4");
        }

        return parameters;

    }

}