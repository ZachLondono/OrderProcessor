using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.AllmoxyOrderExport.Attributes;

public static class DrawerBoxMaterial {

    public const string SOLID_BIRCH = "Pre-Finished Birch";
    public const string ECONOMY_BIRCH = "Economy Birch";

    public static string GetStandardHeight(Dimension boxHeight) {

        var availableHeights = MaxFaceHeightMap.Keys.Order().ToArray();

        if (availableHeights.Last() < boxHeight) {
            throw new ArgumentOutOfRangeException(nameof(boxHeight), "No valid allmoxy drawer box height for given drawer box height");
        }

        foreach (var height in availableHeights) {

            if (height < boxHeight) {
                continue;
            }

            return MaxFaceHeightMap[height];

        }

        return MaxFaceHeightMap[availableHeights.Last()];

    }

    public static Dimension GetBoxWidthFromOpening(Dimension openingWidth) {
        return openingWidth - Dimension.FromInches(3.0/8.0);
    }

    public static Dimension GetBoxDepthFromOpening(Dimension openingDepth) {

        var depths = BoxDepths.OrderDescending()
                              .ToArray();

        foreach (var depth in depths) {

            if (openingDepth < depth) {

                continue;

            }

            return depth;

        }

        return depths.Last();

    }

    public static Dictionary<Dimension, string> MaxFaceHeightMap => new() {
        { Dimension.FromInches(6.25), "2.5" },
        { Dimension.FromInches(7.5), "4.125" },
        { Dimension.FromInches(10), "6" },
        { Dimension.FromInches(13.75), "8.25" },
        { Dimension.FromInches(51), "12" },
    };

    public static Dimension[] BoxDepths => [
        Dimension.FromInches(12),
        Dimension.FromInches(14),
        Dimension.FromInches(16),
        Dimension.FromInches(18),
        Dimension.FromInches(21)
    ];

}
