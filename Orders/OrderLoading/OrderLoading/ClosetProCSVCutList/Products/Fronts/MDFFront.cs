using Domain.Orders.Enums;
using Domain.Orders.Entities.Products.Doors;
using Domain.Orders.ValueObjects;
using Domain.Orders.Entities.Products;
using Domain.ValueObjects;

namespace OrderLoading.ClosetProCSVCutList.Products.Fronts;

public class MDFFront : IClosetProProduct {

	public required int Qty { get; set; }
	public required string Room { get; init; }
	public required decimal UnitPrice { get; init; }
	public required int PartNumber { get; init; }

	public required Dimension Height { get; init; }
	public required Dimension Width { get; init; }
	public required DoorFrame Frame { get; init; }
	public required string Style { get; init; }
	public required DoorType Type { get; init; }
	public required Dimension? HardwareSpread { get; init; }
	public required string PaintColor { get; set; }

	public IProduct ToProduct() {

		return new MDFDoorProduct(Guid.NewGuid(),
									UnitPrice,
									Room,
									Qty,
									PartNumber,
									Type,
									Height,
									Width,
									string.Empty,
									Frame,
									$"MDF-3/4\"",
									Dimension.FromInches(0.75),
									Style,
									"Square",
									"Flat",
									Dimension.FromInches(0.25),
									DoorOrientation.Vertical,
									[],
									PaintColor);

	}

}
