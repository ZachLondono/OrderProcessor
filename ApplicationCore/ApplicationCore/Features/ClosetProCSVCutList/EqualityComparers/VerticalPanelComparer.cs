using ApplicationCore.Features.ClosetProCSVCutList.Products;
using System.Diagnostics.CodeAnalysis;

namespace ApplicationCore.Features.ClosetProCSVCutList.EqualityComparers;

public class VerticalPanelComparer : IEqualityComparer<VerticalPanel> {
    public bool Equals(VerticalPanel? x, VerticalPanel? y) {

        if (x is null && y is null) return true;
        if (x is not null && y is null) return false;
        if (y is not null && x is null) return false;

        if (x!.Room != y!.Room
            || x.UnitPrice != y.UnitPrice
            || x.Height != y.Height
            || x.Depth != y.Depth
            || x.WallHung != y.WallHung
            || x.ExtendBack != y.ExtendBack
            || x.HasBottomRadius != y.HasBottomRadius
            || x.BaseNotch != y.BaseNotch
            || x.Color != y.Color
            || x.EdgeBandingColor != y.EdgeBandingColor
            || x.Drilling != y.Drilling) {
            return false;
        }

        return true;

    }

    public int GetHashCode([DisallowNull] VerticalPanel obj) {

        var a = HashCode.Combine(obj.Room,
                                obj.UnitPrice,
                                obj.Height,
                                obj.Depth,
                                obj.Color,
                                obj.EdgeBandingColor);

        var b = HashCode.Combine(obj.Drilling,
                                 obj.WallHung,
                                 obj.ExtendBack,
                                 obj.HasBottomRadius,
                                 obj.BaseNotch);

        return HashCode.Combine(a, b);

    }

}
