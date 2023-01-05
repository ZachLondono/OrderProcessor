using ApplicationCore.Features.Orders.Domain.ValueObjects;
using ApplicationCore.Features.ProductPlanner.Contracts;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.Orders.Domain.Products;

internal class WallCabinet : Cabinet, IPPProductContainer {

    public WallCabinetDoors Doors { get; }
    public WallCabinetInside Inside { get; }

    public static WallCabinet Create(int qty, decimal unitPrice, string room, bool assembled,
                        Dimension height, Dimension width, Dimension depth,
                        CabinetMaterial boxMaterial, CabinetMaterial finishMaterial, string edgeBandingColor,
                        CabinetSide rightSide, CabinetSide leftSide,
                        WallCabinetDoors doors, WallCabinetInside inside) {
        return new(Guid.NewGuid(), qty, unitPrice, room, assembled, height, width, depth, boxMaterial, finishMaterial, edgeBandingColor, rightSide, leftSide, doors, inside);
    }

    private WallCabinet(Guid id, int qty, decimal unitPrice, string room, bool assembled,
                        Dimension height, Dimension width, Dimension depth,
                        CabinetMaterial boxMaterial, CabinetMaterial finishMaterial, string edgeBandingColor,
                        CabinetSide rightSide, CabinetSide leftSide,
                        WallCabinetDoors doors, WallCabinetInside inside)
                        : base(id, qty, unitPrice, room, assembled, height, width, depth, boxMaterial, finishMaterial, edgeBandingColor, rightSide, leftSide) {

        if (leftSide.Type == CabinetSideType.AppliedPanel || rightSide.Type == CabinetSideType.AppliedPanel)
            throw new InvalidOperationException("Wall cabinet cannot have applied panel sides");

        if (doors.Quantity > 2 || doors.Quantity < 0)
            throw new InvalidOperationException("Invalid number of doors");

        Doors = doors;
        Inside = inside;

    }

    public IEnumerable<PPProduct> GetPPProducts() {
        // TODO: add option for no doors
        string doorType = (Doors.MDFOptions is null) ? "Slab" : "Buyout";
        yield return new PPProduct(Room, GetProductName(), "Royal2", GetMaterialType(), doorType, "Standard", GetFinishMaterials(), GetEBMaterials(), GetParameters(), new());
    }

    private string GetProductName() {
        if (Doors.Quantity == 1) return "W1D";
        return "W2D";
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

        return parameters;
    }

    private string GetHingeSideOption() => Doors.HingeSide switch {
        HingeSide.NotApplicable => "0",
        HingeSide.Left => "1",
        HingeSide.Right => "0",
        _ => "0"
    };

}