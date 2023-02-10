using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.ProductPlanner.Contracts;
using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.Products;

internal class WallPieCutCornerCabinet : Cabinet, IPPProductContainer, IDoorContainer {

    public Dimension RightWidth { get; }
    public Dimension RightDepth { get; }
    public int AdjustableShelves { get; }
    public HingeSide HingeSide { get; }
    public Dimension ExtendedDoor { get; }

    public override string Description => "Pie Cut Corner Wall Cabinet";

    public static CabinetDoorGaps DoorGaps { get; set; } = new() {
        TopGap = Dimension.FromMillimeters(3),
        BottomGap = Dimension.Zero,
        EdgeReveal = Dimension.FromMillimeters(2),
        HorizontalGap = Dimension.FromMillimeters(3),
        VerticalGap = Dimension.FromMillimeters(3),
    };

    public WallPieCutCornerCabinet(Guid id, int qty, decimal unitPrice, int productNumber, string room, bool assembled,
                        Dimension height, Dimension width, Dimension depth,
                        CabinetMaterial boxMaterial, CabinetFinishMaterial finishMaterial, MDFDoorOptions? mdfDoorOptions, string edgeBandingColor,
                        CabinetSide rightSide, CabinetSide leftSide, string comment,
                        Dimension rightWidth, Dimension rightDepth, int adjustableShelves, HingeSide hingeSide, Dimension extendedDoor)
                        : base(id, qty, unitPrice, productNumber, room, assembled, height, width, depth, boxMaterial, finishMaterial, mdfDoorOptions, edgeBandingColor, rightSide, leftSide, comment) {

        if (leftSide.Type == CabinetSideType.AppliedPanel || rightSide.Type == CabinetSideType.AppliedPanel)
            throw new InvalidOperationException("Wall cabinet cannot have applied panel sides");

        RightWidth = rightWidth;
        RightDepth = rightDepth;
        AdjustableShelves = adjustableShelves;
        HingeSide = hingeSide;
        ExtendedDoor = extendedDoor;
    }

    public static WallPieCutCornerCabinet Create(int qty, decimal unitPrice, int productNumber, string room, bool assembled,
                        Dimension height, Dimension width, Dimension depth,
                        CabinetMaterial boxMaterial, CabinetFinishMaterial finishMaterial, MDFDoorOptions? mdfDoorOptions, string edgeBandingColor,
                        CabinetSide rightSide, CabinetSide leftSide, string comment,
                        Dimension rightWidth, Dimension rightDepth, int adjustableShelves, HingeSide hingeSide, Dimension extendedDoor)
    => new(Guid.NewGuid(), qty, unitPrice, productNumber, room, assembled, height, width, depth, boxMaterial, finishMaterial, mdfDoorOptions, edgeBandingColor, rightSide, leftSide, comment, rightWidth, rightDepth, adjustableShelves, hingeSide, extendedDoor);


    public IEnumerable<PPProduct> GetPPProducts() {
        string doorType = (MDFDoorOptions is null) ? "Slab" : "Buyout";
        yield return new PPProduct(Id, Room, "WCPC", ProductNumber, "Royal2", GetMaterialType(), doorType, "Standard", Comment, GetFinishMaterials(), GetEBMaterials(), GetParameters(), GetParameterOverrides(), new Dictionary<string, string>());
    }

    public IEnumerable<MDFDoor> GetDoors(Func<MDFDoorBuilder> getBuilder) {

        if (MDFDoorOptions is null) {
            return Enumerable.Empty<MDFDoor>();
        }

        Dimension height = Height - DoorGaps.TopGap;
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

    private Dictionary<string, string> GetParameters() {
        var parameters = new Dictionary<string, string>() {
            { "ProductW", Width.AsMillimeters().ToString() },
            { "ProductWRight", RightWidth.AsMillimeters().ToString() },
            { "ProductH", Height.AsMillimeters().ToString() },
            { "ProductD", Depth.AsMillimeters().ToString() },
            { "ProductDRight", RightDepth.AsMillimeters().ToString() },
            { "FinishedLeft", GetSideOption(LeftSide.Type) },
            { "FinishedRight", GetSideOption(RightSide.Type) },
            { "ShelfQ", AdjustableShelves.ToString() }
        };

        if (HingeSide != HingeSide.NotApplicable) {
            parameters.Add("HingeLeft", GetHingeSideOption());
        }

        if (ExtendedDoor != Dimension.Zero && (LeftSide.Type == CabinetSideType.Finished || RightSide.Type == CabinetSideType.Finished)) {
            parameters.Add("LightRailW", ExtendedDoor.AsMillimeters().ToString());
        }

        return parameters;
    }

    private Dictionary<string, string> GetParameterOverrides() {

        var parameters = new Dictionary<string, string>();

        if (ExtendedDoor != Dimension.Zero && LeftSide.Type != CabinetSideType.Finished && RightSide.Type != CabinetSideType.Finished) {
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
