using OrderLoading.ClosetProCSVCutList.Products.Verticals;
using System.Diagnostics.CodeAnalysis;

namespace OrderLoading.ClosetProCSVCutList.EqualityComparers;

public class DividerVerticalPanelComparer : IEqualityComparer<DividerVerticalPanel> {

	public bool Equals(DividerVerticalPanel? x, DividerVerticalPanel? y) {

		if (x is null && y is null) return true;
		if (x is not null && y is null) return false;
		if (y is not null && x is null) return false;

		if (x!.Room != y!.Room
			|| x.UnitPrice != y.UnitPrice
			|| x.Height != y.Height
			|| x.Depth != y.Depth
			|| x.Color != y.Color
			|| x.EdgeBandingColor != y.EdgeBandingColor
			|| x.Drilling != y.Drilling) {
			return false;
		}

		return true;

	}

	public int GetHashCode([DisallowNull] DividerVerticalPanel obj) {
		return HashCode.Combine(obj.Room,
								obj.UnitPrice,
								obj.Height,
								obj.Depth,
								obj.Color,
								obj.EdgeBandingColor,
								obj.Drilling);
	}

}
