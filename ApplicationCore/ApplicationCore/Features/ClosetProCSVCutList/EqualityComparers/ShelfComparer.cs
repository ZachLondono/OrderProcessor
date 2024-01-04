using ApplicationCore.Features.ClosetProCSVCutList.Products;
using System.Diagnostics.CodeAnalysis;

namespace ApplicationCore.Features.ClosetProCSVCutList.EqualityComparers;

public class ShelfComparer : IEqualityComparer<Shelf> {

    public bool Equals(Shelf? x, Shelf? y) {

        if (x is null && y is null) return true;
        if (x is not null && y is null) return false;
        if (y is not null && x is null) return false;

        if (x!.Room != y!.Room
            || x.UnitPrice != y.UnitPrice
            || x.Width != y.Width
            || x.Depth != y.Depth
            || x.Color != y.Color
            || x.EdgeBandingColor != y.EdgeBandingColor
            || x.ExtendBack != y.ExtendBack
            || x.Type != y.Type) {
            return false;
        }

        return true;

    }

    public int GetHashCode([DisallowNull] Shelf obj) {

        var a = HashCode.Combine(obj.Room,
                                obj.UnitPrice,
                                obj.Width,
                                obj.Depth,
                                obj.Type,
                                obj.Color,
                                obj.EdgeBandingColor);

        var b = HashCode.Combine(obj.ExtendBack);

        return HashCode.Combine(a, b);

    }

}
