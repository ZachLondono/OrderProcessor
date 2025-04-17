using Domain.Orders.Builders;
using Domain.Orders.Components;
using Domain.Orders.Entities.Hardware;
using Domain.Orders.Enums;
using Domain.Orders.Exceptions;
using Domain.Orders.ValueObjects;
using Domain.ValueObjects;

namespace Domain.Orders.Entities.Products.Cabinets;

public class WallDiagonalCornerCabinet : GarageCabinet, IMDFDoorContainer, ISupplyContainer {

    public Dimension RightWidth { get; }
    public Dimension RightDepth { get; }
    public HingeSide HingeSide { get; }
    public int DoorQty { get; }
    public int AdjustableShelves { get; }
    public Dimension ExtendedDoor { get; }

    public override string GetDescription() => $"Diagonal Corner Wall {(IsGarage ? "Garage" : "")}Cabinet - {DoorQty} Doors";
    public override string GetSimpleDescription() => "Diagonal Corner Wall Cabinet";

    public Dimension DoorHeight => Height - DoorGaps.TopGap - DoorGaps.BottomGap + ExtendedDoor;

    public static CabinetDoorGaps DoorGaps { get; set; } = new() {
        TopGap = Dimension.FromMillimeters(3),
        BottomGap = Dimension.Zero,
        EdgeReveal = Dimension.FromMillimeters(3),
        HorizontalGap = Dimension.FromMillimeters(3),
        VerticalGap = Dimension.FromMillimeters(3),
    };

    public WallDiagonalCornerCabinet(Guid id, int qty, decimal unitPrice, int productNumber, string room, bool assembled,
                        Dimension height, Dimension width, Dimension depth,
                        CabinetMaterial boxMaterial, CabinetFinishMaterial finishMaterial, CabinetDoorConfiguration doorConfiguration, string edgeBandingColor,
                        CabinetSideType rightSideType, CabinetSideType leftSideType, string comment,
                        Dimension rightWidth, Dimension rightDepth, int adjShelfQty, HingeSide hingeSide, int doorQty, Dimension extendedDoor)
                        : base(id, qty, unitPrice, productNumber, room, assembled, height, width, depth, boxMaterial, finishMaterial, doorConfiguration, edgeBandingColor, rightSideType, leftSideType, comment) {

        if (leftSideType == CabinetSideType.AppliedPanel || leftSideType == CabinetSideType.AppliedPanel)
            throw new InvalidProductOptionsException("Wall cabinet cannot have applied panel sides");

        RightWidth = rightWidth;
        RightDepth = rightDepth;
        AdjustableShelves = adjShelfQty;
        HingeSide = hingeSide;
        DoorQty = doorQty;
        ExtendedDoor = extendedDoor;

        if (DoorQty > 2 || DoorQty < 1) throw new InvalidProductOptionsException("Invalid number of doors");

    }

    public static WallDiagonalCornerCabinet Create(int qty, decimal unitPrice, int productNumber, string room, bool assembled,
                        Dimension height, Dimension width, Dimension depth,
                        CabinetMaterial boxMaterial, CabinetFinishMaterial finishMaterial, CabinetDoorConfiguration doorConfiguration, string edgeBandingColor,
                        CabinetSideType rightSideType, CabinetSideType leftSideType, string comment,
                        Dimension rightWidth, Dimension rightDepth, int adjShelfQty, HingeSide hingeSide, int doorQty, Dimension extendedDoor)
                        => new(Guid.NewGuid(), qty, unitPrice, productNumber, room, assembled, height, width, depth, boxMaterial, finishMaterial, doorConfiguration, edgeBandingColor, rightSideType, leftSideType, comment, rightWidth, rightDepth, adjShelfQty, hingeSide, doorQty, extendedDoor);

    public override string GetProductSku() => DoorQty switch {
        1 => $"{(IsGarage ? "G" : "")}WC1D-M",
        2 => $"{(IsGarage ? "G" : "")}WC2D-M",
        _ => $"{(IsGarage ? "G" : "")}WC1D-M"
    };

    public bool ContainsDoors() => DoorConfiguration.IsMDF;

    public override IEnumerable<string> GetNotes() {

        List<string> notes = [
            $"{DoorQty} Doors",
            $"{AdjustableShelves} Adjustable Shelves",
            $"{RightWidth.AsInches():0.00}\" Right Width",
            $"{RightDepth.AsInches():0.00}\" Right Depth",
        ];

        if (ExtendedDoor > Dimension.Zero) {
            notes.Add($"Doors extended down {ExtendedDoor.AsInches():0.00}\"");
        }

        return notes;

    }

    public IEnumerable<MDFDoor> GetDoors(Func<MDFDoorBuilder> getBuilder)
        => DoorConfiguration.Match(
            slab => [],
			mdf => {

				Dimension a = Width - RightDepth - Dimension.FromMillimeters(19);
				Dimension b = RightWidth - Depth - Dimension.FromMillimeters(19);

				Dimension diagOpening = Area.Sqrt(a * a + b * b);

				Dimension width = RoundToHalfMM(diagOpening - 2 * DoorGaps.EdgeReveal);
				if (DoorQty == 2) {
					width = RoundToHalfMM((width - DoorGaps.HorizontalGap) / 2);
				}

				Dimension height = DoorHeight;

				var door = getBuilder().WithQty(DoorQty * Qty)
										.WithProductNumber(ProductNumber)
										.WithFramingBead(mdf.FramingBead)
                                        .WithPaintColor(mdf.Finish.Match<string?>(
                                                    paint => paint.Color,
                                                    _ => null,
                                                    _ => null))
                                        .Build(height, width);

				return new MDFDoor[] { door };

			},
			byothers => []);
        

    public IEnumerable<Supply> GetSupplies() {

        List<Supply> supplies = [];

        if (AdjustableShelves > 0) {

            supplies.Add(Supply.LockingShelfPeg(AdjustableShelves * 4 * Qty));

        }

        if (DoorQty > 0) {

            //supplies.Add(Supply.DoorPull(DoorQty * Qty));
            supplies.AddRange(Supply.CrossCornerHinge(DoorHeight, DoorQty * Qty));

        }

        return supplies;

    }

    public static Dimension RoundToHalfMM(Dimension dim) {

        var value = Math.Round(dim.AsMillimeters() * 2, MidpointRounding.AwayFromZero) / 2;

        return Dimension.FromMillimeters(value);

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
            { "ShelfQ", AdjustableShelves.ToString() },
            { "AppliedPanel", GetAppliedPanelOption() },
            { "LazySusan", "N" },
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
