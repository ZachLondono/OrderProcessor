using ApplicationCore.Features.ClosetProCSVCutList.Products;
using System.Diagnostics.CodeAnalysis;

namespace ApplicationCore.Features.ClosetProCSVCutList.EqualityComparers;

public class ZargenDrawerBoxComparer : IEqualityComparer<ZargenDrawerBox> {
    public bool Equals(ZargenDrawerBox? x, ZargenDrawerBox? y) {
        throw new NotImplementedException();
    }

    public int GetHashCode([DisallowNull] ZargenDrawerBox obj) {
        throw new NotImplementedException();
    }
}
