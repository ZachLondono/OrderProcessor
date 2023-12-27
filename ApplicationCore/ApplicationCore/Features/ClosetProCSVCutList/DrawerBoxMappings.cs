using ApplicationCore.Features.ClosetProCSVCutList.CSVModels;
using ApplicationCore.Features.ClosetProCSVCutList.Products;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.ClosetProCSVCutList;

public partial class ClosetProPartMapper {

    public static DrawerBox CreateDovetailDrawerBox(Part part) => CreateDrawerBox(part, DrawerBoxType.Dovetail);

    public static DrawerBox CreateDowelDrawerBox(Part part) => CreateDrawerBox(part, DrawerBoxType.Dowel);

    public static DrawerBox CreateDrawerBox(Part part, DrawerBoxType type) {

        if (!TryParseMoneyString(part.PartCost, out decimal unitPrice)) {
            unitPrice = 0M;
        }

        string room = GetRoomName(part);
        var height = Dimension.FromInches(part.Height);
        var width = Dimension.FromInches(part.Width);
        var depth = Dimension.FromInches(part.Depth);

        bool isUndermount = part.PartName.Contains("Undermount");

        bool scoopFront = part.ExportName == "Scoop Front Box";

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
