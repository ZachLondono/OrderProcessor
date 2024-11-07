using Domain.Orders.Builders;
using Domain.Orders.Components;
using Domain.Orders.Entities.Hardware;
using Domain.Orders.Enums;
using Domain.Orders.Exceptions;
using Domain.Orders.ValueObjects;
using Domain.ValueObjects;

namespace Domain.Orders.Entities.Products.Cabinets;

public class WallCabinet : GarageCabinet, IMDFDoorContainer, ISupplyContainer {

    public WallCabinetDoors Doors { get; }
    public WallCabinetInside Inside { get; }
    public bool FinishedBottom { get; }

    public override string GetDescription() => $"Wall {(IsGarage ? "Garage " : "")}Cabinet - {Doors.Quantity} Doors";
    public override string GetSimpleDescription() => "Wall Cabinet";

    public Dimension DoorHeight => Height - DoorGaps.TopGap - DoorGaps.BottomGap + Doors.ExtendDown;

    public static CabinetDoorGaps DoorGaps { get; set; } = new() {
        TopGap = Dimension.FromMillimeters(3),
        BottomGap = Dimension.Zero,
        EdgeReveal = Dimension.FromMillimeters(2),
        HorizontalGap = Dimension.FromMillimeters(3),
        VerticalGap = Dimension.FromMillimeters(3),
    };

    public static WallCabinet Create(int qty, decimal unitPrice, int productNumber, string room, bool assembled,
                        Dimension height, Dimension width, Dimension depth,
                        CabinetMaterial boxMaterial, CabinetFinishMaterial finishMaterial, CabinetDoorConfiguration doorConfiguration, string edgeBandingColor,
                        CabinetSideType rightSideType, CabinetSideType leftSideType, string comment,
                        WallCabinetDoors doors, WallCabinetInside inside, bool finishedBottom) {
        return new(Guid.NewGuid(), qty, unitPrice, productNumber, room, assembled, height, width, depth, boxMaterial, finishMaterial, doorConfiguration, edgeBandingColor, rightSideType, leftSideType, comment, doors, inside, finishedBottom);
    }

    public WallCabinet(Guid id, int qty, decimal unitPrice, int productNumber, string room, bool assembled,
                        Dimension height, Dimension width, Dimension depth,
                        CabinetMaterial boxMaterial, CabinetFinishMaterial finishMaterial, CabinetDoorConfiguration doorConfiguration, string edgeBandingColor,
                        CabinetSideType rightSideType, CabinetSideType leftSideType, string comment,
                        WallCabinetDoors doors, WallCabinetInside inside, bool finishedBottom)
                        : base(id, qty, unitPrice, productNumber, room, assembled, height, width, depth, boxMaterial, finishMaterial, doorConfiguration, edgeBandingColor, rightSideType, leftSideType, comment) {

        if (leftSideType == CabinetSideType.AppliedPanel || rightSideType == CabinetSideType.AppliedPanel)
            throw new InvalidProductOptionsException("Wall cabinet cannot have applied panel sides");

        if (doors.Quantity > 2 || doors.Quantity < 0)
            throw new InvalidProductOptionsException("Invalid number of doors");

        Doors = doors;
        Inside = inside;
        FinishedBottom = finishedBottom;

    }

    public override IEnumerable<string> GetNotes() {

        List<string> notes = [
            $"{Doors.Quantity} Doors",
            $"{Inside.AdjustableShelves} Adjustable Shelves",
            $"{Inside.VerticalDividers} Vertical Dividers",
        ];

        if (Doors.ExtendDown > Dimension.Zero) {
            notes.Add($"Doors extended down {Doors.ExtendDown.AsInches()}\"");
        }

        return notes;

    }

    public bool ContainsDoors() => DoorConfiguration.IsMDF;

    public IEnumerable<MDFDoor> GetDoors(Func<MDFDoorBuilder> getBuilder)
        => DoorConfiguration.Match(
            slab => [],
            mdf => {

				if (Doors.Quantity == 0) {
					return Enumerable.Empty<MDFDoor>();
				}

				Dimension width = (Width - 2 * DoorGaps.EdgeReveal - DoorGaps.HorizontalGap * (Doors.Quantity - 1)) / Doors.Quantity;
				Dimension height = DoorHeight;
				var door = getBuilder().WithQty(Doors.Quantity * Qty)
										.WithType(DoorType.Door)
										.WithProductNumber(ProductNumber)
										.WithFramingBead(mdf.FramingBead)
										.WithPaintColor(mdf.PaintColor == "" ? null : mdf.PaintColor)
										.Build(height, width);

				return [door];

			},
			byothers => []);
        

    public IEnumerable<Supply> GetSupplies() {

        List<Supply> supplies = [];

        if (Inside.AdjustableShelves > 0) {

            supplies.Add(Supply.LockingShelfPeg(Inside.AdjustableShelves * 4 * Qty));

        }

        if (Doors.Quantity > 0) {

            //supplies.Add(Supply.DoorPull(Doors.Quantity * Qty));
            supplies.AddRange(Supply.StandardHinge(DoorHeight, Doors.Quantity * Qty));

        }

        return supplies;

    }

    public override string GetProductSku() => $"{(IsGarage ? "G" : "")}W{Doors.Quantity}D";

    protected override IDictionary<string, string> GetParameters() {
        var parameters = new Dictionary<string, string>() {
            { "ProductW", Width.AsMillimeters().ToString() },
            { "ProductH", Height.AsMillimeters().ToString() },
            { "ProductD", Depth.AsMillimeters().ToString() },
            { "FinishedLeft", GetSideOption(LeftSideType) },
            { "FinishedRight", GetSideOption(RightSideType) },
            { "ShelfQ", Inside.AdjustableShelves.ToString() },
            { "DividerQ", Inside.VerticalDividers.ToString() }
        };

        if (Doors.HingeSide != HingeSide.NotApplicable) {
            parameters.Add("HingeLeft", GetHingeSideOption());
        }

        if (Doors.ExtendDown != Dimension.Zero && (LeftSideType == CabinetSideType.Finished || RightSideType == CabinetSideType.Finished)) {
            parameters.Add("LightRailW", Doors.ExtendDown.AsMillimeters().ToString());
        }

        return parameters;
    }

    protected override IDictionary<string, string> GetParameterOverrides() {

        var parameters = new Dictionary<string, string>();

        if (Doors.ExtendDown != Dimension.Zero && LeftSideType != CabinetSideType.Finished && RightSideType != CabinetSideType.Finished) {
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