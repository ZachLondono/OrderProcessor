using OrderLoading.ClosetProCSVCutList.Products.Fronts;
using System.Diagnostics.CodeAnalysis;

namespace OrderLoading.ClosetProCSVCutList.EqualityComparers;

public class MDFFrontComparer : IEqualityComparer<MDFFront> {
	public bool Equals(MDFFront? x, MDFFront? y) {

		if (x is null && y is null) return true;
		if (x is not null && y is null) return false;
		if (y is not null && x is null) return false;

		if (x!.Room != y!.Room
			|| x.UnitPrice != y.UnitPrice
			|| x.Height != y.Height
			|| x.Width != y.Width
			|| x.Frame != y.Frame
			|| x.Style != y.Style
			|| x.Type != y.Type
			|| x.HardwareSpread != y.HardwareSpread) {
			return false;
		}

		return true;

	}

	public int GetHashCode([DisallowNull] MDFFront obj) {

		return HashCode.Combine(obj.Room,
								obj.UnitPrice,
								obj.Height,
								obj.Width,
								obj.Frame,
								obj.Style,
								obj.Type,
								obj.HardwareSpread);

	}

}
