using OrderLoading.ClosetProCSVCutList.CSVModels;
using OrderLoading.ClosetProCSVCutList.Products;
using Domain.ValueObjects;
using OrderLoading.ClosetProCSVCutList.Products.Shelves;

namespace OrderLoading.ClosetProCSVCutList;

public partial class ClosetProPartMapper {

	public static IClosetProProduct CreateFixedShelfFromPart(Part part, bool wallHasBacking, bool extendBack, RoomNamingStrategy strategy) {

		if (part.ExportName == "L Fixed Shelf" || part.PartName == "L Fixed Shelf") {

			if (wallHasBacking || extendBack) {
				throw new UnsupportedDesignException("Corner units with back panels are not supported");
			}

			return CreateLFixedShelf(part, strategy);

		} else if (part.ExportName == "Pie Fixed Shelf" || part.PartName == "Pie Fixed Shelf") {

			if (wallHasBacking || extendBack) {
				throw new UnsupportedDesignException("Corner units with back panels are not supported");
			}

			return CreateDiagonalFixedShelf(part, strategy);

		} else {

			return CreateFixedShelf(part, extendBack, wallHasBacking, strategy);

		}

	}

	public static IClosetProProduct CreateAdjustableShelfFromPart(Part part, bool wallHasBacking, bool extendBack, RoomNamingStrategy strategy) {

		if (part.ExportName == "L Adj Shelf" || part.PartName == "L Adj Shelf") {

			if (wallHasBacking || extendBack) {
				throw new UnsupportedDesignException("Corner units with back panels are not supported");
			}

			return CreateLAdjustableShelf(part, strategy);

		} else if (part.ExportName == "Pie Adj Shelf" || part.PartName == "Pie Adj Shelf") {

			if (wallHasBacking || extendBack) {
				throw new UnsupportedDesignException("Corner units with back panels are not supported");
			}

			return CreateDiagonalAdjustableShelf(part, strategy);

		} else {

			return CreateAdjustableShelf(part, extendBack, wallHasBacking, strategy);

		}

	}

	public static IClosetProProduct CreateRollOutShelfFromPart(Part part, RoomNamingStrategy strategy) {

		if (!TryParseMoneyString(part.PartCost, out decimal unitPrice)) {
			unitPrice = 0M;
		}
		string room = GetRoomName(part, strategy);
		Dimension depth = Dimension.FromInches(part.Depth);
		Dimension width = Dimension.FromInches(part.Width) - Dimension.FromMillimeters(27);
		string edgeBandingColor = part.InfoRecords
								.Where(i => i.PartName == "Edge Banding")
								.Select(i => i.Color)
								.FirstOrDefault() ?? part.Color;

		return new Shelf() {
			Qty = part.Quantity,
			UnitPrice = unitPrice,
			Color = part.Color,
			Room = room,
			PartNumber = part.PartNum,
			EdgeBandingColor = edgeBandingColor,

			Width = width,
			Depth = depth,
			Type = ShelfType.RollOut, 
			ExtendBack = false,
			LEDChannel = false
		};

	}

	public static Shelf CreateAdjustableShelf(Part part, bool extendBack, bool wallHasBacking, RoomNamingStrategy strategy) => CreateShelf(part, ShelfType.Adjustable, extendBack, wallHasBacking, strategy);

	public static Shelf CreateFixedShelf(Part part, bool extendBack, bool wallHasBacking, RoomNamingStrategy strategy) => CreateShelf(part, ShelfType.Fixed, extendBack, wallHasBacking, strategy);

	public static Shelf CreateShoeShelf(Part part, bool extendBack, bool wallHasBacking, RoomNamingStrategy strategy) => CreateShelf(part, ShelfType.Shoe, extendBack, wallHasBacking, strategy);

	public static Shelf CreateShelf(Part part, ShelfType type, bool extendBack, bool wallHasBacking, RoomNamingStrategy strategy) {

		if (!TryParseMoneyString(part.PartCost, out decimal unitPrice)) {
			unitPrice = 0M;
		}
		string room = GetRoomName(part, strategy);
		Dimension depth = Dimension.FromInches(part.Depth);
		Dimension width = Dimension.FromInches(part.Width);
		string edgeBandingColor = part.InfoRecords
								.Where(i => i.PartName == "Edge Banding")
								.Select(i => i.Color)
								.FirstOrDefault() ?? part.Color;

		bool ledChannel = part.PartName.Contains("with Strip");
		if (ledChannel && part.PartName.StartsWith("Bottom")) {
			throw new UnsupportedDesignException("Bottom fixed shelves with LED channels are not supported");
        }

		return new Shelf() {
			Qty = part.Quantity,
			UnitPrice = unitPrice,
			Color = part.Color,
			Room = room,
			PartNumber = part.PartNum,
			EdgeBandingColor = edgeBandingColor,

			Width = width,
			Depth = depth,
			Type = type,
			ExtendBack = extendBack,
			LEDChannel = ledChannel
		};

	}

	public static CornerShelf CreateLFixedShelf(Part part, RoomNamingStrategy strategy) => CreateCornerShelf(part, CornerShelfType.LFixed, strategy);

	public static CornerShelf CreateLAdjustableShelf(Part part, RoomNamingStrategy strategy) => CreateCornerShelf(part, CornerShelfType.LAdjustable, strategy);

	public static CornerShelf CreateDiagonalFixedShelf(Part part, RoomNamingStrategy strategy) => CreateCornerShelf(part, CornerShelfType.DiagonalFixed, strategy);

	public static CornerShelf CreateDiagonalAdjustableShelf(Part part, RoomNamingStrategy strategy) => CreateCornerShelf(part, CornerShelfType.DiagonalAdjustable, strategy);

	public static CornerShelf CreateCornerShelf(Part part, CornerShelfType type, RoomNamingStrategy strategy) {

		if (!TryParseMoneyString(part.PartCost, out decimal unitPrice)) {
			unitPrice = 0M;
		}
		string room = GetRoomName(part, strategy);
		Dimension depth = Dimension.FromInches(part.Depth);
		Dimension width = Dimension.FromInches(part.Width);
		string edgeBandingColor = part.InfoRecords
								.Where(i => i.PartName == "Edge Banding")
								.Select(i => i.Color)
								.FirstOrDefault() ?? part.Color;

		var dimensions = ParseCornerShelfDimensions(part.CornerShelfSizes);
		var left = dimensions[0];
		var right = dimensions[3];

		return new CornerShelf() {
			Qty = part.Quantity,
			UnitPrice = unitPrice,
			Color = part.Color,
			Room = room,
			PartNumber = part.PartNum,
			EdgeBandingColor = edgeBandingColor,

			ProductWidth = left,
			ProductLength = width,
			RightWidth = right,
			NotchSideLength = depth,
			Type = type,
		};

	}

}
