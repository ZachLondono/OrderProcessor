using OrderLoading.ClosetProCSVCutList.Products;
using System.Diagnostics.CodeAnalysis;

namespace OrderLoading.ClosetProCSVCutList.EqualityComparers;

public class HutchVerticalPanelComparer : IEqualityComparer<HutchVerticalPanel> {

    public bool Equals(HutchVerticalPanel? x, HutchVerticalPanel? y) {

        if (x is null && y is null) return true;
        if (x is not null && y is null) return false;
        if (y is not null && x is null) return false;

        if (x!.Room != y!.Room
            || x.UnitPrice != y.UnitPrice
            || x.PanelHeight != y.PanelHeight
            || x.BottomHeight != y.BottomHeight
            || x.TopDepth != y.TopDepth
            || x.BottomDepth != y.BottomDepth
            || x.Color != y.Color
            || x.EdgeBandingColor != y.EdgeBandingColor
            || x.WallHung != y.WallHung
            || x.ExtendBack != y.ExtendBack
            || x.HasBottomRadius != y.HasBottomRadius
            || x.Drilling != y.Drilling) {
            return false;
        }

        return true;

    }

    public int GetHashCode([DisallowNull] HutchVerticalPanel obj) {

        var a = HashCode.Combine(obj.Room,
                                obj.UnitPrice,
                                obj.PanelHeight,
                                obj.BottomHeight,
                                obj.TopDepth,
                                obj.BottomDepth,
                                obj.Color);

        var b = HashCode.Combine(obj.EdgeBandingColor,
                                obj.WallHung,
                                obj.ExtendBack,
                                obj.HasBottomRadius,
                                obj.Drilling);

        return HashCode.Combine(a, b);

    }

}
