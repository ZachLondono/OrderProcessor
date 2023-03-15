namespace ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.JobSummary;

internal class ClosetPartGroup {

    public string Room { get; set; } = string.Empty;
    public string MaterialFinish { get; set; } = string.Empty;
    public string MaterialCore { get; set; } = string.Empty;
    public string EdgeBandingMaterial { get; set; } = string.Empty;
    public string EdgeBandingFinish { get; set; } = string.Empty;

    public List<ClosetPartItem> Items { get; set; } = new();

    public class ClosetPartGroupComparer : IEqualityComparer<ClosetPartGroup> {

        bool IEqualityComparer<ClosetPartGroup>.Equals(ClosetPartGroup? x, ClosetPartGroup? y) {
            
            if (x is null && y is null) return true;
            if (x is null || y is null) return false;

            return x.Room == y.Room
                && x.MaterialFinish == y.MaterialFinish
                && x.MaterialCore == y.MaterialCore
                && x.EdgeBandingMaterial == y.EdgeBandingMaterial
                && x.EdgeBandingFinish == y.EdgeBandingFinish;

        }

        int IEqualityComparer<ClosetPartGroup>.GetHashCode(ClosetPartGroup obj) {
            return HashCode.Combine(obj.Room.GetHashCode(), obj.MaterialFinish.GetHashCode(), obj.MaterialCore.GetHashCode(), obj.EdgeBandingMaterial.GetHashCode(), obj.EdgeBandingFinish.GetHashCode());
        }

    }

}
