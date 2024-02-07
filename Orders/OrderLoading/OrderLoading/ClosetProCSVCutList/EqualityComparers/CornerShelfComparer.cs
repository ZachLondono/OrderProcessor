using OrderLoading.ClosetProCSVCutList.Products;
using System.Diagnostics.CodeAnalysis;

namespace OrderLoading.ClosetProCSVCutList.EqualityComparers;

public class CornerShelfComparer : IEqualityComparer<CornerShelf> {

    public bool Equals(CornerShelf? x, CornerShelf? y) {

        if (x is null && y is null) return true;
        if (x is not null && y is null) return false;
        if (y is not null && x is null) return false;

        if (x!.Room != y!.Room
            || x.UnitPrice != y.UnitPrice
            || x.RightWidth != y.RightWidth
            || x.ProductWidth != y.ProductWidth
            || x.ProductLength != y.ProductLength
            || x.NotchSideLength != y.NotchSideLength
            || x.Color != y.Color
            || x.EdgeBandingColor != y.EdgeBandingColor
            || x.Type != y.Type) {
            return false;
        }

        return true;

    }

    public int GetHashCode([DisallowNull] CornerShelf obj) {
        var a = HashCode.Combine(obj.Room,
                                obj.UnitPrice,
                                obj.RightWidth,
                                obj.ProductWidth,
                                obj.ProductLength,
                                obj.NotchSideLength,
                                obj.Color);

        var b = HashCode.Combine(obj.EdgeBandingColor,
                                obj.Type);

        return HashCode.Combine(a, b);
    }

}
