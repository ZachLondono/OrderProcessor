namespace OrderExporting.JobSummary;

public class CabinetPartGroup {

    public string Room { get; set; } = string.Empty;
    public string MaterialFinish { get; set; } = string.Empty;
    public string MaterialCore { get; set; } = string.Empty;
    public string EdgeBandingFinish { get; set; } = string.Empty;

    public List<CabinetPartItem> Items { get; set; } = new();

    public class CabinetPartGroupComparer : IEqualityComparer<CabinetPartGroup> {

        bool IEqualityComparer<CabinetPartGroup>.Equals(CabinetPartGroup? x, CabinetPartGroup? y) {

            if (x is null && y is null) return true;
            if (x is null || y is null) return false;

            return x.Room == y.Room
                && x.MaterialFinish == y.MaterialFinish
                && x.MaterialCore == y.MaterialCore
                && x.EdgeBandingFinish == y.EdgeBandingFinish;

        }

        int IEqualityComparer<CabinetPartGroup>.GetHashCode(CabinetPartGroup obj) {
            return HashCode.Combine(obj.Room, obj.MaterialFinish, obj.MaterialCore, obj.EdgeBandingFinish);
        }

    }

}
