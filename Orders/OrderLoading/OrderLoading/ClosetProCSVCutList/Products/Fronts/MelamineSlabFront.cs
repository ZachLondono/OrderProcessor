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
				if (!TryGetNearest32MMComplientHalfOverlayHeight(Height, out length)) {
					length = Height;
				}

				parameters.Add("OpeningWidth", (width - Dimension.FromMillimeters(15)).AsMillimeters().ToString());

				break;

			case DoorType.DrawerFront:

				sku = "DF-XX";
				length = Width;
				if (!TryGetNearest32MMComplientHalfOverlayHeight(Height, out width)) {
					width = Height;
				}

				if (HardwareSpread is not null) {
					parameters.Add("PullCenters", ((Dimension) HardwareSpread).AsMillimeters().ToString());
				}

				break;

			case DoorType.HamperDoor:

				sku = "HAMPDOOR";
				width = Width;
				if (!TryGetNearest32MMComplientHalfOverlayHeight(Height, out length)) {
					length = Height;
				}

				if (HardwareSpread is not null) {
					parameters.Add("PullCenters", ((Dimension) HardwareSpread).AsMillimeters().ToString());
				}

				break;

			default:
				throw new InvalidOperationException("Unexpected melamine slab front tyep");

		}

		return new ClosetPart(Guid.NewGuid(), Qty, UnitPrice, PartNumber, Room, sku, width, length, material, paint, EdgeBandingColor, comment, false, parameters);

	}

	/// <summary>
	/// If the provided height is within a specified range from a valid 32mm half overlay compliant door height, a corrected height will be outputed and true will be returned. Otherwise false will be returned.
    /// Heights which are further than the nearest compliant height by more than the specified max error are considered to be intentionally non-compliant and therefore a compliant height is not given.
	/// </summary>
	/// <param name="input">The given height of the door</param>
	/// <param name="output">The nearest 32mm half overlay compliant door height</param>
	/// <param name="maxErrorMM">The maximum amount of deviation from a compliant height which is still considered not intentionally specifying a non-compliant height</param>
	/// <returns></returns>
	public static bool TryGetNearest32MMComplientHalfOverlayHeight(Dimension input, out Dimension output, double maxErrorMM = 3.1) {

		var multiple = (input.AsMillimeters() - 29d) / 32d;

		var rounded = Math.Round(multiple);

		output = Dimension.FromMillimeters(rounded * 32 + 29);

		var error = Math.Abs((input.AsMillimeters() - output.AsMillimeters()));

		if (error > maxErrorMM) {

			return false;

		}

		return true;

	}

}
