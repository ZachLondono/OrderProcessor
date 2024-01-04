using ApplicationCore.Features.ClosetProCSVCutList.Products;
using System.Diagnostics.CodeAnalysis;

namespace ApplicationCore.Features.ClosetProCSVCutList.EqualityComparers;

public class FivePieceFrontComparer : IEqualityComparer<FivePieceFront> {
    public bool Equals(FivePieceFront? x, FivePieceFront? y) {

        if (x is null && y is null) return true;
        if (x is not null && y is null) return false;
        if (y is not null && x is null) return false;

        if (x!.Room != y!.Room
            || x.UnitPrice != y.UnitPrice
            || x.Width != y.Width
            || x.Height != y.Height
            || x.Color != y.Color
            || x.Frame != y.Frame
            || x.HardwareSpread != y.HardwareSpread
            || x.Type != y.Type) {
            return false;
        }

        return true;

    }

    public int GetHashCode([DisallowNull] FivePieceFront obj) {
        return HashCode.Combine(obj.Room,
                                obj.UnitPrice,
                                obj.Width,
                                obj.Height,
                                obj.Frame,
                                obj.Color,
                                obj.HardwareSpread,
                                obj.Type);
    }

}
