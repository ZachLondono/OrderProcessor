using ApplicationCore.Features.ClosetProCSVCutList.CSVModels;
using ApplicationCore.Features.ClosetProCSVCutList.Products;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.ClosetProCSVCutList;

public partial class ClosetProPartMapper {

    public static MiscellaneousClosetPart CreateToeKick(Part part) {

        Dimension width = Dimension.FromInches(part.Height);
        Dimension length = Dimension.FromInches(part.Width);

        return CreateMiscPart(part, width, length, MiscellaneousType.ToeKick);

    }

    public static MiscellaneousClosetPart CreateFiller(Part part) {

        Dimension width = Dimension.FromInches(4);
        Dimension length = Dimension.FromInches(part.Height);

        return CreateMiscPart(part, width, length, MiscellaneousType.Filler);

    }

    public static MiscellaneousClosetPart CreateBacking(Part part) {

        Dimension width = Dimension.FromInches(part.Width);
        Dimension length = Dimension.FromInches(part.Height);

        return CreateMiscPart(part, width, length, MiscellaneousType.Backing);

    }

    public static MiscellaneousClosetPart CreateCleat(Part part) {

        Dimension width = Dimension.FromInches(part.Height);
        Dimension length = Dimension.FromInches(part.Width);

        return CreateMiscPart(part, width, length, MiscellaneousType.Cleat);

    }

    public static MiscellaneousClosetPart CreateExtraPanel(Part part) {

        Dimension width = Dimension.FromInches(part.Width);
        Dimension length = Dimension.FromInches(part.Height);

        return CreateMiscPart(part, width, length, MiscellaneousType.ExtraPanel);

    }

    public static MiscellaneousClosetPart CreateTop(Part part) {
    
        // TODO: need to choose width / depth correctly so graining is going in the right direction
        Dimension width = Dimension.FromInches(part.Width);
        Dimension length = Dimension.FromInches(part.Depth);

        return CreateMiscPart(part, width, length, MiscellaneousType.Top);

    }

    public static MiscellaneousClosetPart CreateMiscPart(Part part, Dimension width, Dimension length, MiscellaneousType type) {

        if (!TryParseMoneyString(part.PartCost, out decimal unitPrice)) {
            unitPrice = 0M;
        }
        string room = GetRoomName(part);
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
