using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.Products.Closets;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.ClosetProCSVCutList.Products;

public class MelamineSlabFront {

    public required int Qty { get; init; }
    public required string Color { get; init; }
    public required string EdgeBandingColor { get; init; }
    public required string Room { get; init; }
    public required decimal UnitPrice { get; init; }
    public required int PartNumber { get; init; }

    public required Dimension Height { get; init; }
    public required Dimension Width { get; init; }
    public required DoorType Type { get; init; }
    public required Dimension? HardwareSpread { get; init; }

    public IProduct ToProduct() {

        string sku = Type switch {
            DoorType.Door => "DOOR",
            DoorType.DrawerFront => "DF-XX",
            _ => throw new InvalidOperationException("Unexpected melamine slab front type")
        };
        ClosetMaterial material = new(Color, ClosetMaterialCore.Plywood);
        ClosetPaint? paint = null;
        string comment = "";
        var parameters = new Dictionary<string, string>();

        if (Type == DoorType.DrawerFront && HardwareSpread is Dimension hardwareSpread) {
            parameters.Add("PullCenters", hardwareSpread.AsMillimeters().ToString());
        }

        return new ClosetPart(Guid.NewGuid(), Qty, UnitPrice, PartNumber, Room, sku, Width, Height, material, paint, EdgeBandingColor, comment, parameters);

    }

}
