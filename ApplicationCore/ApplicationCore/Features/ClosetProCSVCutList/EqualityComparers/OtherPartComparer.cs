using ApplicationCore.Features.ClosetProCSVCutList.Products;
using System.Diagnostics.CodeAnalysis;

namespace ApplicationCore.Features.ClosetProCSVCutList.EqualityComparers;

public class OtherPartComparer : IEqualityComparer<OtherPart> {
    public bool Equals(OtherPart? x, OtherPart? y) {
        throw new NotImplementedException();
    }

    public int GetHashCode([DisallowNull] OtherPart obj) {
        throw new NotImplementedException();
    }
}
