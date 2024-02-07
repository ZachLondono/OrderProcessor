using OrderLoading.ClosetProCSVCutList.Products;
using System.Diagnostics.CodeAnalysis;

namespace OrderLoading.ClosetProCSVCutList.EqualityComparers;

public class MelamineSlabFrontComparer : IEqualityComparer<MelamineSlabFront> {

    public bool Equals(MelamineSlabFront? x, MelamineSlabFront? y) {

        if (x is null && y is null) return true;
        if (x is not null && y is null) return false;
        if (y is not null && x is null) return false;

        if (x!.Room != y!.Room
            || x.UnitPrice != y.UnitPrice
            || x.Height != y.Height
            || x.Width != y.Width
            || x.Type != y.Type
            || x.Color != y.Color
            || x.EdgeBandingColor != y.EdgeBandingColor
            || x.HardwareSpread != y.HardwareSpread) {
            return false;
        }

        return true;

    }

    public int GetHashCode([DisallowNull] MelamineSlabFront obj) {

        var a = HashCode.Combine(obj.Room,
                                obj.UnitPrice,
                                obj.Height,
                                obj.Width,
                                obj.Type,
                                obj.Color,
                                obj.EdgeBandingColor);

        var b = HashCode.Combine(obj.HardwareSpread);

        return HashCode.Combine(a, b);

    }

}
