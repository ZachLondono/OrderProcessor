using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.ProductPlanner.Contracts;
using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.Products;

internal class TrashCabinet : Cabinet, IDoorContainer, IDrawerBoxContainer, IPPProductContainer {

    public Dimension DrawerFaceHeight { get; set; }
    public DrawerSlideType SlideType { get; set; }
    public CabinetDrawerBoxMaterial DrawerBoxMaterial { get; set; }
    public TrashPulloutConfiguration TrashPulloutConfiguration { get; set; }
    public IToeType ToeType { get; }

    public override string Description => "Trash Pullout Cabinet";

    public static CabinetDoorGaps DoorGaps { get; set; } = new() {
        TopGap = Dimension.FromMillimeters(7),
        BottomGap = Dimension.Zero,
        EdgeReveal = Dimension.FromMillimeters(2),
        HorizontalGap = Dimension.FromMillimeters(3),
        VerticalGap = Dimension.FromMillimeters(3),
    };

    public TrashCabinet(Guid id, int qty, decimal unitPrice, int productNumber, string room, bool assembled,
                        Dimension height, Dimension width, Dimension depth,
                        CabinetMaterial boxMaterial, CabinetFinishMaterial finishMaterial, MDFDoorOptions? mdfDoorOptions, string edgeBandingColor,
                        CabinetSide rightSide, CabinetSide leftSide, string comment,
                        Dimension drawerFaceHeight, TrashPulloutConfiguration trashPulloutConfiguration, DrawerSlideType slideType, CabinetDrawerBoxMaterial drawerBoxMaterial, IToeType toeType)
        : base(id, qty, unitPrice, productNumber, room, assembled, height, width, depth, boxMaterial, finishMaterial, mdfDoorOptions, edgeBandingColor, rightSide, leftSide, comment) {
        DrawerFaceHeight = drawerFaceHeight;
        TrashPulloutConfiguration = trashPulloutConfiguration;
        SlideType = slideType;
        DrawerBoxMaterial = drawerBoxMaterial;
        ToeType = toeType;
    }

    public static TrashCabinet Create(int qty, decimal unitPrice, int productNumber, string room, bool assembled,
                        Dimension height, Dimension width, Dimension depth,
                        CabinetMaterial boxMaterial, CabinetFinishMaterial finishMaterial, MDFDoorOptions? mdfDoorOptions, string edgeBandingColor,
                        CabinetSide rightSide, CabinetSide leftSide, string comment,
                        Dimension drawerFaceHeight, TrashPulloutConfiguration trashPulloutConfiguration, DrawerSlideType slideType, CabinetDrawerBoxMaterial drawerBoxMaterial, IToeType toeType)
        => new(Guid.NewGuid(), qty, unitPrice, productNumber, room, assembled, height, width, depth, boxMaterial, finishMaterial, mdfDoorOptions, edgeBandingColor, rightSide, leftSide, comment, drawerFaceHeight, trashPulloutConfiguration, slideType, drawerBoxMaterial, toeType);
    
    public IEnumerable<PPProduct> GetPPProducts() {
        string doorType = (MDFDoorOptions is null) ? "Slab" : "Buyout";
        yield return new PPProduct(Id, Room, "BT1D1D", ProductNumber, "Royal2", GetMaterialType(), doorType, "Standard", Comment, GetFinishMaterials(), GetEBMaterials(), GetParameters(), GetOverrideParameters(), new Dictionary<string, string>());
    }

    public IEnumerable<MDFDoor> GetDoors(Func<MDFDoorBuilder> getBuilder) {

        if (MDFDoorOptions is null) {
            return Enumerable.Empty<MDFDoor>();
        }

        List<MDFDoor> doors = new();

        Dimension width = Width - 2 * DoorGaps.EdgeReveal;
        Dimension height = Height - ToeType.ToeHeight - DoorGaps.TopGap - DoorGaps.BottomGap - DrawerFaceHeight - DoorGaps.VerticalGap;
        var door = getBuilder().WithQty(Qty)
                                .WithProductNumber(ProductNumber)
                                .WithType(DoorType.Door)
                                .WithFramingBead(MDFDoorOptions.StyleName)
                                .WithPaintColor(MDFDoorOptions.Color == "" ? null : MDFDoorOptions.Color)
                                .Build(height, width);
        doors.Add(door);

        var drawers = getBuilder().WithQty(Qty)
                                    .WithProductNumber(ProductNumber)
                                    .WithType(DoorType.DrawerFront)
                                    .WithFramingBead(MDFDoorOptions.StyleName)
                                    .WithPaintColor(MDFDoorOptions.Color == "" ? null : MDFDoorOptions.Color)
                                    .Build(DrawerFaceHeight, width);
        doors.Add(drawers);

        return doors;

    }

    public IEnumerable<DovetailDrawerBox> GetDrawerBoxes(Func<DovetailDrawerBoxBuilder> getBuilder) {

        var insideWidth = Width - Construction.SideThickness * 2;
        var insideDepth = Depth - (Construction.BackThickness + Construction.BackInset);

        var options = new DrawerBoxOptions("", "", "", "", "Blum", GetNotchFromSlideType(SlideType), "", LogoPosition.None);


        var box = getBuilder().WithInnerCabinetDepth(insideDepth, SlideType)
                                .WithInnerCabinetWidth(insideWidth, 1, SlideType)
                                .WithDrawerFaceHeight(DrawerFaceHeight)
                                .WithQty(Qty)
                                .WithOptions(options)
                                .WithProductNumber(ProductNumber)
                                .Build();

        return new DovetailDrawerBox[] { box };

    }

    public override IEnumerable<Supply> GetSupplies() {

        List<Supply> supplies = new() {
            Supply.DoorPull(Qty),
            Supply.DrawerPull(Qty)
        };

        var boxDepth = DovetailDrawerBoxBuilder.GetDrawerBoxDepthFromInnerCabinetDepth(InnerDepth, SlideType);
        switch (SlideType) {
            case DrawerSlideType.UnderMount:
                supplies.Add(Supply.UndermountSlide(Qty, boxDepth));
                break;

            case DrawerSlideType.SideMount:
                supplies.Add(Supply.SidemountSlide(Qty, boxDepth));
                break;
        }

        if (ToeType is LegLevelers) {

            supplies.Add(Supply.CabinetLeveler(4 * Qty));

        }

        switch (TrashPulloutConfiguration) {
            case TrashPulloutConfiguration.OneCan:
                supplies.Add(Supply.SingleTrashPullout(Qty));
                break;

            case TrashPulloutConfiguration.TwoCans:
                supplies.Add(Supply.DoubleTrashPullout(Qty));
                break;
        }

        return supplies;

    }

    private Dictionary<string, string> GetParameters() {
        var parameters = new Dictionary<string, string> {
            ["ProductW"] = Width.AsMillimeters().ToString(),
            ["ProductH"] = Height.AsMillimeters().ToString(),
            ["ProductD"] = Depth.AsMillimeters().ToString(),
            ["FinishedLeft"] = GetSideOption(LeftSide.Type),
            ["FinishedRight"] = GetSideOption(RightSide.Type),
            ["AppliedPanel"] = GetAppliedPanelOption(),
            ["DrawerH1"] = DrawerFaceHeight.AsMillimeters().ToString()
        };

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

        if (SlideType == DrawerSlideType.SideMount) {
            parameters.Add("_DrawerRunType", "4");
        }

        return parameters;

    }

    private static string GetNotchFromSlideType(DrawerSlideType slide) => slide switch {
        DrawerSlideType.UnderMount => "Standard Notch",
        DrawerSlideType.SideMount => "",
        _ => ""
    };

}