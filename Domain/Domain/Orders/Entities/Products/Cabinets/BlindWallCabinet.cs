using Domain.Orders.Builders;
using Domain.Orders.Components;
using Domain.Orders.Enums;
using Domain.Orders.ValueObjects;
using Domain.ValueObjects;

namespace Domain.Orders.Entities.Products.Cabinets;

public class BlindWallCabinet : GarageCabinet, IMDFDoorContainer {

    public BlindCabinetDoors Doors { get; }
    public int AdjustableShelves { get; }
    public BlindSide BlindSide { get; }
    public Dimension BlindWidth { get; }
    public Dimension ExtendedDoor { get; }

    public Dimension DoorHeight => Height - DoorGaps.TopGap - DoorGaps.BottomGap + ExtendedDoor;

    public override string GetDescription() => $"Blind {(IsGarage ? "Garage " : "")}Wall Cabinet - {Doors.Quantity} Doors";

    public static CabinetDoorGaps DoorGaps { get; set; } = new() {
        TopGap = Dimension.FromMillimeters(3),
        BottomGap = Dimension.Zero,
        EdgeReveal = Dimension.FromMillimeters(2),
        HorizontalGap = Dimension.FromMillimeters(3),
        VerticalGap = Dimension.FromMillimeters(3),
    };

    public static BlindWallCabinet Create(int qty, decimal unitPrice, int productNumber, string room, bool assembled,
                        Dimension height, Dimension width, Dimension depth,
                        CabinetMaterial boxMaterial, CabinetFinishMaterial finishMaterial, CabinetSlabDoorMaterial? slabDoorMaterial, MDFDoorOptions? mdfDoorOptions, string edgeBandingColor,
                        CabinetSideType rightSideType, CabinetSideType leftSideType, string comment,
                        BlindCabinetDoors doors, BlindSide blindSide, Dimension blindWidth, int adjustableShelves, Dimension extendedDoor) {
        return new(Guid.NewGuid(), qty, unitPrice, productNumber, room, assembled, height, width, depth, boxMaterial, finishMaterial, slabDoorMaterial, mdfDoorOptions, edgeBandingColor, rightSideType, leftSideType, comment, doors, blindSide, blindWidth, adjustableShelves, extendedDoor);
    }

    public BlindWallCabinet(Guid id, int qty, decimal unitPrice, int productNumber, string room, bool assembled,
                        Dimension height, Dimension width, Dimension depth,
                        CabinetMaterial boxMaterial, CabinetFinishMaterial finishMaterial, CabinetSlabDoorMaterial? slabDoorMaterial, MDFDoorOptions? mdfDoorOptions, string edgeBandingColor,
                        CabinetSideType rightSideType, CabinetSideType leftSideType, string comment,
                        BlindCabinetDoors doors, BlindSide blindSide, Dimension blindWidth, int adjustableShelves, Dimension extendedDoor)
                        : base(id, qty, unitPrice, productNumber, room, assembled, height, width, depth, boxMaterial, finishMaterial, slabDoorMaterial, mdfDoorOptions, edgeBandingColor, rightSideType, leftSideType, comment) {

        Doors = doors;
        AdjustableShelves = adjustableShelves;
        BlindSide = blindSide;
        BlindWidth = blindWidth;
        ExtendedDoor = extendedDoor;

    }

    public bool ContainsDoors() => MDFDoorOptions is not null;

    public IEnumerable<MDFDoor> GetDoors(Func<MDFDoorBuilder> getBuilder) {

        if (MDFDoorOptions is null) {
            return Enumerable.Empty<MDFDoor>();
        }

        if (Doors.Quantity < 0) {
            return Enumerable.Empty<MDFDoor>();
        }

        Dimension width = (Width - BlindWidth - DoorGaps.EdgeReveal - DoorGaps.HorizontalGap / 2 - DoorGaps.HorizontalGap * (Doors.Quantity - 1)) / Doors.Quantity;
        Dimension height = DoorHeight;
        var door = getBuilder().WithQty(Doors.Quantity * Qty)
                                .WithProductNumber(ProductNumber)
                                .WithType(DoorType.Door)
                                .WithFramingBead(MDFDoorOptions.FramingBead)
                                .WithPaintColor(MDFDoorOptions.PaintColor == "" ? null : MDFDoorOptions.PaintColor)
                                .Build(height, width);


        return new MDFDoor[] { door };

    }

    public override IEnumerable<Supply> GetSupplies() {

        var supplies = new List<Supply>();

        if (AdjustableShelves > 0) {

            supplies.Add(Supply.LockingShelfPeg(4 * AdjustableShelves * Qty));

        }

        if (Doors.Quantity > 0) {

            supplies.Add(Supply.DoorPull(Qty * Doors.Quantity));
            supplies.AddRange(Supply.StandardHinge(DoorHeight, Qty * Doors.Quantity));

        }

        return supplies;

    }

    public override string GetProductSku() {
        return $"WB{Doors.Quantity}D{GetBlindSideLetter()}";
    }

    protected override IDictionary<string, string> GetParameters() {
        var parameters = new Dictionary<string, string>() {
            { "ProductW", Width.AsMillimeters().ToString() },
            { "ProductH", Height.AsMillimeters().ToString() },
            { "ProductD", Depth.AsMillimeters().ToString() },
            { "FinishedLeft", GetSideOption(LeftSideType) },
            { "FinishedRight", GetSideOption(RightSideType) },
            { "ShelfQ", AdjustableShelves.ToString() },
            { "BlindWWall", BlindWidth.AsMillimeters().ToString() }
        };

        if (Doors.HingeSide != HingeSide.NotApplicable) {
            parameters.Add("HingeLeft", GetHingeSideOption());
        }

        if (ExtendedDoor != Dimension.Zero && (LeftSideType == CabinetSideType.Finished || RightSideType == CabinetSideType.Finished)) {
            parameters.Add("LightRailW", ExtendedDoor.AsMillimeters().ToString());
        }

        return parameters;
    }

    protected override IDictionary<string, string> GetParameterOverrides() {

        var parameters = new Dictionary<string, string>();

        if (ExtendedDoor != Dimension.Zero && LeftSideType != CabinetSideType.Finished && RightSideType != CabinetSideType.Finished) {
            parameters.Add("ExtendDoorD", ExtendedDoor.AsMillimeters().ToString());
        }

        return parameters;

    }

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