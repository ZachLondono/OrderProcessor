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
    public MDFDoorOptions? MDFOptions { get; }

    public CabinetDoorGaps DoorGaps { get; set; } = new();

    public WallPieCutCornerCabinet(Guid id, int qty, decimal unitPrice, string room, bool assembled,
                        Dimension height, Dimension width, Dimension depth,
                        CabinetMaterial boxMaterial, CabinetFinishMaterial finishMaterial, string edgeBandingColor,
                        CabinetSide rightSide, CabinetSide leftSide,
                        Dimension rightWidth, Dimension rightDepth, int adjustableShelves, HingeSide hingeSide, MDFDoorOptions? mdfDoorOptions)
                        : base(id, qty, unitPrice, room, assembled, height, width, depth, boxMaterial, finishMaterial, edgeBandingColor, rightSide, leftSide) {

        if (leftSide.Type == CabinetSideType.AppliedPanel || rightSide.Type == CabinetSideType.AppliedPanel)
            throw new InvalidOperationException("Wall cabinet cannot have applied panel sides");

        RightWidth = rightWidth;
        RightDepth = rightDepth;
        AdjustableShelves = adjustableShelves;
        HingeSide = hingeSide;
        MDFOptions = mdfDoorOptions;
    }

    public static WallPieCutCornerCabinet Create(int qty, decimal unitPrice, string room, bool assembled,
                        Dimension height, Dimension width, Dimension depth,
                        CabinetMaterial boxMaterial, CabinetFinishMaterial finishMaterial, string edgeBandingColor,
                        CabinetSide rightSide, CabinetSide leftSide,
                        Dimension rightWidth, Dimension rightDepth, int adjustableShelves, HingeSide hingeSide, MDFDoorOptions? mdfDoorOptions)
    => new(Guid.NewGuid(), qty, unitPrice, room, assembled, height, width, depth, boxMaterial, finishMaterial, edgeBandingColor, rightSide, leftSide, rightWidth, rightDepth, adjustableShelves, hingeSide, mdfDoorOptions);


    public IEnumerable<PPProduct> GetPPProducts() {
        string doorType = (MDFOptions is null) ? "Slab" : "Buyout";
        yield return new PPProduct(Room, "WCPC", "Royal2", GetMaterialType(), doorType, "Standard", GetFinishMaterials(), GetEBMaterials(), GetParameters(), new(), new());
    }

    public IEnumerable<MDFDoor> GetDoors(Func<MDFDoorBuilder> getBuilder) {

        if (MDFOptions is null) {
            return Enumerable.Empty<MDFDoor>();
        }

        Dimension height = Height - DoorGaps.TopGap;
        Dimension doorThickness = Dimension.FromMillimeters(19);
        Dimension bumperWidth = Dimension.FromMillimeters(3);

        Dimension leftWidth = Width - RightDepth - bumperWidth - doorThickness - DoorGaps.EdgeReveal;
        MDFDoor leftDoor = getBuilder().WithQty(Qty).Build(height, leftWidth);

        Dimension rightWidth = RightWidth - Depth - bumperWidth - doorThickness - DoorGaps.EdgeReveal;
        MDFDoor rightDoor = getBuilder().WithQty(Qty).Build(height, rightWidth);

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

        return parameters;
    }

    private string GetHingeSideOption() => HingeSide switch {
        HingeSide.NotApplicable => "0",
        HingeSide.Left => "1",
        HingeSide.Right => "0",
        _ => "0"
    };

}
