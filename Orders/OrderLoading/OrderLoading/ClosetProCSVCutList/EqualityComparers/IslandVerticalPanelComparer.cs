using OrderLoading.ClosetProCSVCutList.Products.Verticals;
using System.Diagnostics.CodeAnalysis;

namespace OrderLoading.ClosetProCSVCutList.EqualityComparers;

public class IslandVerticalPanelComparer : IEqualityComparer<IslandVerticalPanel> {

	public bool Equals(IslandVerticalPanel? x, IslandVerticalPanel? y) {

		if (x is null && y is null) return true;
		if (x is not null && y is null) return false;
		if (y is not null && x is null) return false;

		if (x!.Room != y!.Room
			|| x.UnitPrice != y.UnitPrice
			|| x.Height != y.Height
			|| x.PanelDepth != y.PanelDepth
			|| x.Side1Depth != y.Side1Depth
			|| x.Color != y.Color
			|| x.EdgeBandingColor != y.EdgeBandingColor
			|| x.Drilling != y.Drilling) {
			return false;
		}

		return true;

	}

	public int GetHashCode([DisallowNull] IslandVerticalPanel obj) {

		var a = HashCode.Combine(obj.Room,
								obj.UnitPrice,
								obj.Height,
								obj.PanelDepth,
								obj.Side1Depth,
								obj.Color,
								obj.EdgeBandingColor);

		var b = HashCode.Combine(obj.Drilling);

		return HashCode.Combine(a, b);

	}

}
