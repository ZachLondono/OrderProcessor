using ApplicationCore.Features.ClosetProCSVCutList.Products;
using System.Diagnostics.CodeAnalysis;

namespace ApplicationCore.Features.ClosetProCSVCutList.EqualityComparers;

public class DrawerBoxComparer : IEqualityComparer<DrawerBox> {

    public bool Equals(DrawerBox? x, DrawerBox? y) {

        if (x is null && y is null) return true;
        if (x is not null && y is null) return false;
        if (y is not null && x is null) return false;

        if (x!.Room != y!.Room
            || x.UnitPrice != y.UnitPrice
            || x.Height != y.Height
            || x.Width != y.Width
            || x.Depth != y.Depth
            || x.ScoopFront != y.ScoopFront
            || x.UnderMountNotches != y.UnderMountNotches
            || x.Type != y.Type) {
            return false;
        }

        return true;

    }

    public int GetHashCode([DisallowNull] DrawerBox obj) {
        return HashCode.Combine(obj.Room,
                                obj.UnitPrice,
                                obj.Height,
                                obj.Width,
                                obj.Depth,
                                obj.ScoopFront,
                                obj.UnderMountNotches,
                                obj.Type);
    }

}