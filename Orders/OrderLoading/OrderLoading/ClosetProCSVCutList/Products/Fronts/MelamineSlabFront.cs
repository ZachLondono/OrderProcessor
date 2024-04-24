using Domain.Orders.Enums;
using Domain.Orders.Entities.Products.Closets;
using Domain.Orders.ValueObjects;
using Domain.Orders.Entities.Products;
using Domain.ValueObjects;

namespace OrderLoading.ClosetProCSVCutList.Products.Fronts;

public class MelamineSlabFront : IClosetProProduct {

	public required int Qty { get; set; }
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

		ClosetMaterial material = new(Color, ClosetMaterialCore.ParticleBoard);
		ClosetPaint? paint = null;
		string comment = "";

		string sku;
		Dimension width;
		Dimension length;
		var parameters = new Dictionary<string, string>();

		switch (Type) {

			case DoorType.Door:

				sku = "DOOR";
				width = Width;
				length = Height;

				parameters.Add("OpeningWidth", (width - Dimension.FromMillimeters(15)).ToString());

				break;

			case DoorType.DrawerFront:

				sku = "DF-XX";
				width = Height;
				length = Width;

				if (HardwareSpread is Dimension hardwareSpread) {
					parameters.Add("PullCenters", hardwareSpread.AsMillimeters().ToString());
				}

				break;

			default:
				throw new InvalidOperationException("Unexpected melamine slab front tyep");

		}

		return new ClosetPart(Guid.NewGuid(), Qty, UnitPrice, PartNumber, Room, sku, width, length, material, paint, EdgeBandingColor, comment, parameters);

	}

}
