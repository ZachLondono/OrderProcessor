using ApplicationCore.Features.ClosetProCSVCutList.CSVModels;
using ApplicationCore.Features.ClosetProCSVCutList.Products;
using Domain.ValueObjects;

namespace ApplicationCore.Features.ClosetProCSVCutList;

public partial class ClosetProPartMapper {

    public static DrawerBox CreateDovetailDrawerBox(Part part, RoomNamingStrategy strategy) => CreateDrawerBox(part, DrawerBoxType.Dovetail, strategy);

    public static DrawerBox CreateDowelDrawerBox(Part part, RoomNamingStrategy strategy) => CreateDrawerBox(part, DrawerBoxType.Dowel, strategy);

    public static DrawerBox CreateDrawerBox(Part part, DrawerBoxType type, RoomNamingStrategy strategy) {

        if (!TryParseMoneyString(part.PartCost, out decimal unitPrice)) {
            unitPrice = 0M;
        }

        string room = GetRoomName(part, strategy);
        var height = Dimension.FromInches(part.Height);
        var width = Dimension.FromInches(part.Width);
        var depth = Dimension.FromInches(part.Depth);

        bool isUndermount = part.PartName.Contains("Undermount", StringComparison.InvariantCultureIgnoreCase)
                            || part.ExportName.Contains("Undermount", StringComparison.InvariantCultureIgnoreCase);

        bool scoopFront = part.ExportName.Contains("Scoop Front Box");

        return new() {
            Qty = part.Quantity,
            PartNumber = part.PartNum,
            UnitPrice = unitPrice,
            Room = room,
            Width = width,
            Height = height,
            Depth = depth,
            UnderMountNotches = isUndermount,
            ScoopFront = scoopFront,
            Type = type
        };

    }

    public static ZargenDrawerBox CreateZargenDrawerBox() => throw new NotImplementedException();

}
