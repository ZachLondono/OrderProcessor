using OrderLoading.ClosetProCSVCutList.CSVModels;
using OrderLoading.ClosetProCSVCutList.Products;
using Domain.ValueObjects;

namespace OrderLoading.ClosetProCSVCutList;

public partial class ClosetProPartMapper {

	public static MiscellaneousClosetPart CreateToeKick(Part part, RoomNamingStrategy strategy) {

		Dimension width = Dimension.FromInches(part.Height);
		Dimension length = Dimension.FromInches(part.Width);

		return CreateMiscPart(part, width, length, MiscellaneousType.ToeKick, strategy);

	}

	public static MiscellaneousClosetPart CreateFiller(Part part, RoomNamingStrategy strategy) {

		Dimension width = Dimension.FromInches(4); // Fillers are oversized so they can be scribed to the wall
		Dimension length = Dimension.FromInches(part.Height);

		return CreateMiscPart(part, width, length, MiscellaneousType.Filler, strategy);

	}

	/*
	 * Used for mapping parts that are added using the "Material" section on the ClosetPro editor
	 * The value of the 'CornerShelfSizes' property can be used to distinguish between a Cleat (c), Shelf (s) or Vertical (v)
	 */
	public static MiscellaneousClosetPart CreateMelaminePart(Part part, RoomNamingStrategy strategy) {

		Dimension width = Dimension.FromInches(part.Width);
        Dimension length = Dimension.FromInches(part.Height);

		return CreateMiscPart(part, width, length, MiscellaneousType.ExtraPanel, strategy);

	}

	public static MiscellaneousClosetPart CreateFlatMolding(Part part, RoomNamingStrategy strategy) {

		Dimension width = part.PartName switch {
			"4\" Flat Crown" => Dimension.FromInches(4),
			_ => throw new InvalidOperationException($"Unknown molding type {part.PartName}")
		};

		Dimension length = Dimension.FromInches(96);

		return CreateMiscPart(part, width, length, MiscellaneousType.ExtraPanel, strategy);

	}

	public static MiscellaneousClosetPart CreateBacking(Part part, RoomNamingStrategy strategy) {

		Dimension width = Dimension.FromInches(part.Width);
		Dimension length = Dimension.FromInches(part.Height);

		return CreateMiscPart(part, width, length, MiscellaneousType.Backing, strategy);

	}

	public static MiscellaneousClosetPart CreateCleat(Part part, RoomNamingStrategy strategy) {

		Dimension width = Dimension.FromInches(part.Height);
		Dimension length = Dimension.FromInches(part.Width);

		return CreateMiscPart(part, width, length, MiscellaneousType.Cleat, strategy);

	}

	public static MiscellaneousClosetPart CreateExtraPanel(Part part, RoomNamingStrategy strategy) {

		Dimension width = Dimension.FromInches(part.Width);
		Dimension length = Dimension.FromInches(part.Height);

		return CreateMiscPart(part, width, length, MiscellaneousType.ExtraPanel, strategy);

	}

	public static MiscellaneousClosetPart CreateTop(Part part, RoomNamingStrategy strategy) {

        // TODO: need to choose width / depth correctly so graining is going in the right direction
        Dimension width = Dimension.FromInches(part.Width);
		Dimension length = Dimension.FromInches(part.Depth);

		return CreateMiscPart(part, width, length, MiscellaneousType.Top, strategy);

	}

	public static MiscellaneousClosetPart CreateMiscPart(Part part, Dimension width, Dimension length, MiscellaneousType type, RoomNamingStrategy strategy) {

		if (!TryParseMoneyString(part.PartCost, out decimal unitPrice)) {
			unitPrice = 0M;
		}
		string room = GetRoomName(part, strategy);
		string edgeBandingColor = part.InfoRecords
										.Where(i => i.PartName == "Edge Banding")
										.Select(i => i.Color)
										.FirstOrDefault() ?? part.Color;

		return new MiscellaneousClosetPart() {
			Qty = part.Quantity,
			Color = part.Color,
			EdgeBandingColor = part.Color,
			Room = room,
			UnitPrice = unitPrice,
			PartNumber = part.PartNum,
			Width = width,
			Length = length,
			Type = type
		};

	}

}
