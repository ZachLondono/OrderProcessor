namespace OrderExporting.JobSummary;

public class ZargenDrawerGroup {

    // TODO: ADD Paint to group key
    public string Room { get; set; } = string.Empty;
    public string MaterialFinish { get; set; } = string.Empty;
    public string MaterialCore { get; set; } = string.Empty;
    public string EdgeBandingFinish { get; set; } = string.Empty;

    public List<ZargenDrawerItem> Items { get; set; } = new();

    public class ZargenDrawerGroupComparer : IEqualityComparer<ZargenDrawerGroup> {

        bool IEqualityComparer<ZargenDrawerGroup>.Equals(ZargenDrawerGroup? x, ZargenDrawerGroup? y) {

            if (x is null && y is null) return true;
            if (x is null || y is null) return false;

            return x.Room == y.Room
                && x.MaterialFinish == y.MaterialFinish
                && x.MaterialCore == y.MaterialCore
                && x.EdgeBandingFinish == y.EdgeBandingFinish;

        }

        int IEqualityComparer<ZargenDrawerGroup>.GetHashCode(ZargenDrawerGroup obj) {
            return HashCode.Combine(obj.Room, obj.MaterialFinish, obj.MaterialCore, obj.EdgeBandingFinish);
        }

    }

}