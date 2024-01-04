using ApplicationCore.Features.ClosetProCSVCutList.Products;
using System.Diagnostics.CodeAnalysis;

namespace ApplicationCore.Features.ClosetProCSVCutList.EqualityComparers;

public class MiscellaneousClosetPartComparer : IEqualityComparer<MiscellaneousClosetPart> {
    public bool Equals(MiscellaneousClosetPart? x, MiscellaneousClosetPart? y) {

        if (x is null && y is null) return true;
        if (x is not null && y is null) return false;
        if (y is not null && x is null) return false;

        if (x!.Room != y!.Room
            || x.UnitPrice != y.UnitPrice
            || x.Width != y.Width
            || x.Length != y.Length
            || x.Color != y.Color
            || x.EdgeBandingColor != y.EdgeBandingColor) {
            return false;
        }

        return true;

    }

    public int GetHashCode([DisallowNull] MiscellaneousClosetPart obj) {

        return HashCode.Combine(obj.Room,
                                obj.UnitPrice,
                                obj.Width,
                                obj.Length,
                                obj.Color,
                                obj.EdgeBandingColor);

    }
}
