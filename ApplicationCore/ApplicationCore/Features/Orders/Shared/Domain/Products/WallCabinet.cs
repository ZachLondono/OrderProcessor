using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.ProductPlanner.Contracts;
using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.Products;

internal class WallCabinet : Cabinet, IPPProductContainer, IDoorContainer {

    public WallCabinetDoors Doors { get; }
    public WallCabinetInside Inside { get; }
    public bool FinishedBottom { get; }

    public override string Description => "Wall Cabinet";

    public static CabinetDoorGaps DoorGaps { get; set; } = new() {
        TopGap = Dimension.FromMillimeters(3),
        BottomGap = Dimension.Zero,
        EdgeReveal = Dimension.FromMillimeters(2),
        HorizontalGap = Dimension.FromMillimeters(3),
        VerticalGap = Dimension.FromMillimeters(3),
    };

    public static WallCabinet Create(int qty, decimal unitPrice, int productNumber, string room, bool assembled,
                        Dimension height, Dimension width, Dimension depth,
                        CabinetMaterial boxMaterial, CabinetFinishMaterial finishMaterial, MDFDoorOptions? mdfDoorOptions, string edgeBandingColor,
                        CabinetSide rightSide, CabinetSide leftSide, string comment,
                        WallCabinetDoors doors, WallCabinetInside inside, bool finishedBottom) {
        return new(Guid.NewGuid(), qty, unitPrice, productNumber, room, assembled, height, width, depth, boxMaterial, finishMaterial, mdfDoorOptions, edgeBandingColor, rightSide, leftSide, comment, doors, inside, finishedBottom);
    }

    private WallCabinet(Guid id, int qty, decimal unitPrice, int productNumber, string room, bool assembled,
                        Dimension height, Dimension width, Dimension depth,
                        CabinetMaterial boxMaterial, CabinetFinishMaterial finishMaterial, MDFDoorOptions? mdfDoorOptions, string edgeBandingColor,
                        CabinetSide rightSide, CabinetSide leftSide, string comment,
                        WallCabinetDoors doors, WallCabinetInside inside, bool finishedBottom)
                        : base(id, qty, unitPrice, productNumber, room, assembled, height, width, depth, boxMaterial, finishMaterial, mdfDoorOptions, edgeBandingColor, rightSide, leftSide, comment) {

        if (leftSide.Type == CabinetSideType.AppliedPanel || rightSide.Type == CabinetSideType.AppliedPanel)
            throw new InvalidOperationException("Wall cabinet cannot have applied panel sides");

        if (doors.Quantity > 2 || doors.Quantity < 0)
            throw new InvalidOperationException("Invalid number of doors");

        Doors = doors;
        Inside = inside;
        FinishedBottom = finishedBottom;

    }

    public IEnumerable<PPProduct> GetPPProducts() {
        string doorType = (MDFDoorOptions is null) ? "Slab" : "Buyout";
        yield return new PPProduct(Id, Room, GetProductName(), ProductNumber, "Royal2", GetMaterialType(), doorType, "Standard", Comment, GetFinishMaterials(), GetEBMaterials(), GetParameters(), GetParameterOverrides(), new Dictionary<string, string>());
    }

    public IEnumerable<MDFDoor> GetDoors(Func<MDFDoorBuilder> getBuilder) {

        if (MDFDoorOptions is null) {
            return Enumerable.Empty<MDFDoor>();
        }

        if (Doors.Quantity == 0) {
            return Enumerable.Empty<MDFDoor>();
        }

        Dimension width = (Width - 2 * DoorGaps.EdgeReveal - DoorGaps.HorizontalGap * (Doors.Quantity - 1)) / Doors.Quantity;
        Dimension height = Height - DoorGaps.TopGap - DoorGaps.BottomGap;
        var door = getBuilder().WithQty(Doors.Quantity * Qty)
                                .WithType(DoorType.Door)
                                .WithProductNumber(ProductNumber)
                                .WithFramingBead(MDFDoorOptions.StyleName)
                                .WithPaintColor(MDFDoorOptions.Color == "" ? null : MDFDoorOptions.Color)
                                .Build(height, width);

        return new MDFDoor[] { door };

    }

    private string GetProductName() {
        return $"W{Doors.Quantity}D";
    }

    private Dictionary<string, string> GetParameters() {
        var parameters = new Dictionary<string, string>() {
            { "ProductW", Width.AsMillimeters().ToString() },
            { "ProductH", Height.AsMillimeters().ToString() },
            { "ProductD", Depth.AsMillimeters().ToString() },
            { "FinishedLeft", GetSideOption(LeftSide.Type) },
            { "FinishedRight", GetSideOption(RightSide.Type) },
            { "ShelfQ", Inside.AdjustableShelves.ToString() },
            { "DividerQ", Inside.VerticalDividers.ToString() }
        };

        if (Doors.HingeSide != HingeSide.NotApplicable) {
            parameters.Add("HingeLeft", GetHingeSideOption());
        }

        if (Doors.ExtendDown != Dimension.Zero && (LeftSide.Type == CabinetSideType.Finished || RightSide.Type == CabinetSideType.Finished)) {
            parameters.Add("LightRailW", Doors.ExtendDown.AsMillimeters().ToString());
        }

        return parameters;
    }

    private Dictionary<string, string> GetParameterOverrides() {

        var parameters = new Dictionary<string, string>();

        if (Doors.ExtendDown != Dimension.Zero && LeftSide.Type != CabinetSideType.Finished && RightSide.Type != CabinetSideType.Finished) {
            parameters.Add("ExtendDoorD", Doors.ExtendDown.AsMillimeters().ToString());
        }

        if (FinishedBottom) {
            parameters.Add("_FinishedWallBot", "1");
        }

        return parameters;

    }

    private string GetHingeSideOption() => Doors.HingeSide switch {
        HingeSide.NotApplicable => "0",
        HingeSide.Left => "1",
        HingeSide.Right => "0",
        _ => "0"
    };

}