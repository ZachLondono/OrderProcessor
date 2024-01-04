using ApplicationCore.Features.ClosetProCSVCutList.Products;
using System.Diagnostics.CodeAnalysis;

namespace ApplicationCore.Features.ClosetProCSVCutList.EqualityComparers;

public class DividerShelfComparer : IEqualityComparer<DividerShelf> {

    public bool Equals(DividerShelf? x, DividerShelf? y) {

        if (x is null && y is null) return true;
        if (x is not null && y is null) return false;
        if (y is not null && x is null) return false;

        if (x!.Room != y!.Room
            || x.UnitPrice != y.UnitPrice
            || x.Width != y.Width
            || x.Depth != y.Depth
            || x.Color != y.Color
            || x.EdgeBandingColor != y.EdgeBandingColor
            || x.Type != y.Type) {
            return false;
        }

        return true;

    }

    public int GetHashCode([DisallowNull] DividerShelf obj) {
        return HashCode.Combine(obj.Room,
                                obj.UnitPrice,
                                obj.Width,
                                obj.Depth,
                                obj.DividerCount,
                                obj.Color,
                                obj.EdgeBandingColor,
                                obj.Type);
    }

}
