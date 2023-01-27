using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.ProductPlanner.Contracts;
using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.Products;

internal class BlindWallCabinet : Cabinet, IPPProductContainer, IDoorContainer {

    public BlindCabinetDoors Doors { get; }
    public int AdjustableShelves { get; }
    public BlindSide BlindSide { get; }
    public Dimension BlindWidth { get; }

    public static CabinetDoorGaps DoorGaps { get; set; } = new() {
        TopGap = Dimension.FromMillimeters(3),
        BottomGap = Dimension.Zero,
        EdgeReveal = Dimension.FromMillimeters(2),
        HorizontalGap = Dimension.FromMillimeters(3),
        VerticalGap = Dimension.FromMillimeters(3),
    };

    public static BlindWallCabinet Create(int qty, decimal unitPrice, int productNumber, string room, bool assembled,
                        Dimension height, Dimension width, Dimension depth,
                        CabinetMaterial boxMaterial, CabinetFinishMaterial finishMaterial, string edgeBandingColor,
                        CabinetSide rightSide, CabinetSide leftSide,
                        BlindCabinetDoors doors, BlindSide blindSide, Dimension blindWidth, int adjustableShelves) {
        return new(Guid.NewGuid(), qty, unitPrice, productNumber, room, assembled, height, width, depth, boxMaterial, finishMaterial, edgeBandingColor, rightSide, leftSide, doors, blindSide, blindWidth, adjustableShelves);
    }

    private BlindWallCabinet(Guid id, int qty, decimal unitPrice, int productNumber, string room, bool assembled,
                        Dimension height, Dimension width, Dimension depth,
                        CabinetMaterial boxMaterial, CabinetFinishMaterial finishMaterial, string edgeBandingColor,
                        CabinetSide rightSide, CabinetSide leftSide,
                        BlindCabinetDoors doors, BlindSide blindSide, Dimension blindWidth, int adjustableShelves)
                        : base(id, qty, unitPrice, productNumber, room, assembled, height, width, depth, boxMaterial, finishMaterial, edgeBandingColor, rightSide, leftSide) {

        Doors = doors;
        AdjustableShelves = adjustableShelves;
        BlindSide = blindSide;
        BlindWidth = blindWidth;

    }

    public IEnumerable<PPProduct> GetPPProducts() {
        string doorType = (Doors.MDFOptions is null) ? "Slab" : "Buyout";
        yield return new PPProduct(Room, GetProductName(), "Royal2", GetMaterialType(), doorType, "Standard", GetFinishMaterials(), GetEBMaterials(), GetParameters(), new(), new());
    }

    public IEnumerable<MDFDoor> GetDoors(Func<MDFDoorBuilder> getBuilder) {

        if (Doors.MDFOptions is null) {
            return Enumerable.Empty<MDFDoor>();
        }

        if (Doors.Quantity < 0) {
            return Enumerable.Empty<MDFDoor>();
        }

        Dimension width = (Width - BlindWidth - DoorGaps.EdgeReveal - DoorGaps.HorizontalGap / 2 - DoorGaps.HorizontalGap * (Doors.Quantity - 1)) / Doors.Quantity;
        Dimension height = Height - DoorGaps.TopGap - DoorGaps.BottomGap;
        var door = getBuilder().WithQty(Doors.Quantity * Qty)
                                .WithProductNumber(ProductNumber)
                                .WithType(DoorType.Door)
                                .Build(height, width);


        return new MDFDoor[] { door };

    }

    private string GetProductName() {
        return $"WB{Doors.Quantity}D{GetBlindSideLetter()}";
    }

    private Dictionary<string, string> GetParameters() {
        var parameters = new Dictionary<string, string>() {
            { "ProductW", Width.AsMillimeters().ToString() },
            { "ProductH", Height.AsMillimeters().ToString() },
            { "ProductD", Depth.AsMillimeters().ToString() },
            { "FinishedLeft", GetSideOption(LeftSide.Type) },
            { "FinishedRight", GetSideOption(RightSide.Type) },
            { "ShelfQ", AdjustableShelves.ToString() },
            { "BlindWWall", BlindWidth.AsMillimeters().ToString() }
        };

        if (Doors.HingeSide != HingeSide.NotApplicable) {
            parameters.Add("HingeLeft", GetHingeSideOption());
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