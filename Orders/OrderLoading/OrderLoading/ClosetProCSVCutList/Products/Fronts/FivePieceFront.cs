using Domain.Orders.Enums;
using Domain.Orders.Entities.Products.Doors;
using Domain.Orders.ValueObjects;
using Domain.Orders.Entities.Products;
using Domain.ValueObjects;

namespace OrderLoading.ClosetProCSVCutList.Products.Fronts;

public class FivePieceFront : IClosetProProduct {

	public required int Qty { get; set; }
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
									   Color,
									   Type);

	}

}
