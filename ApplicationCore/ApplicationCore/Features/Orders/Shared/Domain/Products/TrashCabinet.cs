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
    public IEnumerable<PPProduct> GetPPProducts() {
        string doorType = (MDFDoorOptions is null) ? "Slab" : "Buyout";
        yield return new PPProduct(Id, Room, "BT1D1D", ProductNumber, "Royal2", GetMaterialType(), doorType, "Standard", Comment, GetFinishMaterials(), GetEBMaterials(), new Dictionary<string, string>(), new Dictionary<string, string>(), new Dictionary<string, string>());
    }

    public IEnumerable<MDFDoor> GetDoors(Func<MDFDoorBuilder> getBuilder) {

        if (MDFDoorOptions is null) {
            return Enumerable.Empty<MDFDoor>();
        }

        List<MDFDoor> doors = new();

        Dimension width = Width - 2 * DoorGaps.EdgeReveal - DoorGaps.HorizontalGap;
        Dimension height = Height - ToeType.ToeHeight - DoorGaps.TopGap - DoorGaps.BottomGap;
        var door = getBuilder().WithQty(Qty)
                                .WithProductNumber(ProductNumber)
                                .WithType(DoorType.Door)
                                .WithFramingBead(MDFDoorOptions.StyleName)
                                .WithPaintColor(MDFDoorOptions.Color == "" ? null : MDFDoorOptions.Color)
                                .Build(height, width);
        doors.Add(door);

        
        Dimension drwWidth = Width - 2 * DoorGaps.EdgeReveal;
        var drawers = getBuilder().WithQty(Qty)
                                    .WithProductNumber(ProductNumber)
                                    .WithType(DoorType.DrawerFront)
                                    .WithFramingBead(MDFDoorOptions.StyleName)
                                    .WithPaintColor(MDFDoorOptions.Color == "" ? null : MDFDoorOptions.Color)
                                    .Build(DrawerFaceHeight, drwWidth);
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

    private static string GetNotchFromSlideType(DrawerSlideType slide) => slide switch {
        DrawerSlideType.UnderMount => "Standard Notch",
        DrawerSlideType.SideMount => "",
        _ => ""
    };

}