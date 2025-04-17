using OrderLoading.ClosetProCSVCutList.CSVModels;
using OrderLoading.ClosetProCSVCutList.Products;
using Domain.Orders.Enums;
using Domain.Orders.ValueObjects;
using Domain.ValueObjects;
using OrderLoading.ClosetProCSVCutList.Products.Fronts;

namespace OrderLoading.ClosetProCSVCutList;

public partial class ClosetProPartMapper {

	public static IClosetProProduct CreateFrontFromParts(Part rail, Part insert, Dimension hardwareSpread, RoomNamingStrategy strategy) {

		if (rail.ExportName.Contains("MDF")) {

			return CreateMDFFront(rail, insert, hardwareSpread, strategy);

		} else {

			return CreateFivePieceFront(rail, insert, hardwareSpread, strategy);

		}

	}

	public static FivePieceFront CreateFivePieceFront(Part rail, Part insert, Dimension hardwareSpread, RoomNamingStrategy strategy) {

		if (rail.Quantity != insert.Quantity) {
			throw new InvalidOperationException("Unexpected mismatch in door rail and insert quantity.");
		}

		if (!TryParseMoneyString(rail.PartCost, out decimal unitPriceRail)) {
			unitPriceRail = 0M;
		}
		if (!TryParseMoneyString(insert.PartCost, out decimal unitPriceInsert)) {
			unitPriceInsert = 0M;
		}

		string room = GetRoomName(insert, strategy);

		Dimension height = Dimension.FromInches(rail.Height);
		Dimension width = Dimension.FromInches(rail.Width);
		var frame = GetDoorFrame(rail, insert);
		DoorType doorType = rail.PartName.Contains("Drawer") ? DoorType.DrawerFront : DoorType.Door;

		return new() {
			Qty = rail.Quantity,
			Room = room,
			UnitPrice = unitPriceRail + unitPriceInsert,
			Color = rail.Color,
			PartNumber = rail.PartNum,
			Height = height,
			Width = width,
			HardwareSpread = hardwareSpread,
			Type = doorType,
			Frame = frame,
		};

	}

	public static MDFFront CreateMDFFront(Part rail, Part insert, Dimension hardwareSpread, RoomNamingStrategy strategy) {

		if (rail.Quantity != insert.Quantity) {
			throw new InvalidOperationException("Unexpected mismatch in door rail and insert quantity.");
		}

		if (!TryParseMoneyString(rail.PartCost, out decimal unitPriceRail)) {
			unitPriceRail = 0M;
		}
		if (!TryParseMoneyString(insert.PartCost, out decimal unitPriceInsert)) {
			unitPriceInsert = 0M;
		}

		string room = GetRoomName(insert, strategy);

		Dimension height = Dimension.FromInches(rail.Height);
		Dimension width = Dimension.FromInches(rail.Width);
		var frame = GetDoorFrame(rail, insert);
		DoorType doorType = rail.PartName.Contains("Drawer") ? DoorType.DrawerFront : DoorType.Door;

		string style = "UNKNOWN";
		if (rail.PartName.Contains("shaker", StringComparison.InvariantCultureIgnoreCase)) {
			style = "Shaker";
		}

		return new() {
			Qty = rail.Quantity,
			Room = room,
			UnitPrice = unitPriceRail + unitPriceInsert,
			PartNumber = rail.PartNum,
			Height = height,
			Width = width,
			HardwareSpread = hardwareSpread,
			Type = doorType,
			Frame = frame,
			Style = style
		};

	}

	public static MelamineSlabFront CreateSlabFront(Part part, Dimension hardwareSpread, RoomNamingStrategy strategy) {

		if (!TryParseMoneyString(part.PartCost, out decimal unitPrice)) {
			unitPrice = 0M;
		}
		string room = GetRoomName(part, strategy);

		Dimension height = Dimension.FromInches(part.Height);
		Dimension width = Dimension.FromInches(part.Width);

		DoorType doorType = DoorType.Door;
		if (part.PartName.Contains("Drawer")) {
			doorType = DoorType.DrawerFront;
		} else if (part.PartName.Contains("Tilt Down")) {
			doorType = DoorType.HamperDoor;
		}

		return new() {
			Qty = part.Quantity,
			Color = part.Color,
			EdgeBandingColor = part.Color,
			Room = room,
			UnitPrice = unitPrice,
			PartNumber = part.PartNum,
			Height = height,
			Width = width,
			HardwareSpread = hardwareSpread,
			Type = doorType
		};

	}

	public static DoorFrame GetDoorFrame(Part rail, Part insert) {

		Dimension insertOverlap = Dimension.FromInches(0.5); // The depth that ClosetPro assumes the center panel goes into the frame (both sides). E.g. a 10" wide door with 2.5" stiles will have a 6" wide center panel

		Dimension height = Dimension.FromInches(rail.Height);
		Dimension width = Dimension.FromInches(rail.Width);

		Dimension insertHeight = Dimension.FromInches(insert.Height);
		Dimension insertWidth = Dimension.FromInches(insert.Width);

		Dimension railWidth = (height - insertHeight + insertOverlap*2) / 2;
		Dimension stileWidth = (width - insertWidth + insertOverlap*2) / 2;

		return new(railWidth, stileWidth);

	}

}
