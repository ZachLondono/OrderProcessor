using Domain.Companies.ValueObjects;
using Domain.Orders.Enums;
using Domain.Orders.Entities.Products.Closets;
using Domain.Orders.ValueObjects;
using Domain.Orders.Entities.Products;
using Domain.ValueObjects;

namespace OrderLoading.ClosetProCSVCutList.Products;

public class MiscellaneousClosetPart : IClosetProProduct {

	public required int Qty { get; set; }
	public required string Color { get; init; }
	public required string EdgeBandingColor { get; init; }
	public required string Room { get; init; }
	public required decimal UnitPrice { get; init; }
	public required int PartNumber { get; init; }

	public required Dimension Width { get; init; }
	public required Dimension Length { get; init; }
	public required MiscellaneousType Type { get; init; }

	public IProduct ToProduct(ClosetProSettings settings) {

		string sku = Type switch {
			MiscellaneousType.ToeKick => settings.ToeKickSKU,
			MiscellaneousType.Filler or MiscellaneousType.Cleat => "NL1",
			MiscellaneousType.Backing => "BK34",
			MiscellaneousType.ExtraPanel => "PANEL",
			MiscellaneousType.Top => "TOP",
			_ => throw new InvalidOperationException("Unexpected miscellaneous closet part type")
		};

		Dimension finalWidth = Width;
        if (Type == MiscellaneousType.ToeKick && !TryGetNearest32MMComplientToeKickHeight(Width, out finalWidth)) {
            finalWidth = Width;
        }

        var material = new ClosetMaterial(Color, ClosetMaterialCore.ParticleBoard);

		return new ClosetPart(Guid.NewGuid(),
							  Qty,
							  UnitPrice,
							  PartNumber,
							  Room,
							  sku,
							  finalWidth,
							  Length,
							  material,
							  null,
							  EdgeBandingColor,
							  "",
							  true,
							  new Dictionary<string, string>());

	}

    public static bool TryGetNearest32MMComplientToeKickHeight(Dimension input, out Dimension output, double maxErrorMM = 2) {

        var multiple = input.AsMillimeters() / 32d;

        var rounded = Math.Round(multiple);

        output = Dimension.FromMillimeters(rounded * 32);

        var error = Math.Abs((input.AsMillimeters() - output.AsMillimeters()));

        if (error > maxErrorMM) {

            return false;

        }

        return true;

    }

}