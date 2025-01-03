using Domain.Orders.Entities.Products.Closets;
using Domain.Orders.ValueObjects;
using Domain.ValueObjects;
using Domain.Orders.Enums;
using Domain.Orders.Entities.Products;

namespace OrderLoading.ClosetProCSVCutList.Products.Verticals;

public class VerticalPanel : IClosetProProduct {

	public required int Qty { get; set; }
	public required string Color { get; init; }
	public required string EdgeBandingColor { get; init; }
	public required string Room { get; init; }
	public required decimal UnitPrice { get; init; }
	public required int PartNumber { get; init; }

	public required Dimension Height { get; init; }
	public required Dimension Depth { get; init; }
	public required VerticalPanelDrilling Drilling { get; init; }
	public required bool WallHung { get; init; }
	public required bool ExtendBack { get; init; }
	public required bool HasBottomRadius { get; init; }
	public required BaseNotch BaseNotch { get; init; }
	public required VerticalPanelLEDChannel LEDChannel { get; init; }

    public IProduct ToProduct(Dimension verticalPanelBottomRadius) {

		if (LEDChannel != VerticalPanelLEDChannel.None) {
            throw new NotSupportedException("LED Channels are not supported.");
        }

        string sku = Drilling == VerticalPanelDrilling.DrilledThrough ? "PC" : "PE";

		ClosetPaint? paint = null;
		string comment = string.Empty;

		ClosetMaterial material = new(Color, ClosetMaterialCore.ParticleBoard);

		Dictionary<string, string> parameters = new() {
			{ "FINLEFT", Drilling == VerticalPanelDrilling.FinishedLeft ? "1" : "0" },
			{ "FINRIGHT", Drilling == VerticalPanelDrilling.FinishedRight ? "1" : "0" },
			{ "ExtendBack", ExtendBack ? "19.05" : "0" },
			{ "BottomNotchD", BaseNotch.Depth.AsMillimeters().ToString() },
			{ "BottomNotchH", BaseNotch.Height.AsMillimeters().ToString() },
			{ "WallMount", WallHung ? "1" : "0" },                          // TODO: add option to settings to include wall mounting bracket on wall hung panels
            { "BottomRadius", HasBottomRadius ? verticalPanelBottomRadius.AsMillimeters().ToString() : "0" },
		};

		if (!TryGetNearest32MMComplientHeight(Height, out Dimension finalHeight)) {
			finalHeight = Height;
		}

		return new ClosetPart(Guid.NewGuid(),
							  Qty,
							  UnitPrice,
							  PartNumber,
							  Room,
							  sku,
							  Depth,
							  finalHeight,
							  material,
							  paint,
							  EdgeBandingColor,
							  comment,
							  true,
							  parameters);

	}

	public static bool TryGetNearest32MMComplientHeight(Dimension input, out Dimension output, double maxErrorMM = 2) {

		var multiple = (input.AsMillimeters() - 19d) / 32d;

		var rounded = Math.Round(multiple);

		output = Dimension.FromMillimeters(rounded * 32 + 19);

		var error = Math.Abs((input.AsMillimeters() - output.AsMillimeters()));

		if (error > maxErrorMM) {

			return false;

		}

		return true;

	}

}
