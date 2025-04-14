using Domain.Orders.Builders;
using Domain.Orders.Components;
using Domain.Orders.Entities.Hardware;
using Domain.Orders.Enums;
using Domain.Orders.Exceptions;
using Domain.Orders.ValueObjects;
using Domain.ValueObjects;

namespace Domain.Orders.Entities.Products.Cabinets;

public class WallPieCutCornerCabinet : Cabinet, IMDFDoorContainer, ISupplyContainer {

    public Dimension RightWidth { get; }
    public Dimension RightDepth { get; }
    public int AdjustableShelves { get; }
    public HingeSide HingeSide { get; }
    public Dimension ExtendedDoor { get; }

    public override string GetDescription() => "Pie Cut Corner Wall Cabinet";
    public override string GetSimpleDescription() => "Pie Cut Corner Wall Cabinet";

    public Dimension DoorHeight => Height - DoorGaps.TopGap + ExtendedDoor;

    public static CabinetDoorGaps DoorGaps { get; set; } = new() {
        TopGap = Dimension.FromMillimeters(3),
        BottomGap = Dimension.Zero,
        EdgeReveal = Dimension.FromMillimeters(2),
        HorizontalGap = Dimension.FromMillimeters(3),
        VerticalGap = Dimension.FromMillimeters(3),
    };

    public WallPieCutCornerCabinet(Guid id, int qty, decimal unitPrice, int productNumber, string room, bool assembled,
                        Dimension height, Dimension width, Dimension depth,
                        CabinetMaterial boxMaterial, CabinetFinishMaterial finishMaterial, CabinetDoorConfiguration doorConfiguration, string edgeBandingColor,
                        CabinetSideType rightSideType, CabinetSideType leftSideType, string comment,
                        Dimension rightWidth, Dimension rightDepth, int adjustableShelves, HingeSide hingeSide, Dimension extendedDoor)
                        : base(id, qty, unitPrice, productNumber, room, assembled, height, width, depth, boxMaterial, finishMaterial, doorConfiguration, edgeBandingColor, rightSideType, leftSideType, comment) {

        if (leftSideType == CabinetSideType.AppliedPanel || leftSideType == CabinetSideType.AppliedPanel)
            throw new InvalidProductOptionsException("Wall cabinet cannot have applied panel sides");

        RightWidth = rightWidth;
        RightDepth = rightDepth;
        AdjustableShelves = adjustableShelves;
        HingeSide = hingeSide;
        ExtendedDoor = extendedDoor;
    }

    public static WallPieCutCornerCabinet Create(int qty, decimal unitPrice, int productNumber, string room, bool assembled,
                        Dimension height, Dimension width, Dimension depth,
                        CabinetMaterial boxMaterial, CabinetFinishMaterial finishMaterial, CabinetDoorConfiguration doorConfiguration, string edgeBandingColor,
                        CabinetSideType rightSideType, CabinetSideType leftSideType, string comment,
                        Dimension rightWidth, Dimension rightDepth, int adjustableShelves, HingeSide hingeSide, Dimension extendedDoor)
    => new(Guid.NewGuid(), qty, unitPrice, productNumber, room, assembled, height, width, depth, boxMaterial, finishMaterial, doorConfiguration, edgeBandingColor, rightSideType, leftSideType, comment, rightWidth, rightDepth, adjustableShelves, hingeSide, extendedDoor);

    public bool ContainsDoors() => DoorConfiguration.IsMDF;

    public IEnumerable<MDFDoor> GetDoors(Func<MDFDoorBuilder> getBuilder)
        => DoorConfiguration.Match(
            slab => [],
			mdf => {

				Dimension height = DoorHeight;
				Dimension doorThickness = Dimension.FromMillimeters(19);
				Dimension bumperWidth = Dimension.FromMillimeters(3);

				Dimension leftWidth = Width - RightDepth - bumperWidth - doorThickness - DoorGaps.EdgeReveal;
				MDFDoor leftDoor = getBuilder().WithQty(Qty)
												.WithProductNumber(ProductNumber)
												.WithFramingBead(mdf.FramingBead)
												.WithPaintColor(mdf.PaintColor == "" ? null : mdf.PaintColor)
												.Build(height, leftWidth);

				Dimension rightWidth = RightWidth - Depth - bumperWidth - doorThickness - DoorGaps.EdgeReveal;
				MDFDoor rightDoor = getBuilder().WithQty(Qty)
												.WithProductNumber(ProductNumber)
												.WithFramingBead(mdf.FramingBead)
												.WithPaintColor(mdf.PaintColor == "" ? null : mdf.PaintColor)
												.Build(height, rightWidth);

				return new List<MDFDoor>() { leftDoor, rightDoor };

			},
			byothers => []);

    public IEnumerable<Supply> GetSupplies() {

        List<Supply> supplies = [
            //Supply.DoorPull(Qty),
            .. Supply.FullOverlayHinge(DoorHeight, Qty)
        ];

        if (AdjustableShelves > 0) {

            supplies.Add(Supply.LockingShelfPeg(AdjustableShelves * 5 * Qty));

        }

        return supplies;

    }

    public override string GetProductSku() => "WCPC";

    public override IEnumerable<string> GetNotes() {

        List<string> notes = [
            $"{AdjustableShelves} Adjustable Shelves",
            $"{RightWidth.AsInches():0.00}\" Right Width",
            $"{RightDepth.AsInches():0.00}\" Right Depth",
        ];

        if (ExtendedDoor > Dimension.Zero) {
            notes.Add($"Doors extended down {ExtendedDoor.AsInches():0.00}\"");
        }

        return notes;

    }

    protected override IDictionary<string, string> GetParameters() {
        var parameters = new Dictionary<string, string>() {
            { "ProductW", Width.AsMillimeters().ToString() },
            { "ProductWRight", RightWidth.AsMillimeters().ToString() },
            { "ProductH", Height.AsMillimeters().ToString() },
            { "ProductD", Depth.AsMillimeters().ToString() },
            { "ProductDRight", RightDepth.AsMillimeters().ToString() },
            { "FinishedLeft", GetSideOption(LeftSideType) },
            { "FinishedRight", GetSideOption(RightSideType) },
            { "ShelfQ", AdjustableShelves.ToString() }
        };

        if (HingeSide != HingeSide.NotApplicable) {
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

    private string GetHingeSideOption() => HingeSide switch {
        HingeSide.NotApplicable => "0",
        HingeSide.Left => "1",
        HingeSide.Right => "0",
        _ => "0"
    };

}
