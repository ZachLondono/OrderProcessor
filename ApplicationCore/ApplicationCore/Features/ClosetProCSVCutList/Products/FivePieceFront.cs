using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.Products.Doors;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.ClosetProCSVCutList.Products;

public class FivePieceFront {

    public required int Qty { get; init; }
    public required string Room { get; init; }
    public required decimal UnitPrice { get; init; }
    public required string Color { get; init; }
    public required int PartNumber { get; init; }

    public required Dimension Height { get; init; }
    public required Dimension Width { get; init; }
    public required DoorFrame Frame { get; init; }
    public required DoorType Type { get; init; }
    public required Dimension? HardwareSpread { get; init; }

    public IProduct ToProduct() {

        return new FivePieceDoorProduct(Guid.NewGuid(),
                                       Qty,
                                       UnitPrice,
                                       PartNumber,
                                       Room,
                                       Width,
                                       Height,
                                       Frame,
                                       Dimension.FromInches(0.75),
                                       Dimension.FromInches(0.25),
                                       Color);

    }

}
