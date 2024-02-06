using ApplicationCore.Features.ClosetProCSVCutList.CSVModels;
using ApplicationCore.Features.ClosetProCSVCutList.Products;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using Domain.ValueObjects;

namespace ApplicationCore.Features.ClosetProCSVCutList;

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
        DoorType doorType = rail.PartName.Contains("Drawer") ? DoorType.DrawerFront : DoorType.Door;

        DoorFrame frame = new(Dimension.FromInches((rail.Height - insert.Height) / 2), Dimension.FromInches((rail.Width - insert.Width) / 2));

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
        DoorType doorType = rail.PartName.Contains("Drawer") ? DoorType.DrawerFront : DoorType.Door;

        DoorFrame frame = new(Dimension.FromInches((rail.Height - insert.Height) / 2), Dimension.FromInches((rail.Width - insert.Width) / 2));

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
            Style = style,
            PaintColor = rail.Color
        };

    }

    public static MelamineSlabFront CreateSlabFront(Part part, Dimension hardwareSpread, RoomNamingStrategy strategy) {

        if (!TryParseMoneyString(part.PartCost, out decimal unitPrice)) {
            unitPrice = 0M;
        }
        string room = GetRoomName(part, strategy);

        Dimension height = Dimension.FromInches(part.Height);
        Dimension width = Dimension.FromInches(part.Width);
        DoorType doorType = part.PartName.Contains("Drawer") ? DoorType.DrawerFront : DoorType.Door;

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

}
