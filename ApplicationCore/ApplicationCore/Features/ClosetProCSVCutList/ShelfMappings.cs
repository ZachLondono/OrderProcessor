using ApplicationCore.Features.ClosetProCSVCutList.CSVModels;
using ApplicationCore.Features.ClosetProCSVCutList.Products;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.ClosetProCSVCutList;

public partial class ClosetProPartMapper {

    public static IClosetProProduct CreateFixedShelfFromPart(Part part, bool wallHasBacking, bool extendBack) {

        if (part.ExportName == "L Fixed Shelf") {

            return CreateLFixedShelf(part);

        } else if (part.ExportName == "Pie Fixed Shelf") {

            return CreateDiagonalFixedShelf(part);

        } else {

            return CreateFixedShelf(part, extendBack, wallHasBacking);

        }

    }

    public static IClosetProProduct CreateAdjustableShelfFromPart(Part part, bool wallHasBacking, bool extendBack) {

        if (part.ExportName == "L Adj Shelf") {

            return CreateLAdjustableShelf(part);

        } else if (part.ExportName == "Pie Adj Shelf") {

            return CreateDiagonalAdjustableShelf(part);

        } else {

            return CreateAdjustableShelf(part, extendBack, wallHasBacking);

        }

    }

    public static Shelf CreateAdjustableShelf(Part part, bool extendBack, bool wallHasBacking) => CreateShelf(part, ShelfType.Adjustable, extendBack, wallHasBacking);

    public static Shelf CreateFixedShelf(Part part, bool extendBack, bool wallHasBacking) => CreateShelf(part, ShelfType.Fixed, extendBack, wallHasBacking);

    public static Shelf CreateShoeShelf(Part part, bool extendBack, bool wallHasBacking) => CreateShelf(part, ShelfType.Shoe, extendBack, wallHasBacking);

    public static Shelf CreateShelf(Part part, ShelfType type, bool extendBack, bool wallHasBacking) {

        if (!TryParseMoneyString(part.PartCost, out decimal unitPrice)) {
            unitPrice = 0M;
        }
        string room = GetRoomName(part);
        Dimension depth = Dimension.FromInches(part.Depth);
        Dimension width = Dimension.FromInches(part.Width);
        string edgeBandingColor = part.InfoRecords
                                .Where(i => i.PartName == "Edge Banding")
                                .Select(i => i.Color)
                                .FirstOrDefault() ?? part.Color;

        if (wallHasBacking && (part.PartName == "Top Fixed Shelf" || part.PartName == "Bottom Fixed Shelf")) {
            extendBack = true;
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
            ExtendBack = extendBack
        };

    }

    public static CornerShelf CreateLFixedShelf(Part part) => CreateCornerShelf(part, CornerShelfType.LFixed);

    public static CornerShelf CreateLAdjustableShelf(Part part) => CreateCornerShelf(part, CornerShelfType.LAdjustable);

    public static CornerShelf CreateDiagonalFixedShelf(Part part) => CreateCornerShelf(part, CornerShelfType.DiagonalFixed);

    public static CornerShelf CreateDiagonalAdjustableShelf(Part part) => CreateCornerShelf(part, CornerShelfType.DiagonalAdjustable);

    public static CornerShelf CreateCornerShelf(Part part, CornerShelfType type) {

        if (!TryParseMoneyString(part.PartCost, out decimal unitPrice)) {
            unitPrice = 0M;
        }
        string room = GetRoomName(part);
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

            ProductWidth = width,
            ProductLength = depth,
            RightWidth = left,
            NotchSideLength = right,
            Type = type,
        };

    }

}
